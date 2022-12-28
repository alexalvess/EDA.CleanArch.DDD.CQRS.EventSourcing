using Contracts.Abstractions;
using Contracts.Abstractions.Messages;
using Contracts.Abstractions.Paging;
using Grpc.Core;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using static Microsoft.AspNetCore.Http.TypedResults;


namespace WebAPI.Abstractions;

public static class ApplicationApi
{
    public static async Task<Results<Ok<TResponse>, ValidationProblem>> QueryAsync<TClient, TResponse>(IQueryRequest<TClient> request,
        Func<TClient, CancellationToken, AsyncUnaryCall<TResponse>> query)
        where TClient : ClientBase<TClient>
        => request.IsValid(out var errors) ? Ok(await query(request.Client, request.CancellationToken)) : ValidationProblem(errors);

    public static async Task<Results<Ok<TResponse>, NotFound, ValidationProblem, Problem>> GetAsync<TClient, TResponse>
        (IQueryRequest<TClient> request, Func<TClient, CancellationToken, AsyncUnaryCall<Contracts.Abstractions.Protobuf.GetResponse>> query)
        where TClient : ClientBase<TClient>
        where TResponse : Google.Protobuf.IMessage, new()
    {
        return request.IsValid(out var errors) ? await ResponseAsync() : ValidationProblem(errors);

        async Task<Results<Ok<TResponse>, NotFound, ValidationProblem, Problem>> ResponseAsync()
        {
            var response = await query(request.Client, request.CancellationToken);

            return response.OneOfCase switch
            {
                Contracts.Abstractions.Protobuf.GetResponse.OneOfOneofCase.NotFound => NotFound(),
                Contracts.Abstractions.Protobuf.GetResponse.OneOfOneofCase.Projection when response.Projection.TryUnpack<TResponse>(out var result) => Ok(result),
                _ => new Problem()
            };
        }
    }

    public static async Task<Results<Ok<PagedResult<TResponse>>, NoContent, ValidationProblem, Problem>> ListAsync<TClient, TResponse>
        (IQueryRequest<TClient> request, Func<TClient, CancellationToken, AsyncUnaryCall<Contracts.Abstractions.Protobuf.ListResponse>> query)
        where TClient : ClientBase<TClient>
        where TResponse : Google.Protobuf.IMessage, new()
    {
        return request.IsValid(out var errors) ? await ResponseAsync() : ValidationProblem(errors);

        async Task<Results<Ok<PagedResult<TResponse>>, NoContent, ValidationProblem, Problem>> ResponseAsync()
        {
            var response = await query(request.Client, request.CancellationToken);

            return response.OneOfCase switch
            {
                Contracts.Abstractions.Protobuf.ListResponse.OneOfOneofCase.NoContent => NoContent(),
                Contracts.Abstractions.Protobuf.ListResponse.OneOfOneofCase.PagedResult  => Ok<PagedResult<TResponse>>(response.PagedResult),
                _ => new Problem()
            };
        }
    }

    public static async Task<Results<Accepted, ValidationProblem>> SendCommandAsync<TCommand>(ICommandRequest request)
        where TCommand : class, ICommand
    {
        return request.IsValid(out var errors) ? await AcceptAsync() : ValidationProblem(errors);

        async Task<Accepted> AcceptAsync()
        {
            var endpoint = await request.Bus.GetSendEndpoint(Address<TCommand>());
            await endpoint.Send((TCommand)request.Command, request.CancellationToken);
            return Accepted("");
        }
    }

    private static Uri Address<T>()
        => new($"exchange:{KebabCaseEndpointNameFormatter.Instance.SanitizeName(typeof(T).Name)}");

    public static Task<Results<Ok<TProjection>, NoContent, NotFound, Problem>> GetProjectionAsync<TQuery, TProjection>
        (IBus bus, TQuery query, CancellationToken cancellationToken)
        where TQuery : class, IQuery
        where TProjection : class, IProjection
        => GetResponseAsync<TQuery, TProjection>(bus, query, cancellationToken);

    public static Task<Results<Ok<IPagedResult<TProjection>>, NoContent, NotFound, Problem>> GetPagedProjectionAsync<TQuery, TProjection>
        (IBus bus, TQuery query, CancellationToken cancellationToken)
        where TQuery : class, IQuery
        where TProjection : class, IProjection
        => GetResponseAsync<TQuery, IPagedResult<TProjection>>(bus, query, cancellationToken);

    private static async Task<Results<Ok<TProjection>, NoContent, NotFound, Problem>> GetResponseAsync<TQuery, TProjection>
        (IBus bus, TQuery query, CancellationToken cancellationToken)
        where TQuery : class, IQuery
        where TProjection : class
    {
        var response = await bus
            .CreateRequestClient<TQuery>(Address<TQuery>())
            .GetResponse<TProjection, Reply.NoContent, Reply.NotFound>(query, cancellationToken);

        return response.Message switch
        {
            TProjection projection => Ok(projection),
            Reply.NoContent => NoContent(),
            Reply.NotFound => NotFound(),
            _ => new Problem()
        };
    }
}