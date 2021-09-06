using System;
using Application.Abstractions.EventSourcing.EventStore;
using Application.EventSourcing.Customers.EventStore.Events;
using Domain.Entities.Customers;

namespace Application.EventSourcing.Customers.EventStore
{
    public interface ICustomerEventStoreRepository : IEventStoreRepository<Customer, CustomerStoreEvent, CustomerSnapshot, Guid> { }
}