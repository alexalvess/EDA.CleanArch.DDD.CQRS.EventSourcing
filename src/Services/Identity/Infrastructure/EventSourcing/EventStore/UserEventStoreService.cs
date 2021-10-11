using System;
using Application.EventSourcing.EventStore;
using Application.EventSourcing.EventStore.Events;
using Domain.Aggregates;
using Infrastructure.Abstractions.EventSourcing.EventStore;
using Infrastructure.DependencyInjection.Options;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.EventSourcing.EventStore
{
    public class UserEventStoreService : EventStoreService<User, UserStoreEvent, UserSnapshot, Guid>, IUserEventStoreService
    {
        public UserEventStoreService(ILogger<UserEventStoreService> logger, IOptionsMonitor<EventStoreOptions> optionsMonitor, IUserEventStoreRepository repository, IBus bus)
            : base(logger, optionsMonitor, repository, bus) { }
    }
}