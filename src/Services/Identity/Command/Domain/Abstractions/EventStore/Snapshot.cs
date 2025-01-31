﻿using Domain.Abstractions.Aggregates;

namespace Domain.Abstractions.EventStore;

public record Snapshot(long AggregateVersion, IAggregateRoot Aggregate)
{
    public string AggregateName { get; } = Aggregate.GetType().Name;
    public Guid AggregateId { get; } = Aggregate.Id;
}