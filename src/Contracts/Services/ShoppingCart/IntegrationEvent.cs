﻿using Contracts.Abstractions.Messages;
using Contracts.DataTransferObjects;

namespace Contracts.Services.ShoppingCart;

public static class IntegrationEvent
{
    public record ProjectionRebuilt(Dto.ShoppingCart Cart, Guid CartId = default) : Message, IEvent;
    
    public record CartSubmitted(Guid CartId, Guid CustomerId, decimal Total, Dto.Address BillingAddress, Dto.Address ShippingAddress, IEnumerable<Dto.CartItem> Items, IEnumerable<Dto.PaymentMethod> PaymentMethods)
        : Message, IEvent;
}