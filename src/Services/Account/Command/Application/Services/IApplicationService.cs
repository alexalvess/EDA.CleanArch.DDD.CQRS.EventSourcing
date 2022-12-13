﻿using Domain.Abstractions.Aggregates;

namespace Application.Services;

public interface IApplicationService
{
    Task<IAggregateRoot> LoadAggregateAsync<TAggregate>(Guid id, CancellationToken cancellationToken)
        where TAggregate : IAggregateRoot, new();

    Task AppendEventsAsync(IAggregateRoot aggregate, CancellationToken cancellationToken);

    Task StreamReplayAsync(string name, Guid? id, CancellationToken cancellationToken);
}