﻿using Application.Abstractions;
using Application.Services;
using Contracts.Services.Catalog;
using Domain.Aggregates;

namespace Application.UseCases.Commands;

public interface IChangeCatalogTitleInteractor : IInteractor<Command.ChangeCatalogTitle> { }

public class ChangeCatalogTitleInteractor : IChangeCatalogTitleInteractor
{
    private readonly IApplicationService _applicationService;

    public ChangeCatalogTitleInteractor(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    public async Task InteractAsync(Command.ChangeCatalogTitle command, CancellationToken cancellationToken)
    {
        var catalog = await _applicationService.LoadAggregateAsync<Catalog>(command.CatalogId, cancellationToken);
        catalog.Handle(command);
        await _applicationService.AppendEventsAsync(catalog, cancellationToken);
    }
}