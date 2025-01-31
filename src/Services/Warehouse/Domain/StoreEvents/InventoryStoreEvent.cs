﻿using Domain.Abstractions.StoreEvents;
using Domain.Aggregates;

namespace Domain.StoreEvents;

public record InventoryStoreEvent : StoreEvent<Guid, Inventory>;