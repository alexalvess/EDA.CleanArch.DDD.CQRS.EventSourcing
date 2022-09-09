using Application.Abstractions.Interactors;
using Application.UseCases.Commands;
using Application.UseCases.Events;
using Contracts.Services.Identity;
using Microsoft.Extensions.DependencyInjection;
using DomainEvent = Contracts.Services.Account.DomainEvent;

namespace Application.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommandInteractors(this IServiceCollection services)
        => services
            .AddScoped<IInteractor<Command.RegisterUser>, RegisterUserInteractor>()
            .AddScoped<IInteractor<Command.ChangePassword>, ChangePasswordInteractor>();

    public static IServiceCollection AddEventInteractors(this IServiceCollection services)
        => services
            .AddScoped<IInteractor<DomainEvent.AccountDeactivated>, DeactivateUserWhenAccountDeactivatedInteractor>()
            .AddScoped<IInteractor<DomainEvent.AccountDeleted>, DeleteUserWhenAccountDeletedInteractor>();
}