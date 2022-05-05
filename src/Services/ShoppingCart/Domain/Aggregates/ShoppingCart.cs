﻿using Domain.Abstractions.Aggregates;
using Domain.Entities.CartItems;
using Domain.Entities.Customers;
using Domain.Enumerations;
using Contracts.Abstractions;
using Contracts.Services.ShoppingCart;
using Domain.Entities.PaymentMethods;
using Domain.ValueObjects.PaymentOptions.CreditCards;
using Domain.ValueObjects.PaymentOptions.DebitCards;
using Domain.ValueObjects.PaymentOptions.PayPals;

namespace Domain.Aggregates;

public class ShoppingCart : AggregateRoot<Guid, ShoppingCartValidator>
{
    private readonly List<CartItem> _items = new();
    private readonly List<PaymentMethod> _paymentMethods = new();

    public ShoppingCartStatus Status { get; private set; }
    public Customer Customer { get; private set; }

    public decimal Total
        => Items.Sum(item => item.Product.UnitPrice * item.Quantity);

    public IEnumerable<CartItem> Items
        => _items;

    public IEnumerable<PaymentMethod> PaymentMethods
        => _paymentMethods;

    public void Handle(Command.CreateCart cmd)
        => RaiseEvent(new DomainEvent.CartCreated(Guid.NewGuid(), cmd.CustomerId, ShoppingCartStatus.Confirmed.ToString()));

    public void Handle(Command.AddCartItem cmd)
    {
        var item = _items.SingleOrDefault(item => item.Product.Id == cmd.Product.Id);

        RaiseEvent(item is null
            ? new DomainEvent.CartItemAdded(cmd.CartId, Guid.NewGuid(), cmd.Product, cmd.Quantity)
            : new DomainEvent.CartItemIncreased(cmd.CartId, item.Id, item.Product.UnitPrice));
    }

    public void Handle(Command.IncreaseCartItem cmd)
    {
        if (_items.SingleOrDefault(cartItem => cartItem.Id == cmd.ItemId) is {IsDeleted: false} item)
            RaiseEvent(new DomainEvent.CartItemIncreased(cmd.CartId, cmd.ItemId, item.Product.UnitPrice));
    }

    public void Handle(Command.DecreaseCartItem cmd)
    {
        if (_items.SingleOrDefault(cartItem => cartItem.Id == cmd.ItemId) is {IsDeleted: false} item)
            RaiseEvent(new DomainEvent.CartItemDecreased(cmd.CartId, cmd.ItemId, item.Product.UnitPrice));
    }

    public void Handle(Command.RemoveCartItem cmd)
    {
        if (_items.SingleOrDefault(cartItem => cartItem.Id == cmd.ItemId) is {IsDeleted: false} item)
            RaiseEvent(new DomainEvent.CartItemRemoved(cmd.CartId, cmd.ItemId, item.Product.UnitPrice, item.Quantity));
    }

    public void Handle(Command.AddCreditCard cmd)
        => RaiseEvent(new DomainEvent.CreditCardAdded(cmd.CartId, Guid.NewGuid(), cmd.Amount, cmd.CreditCard));

    public void Handle(Command.AddDebitCard cmd)
        => RaiseEvent(new DomainEvent.DebitCardAdded(cmd.CartId, Guid.NewGuid(), cmd.Amount, cmd.DebitCard));

    public void Handle(Command.AddPayPal cmd)
        => RaiseEvent(new DomainEvent.PayPalAdded(cmd.CartId, Guid.NewGuid(), cmd.Amount, cmd.PayPal));

    public void Handle(Command.AddShippingAddress cmd)
        => RaiseEvent(new DomainEvent.ShippingAddressAdded(cmd.CartId, cmd.Address));

    public void Handle(Command.ChangeBillingAddress cmd)
        => RaiseEvent(new DomainEvent.BillingAddressChanged(cmd.CartId, cmd.Address));

    public void Handle(Command.CheckOutCart cmd)
        => RaiseEvent(new DomainEvent.CartCheckedOut(cmd.CartId));

    public void Handle(Command.DiscardCart cmd)
        => RaiseEvent(new DomainEvent.CartDiscarded(cmd.CartId));

    protected override void ApplyEvent(IEvent @event)
        => When(@event as dynamic);

    private void When(DomainEvent.CartCreated @event)
    {
        Id = @event.CartId;
        Customer = new(@event.CustomerId);
        Status = ShoppingCartStatus.FromName(@event.Status);
    }

    private void When(DomainEvent.CartCheckedOut _)
        => Status = ShoppingCartStatus.CheckedOut;

    private void When(DomainEvent.CartDiscarded _)
        => IsDeleted = true;

    private void When(DomainEvent.CartItemIncreased @event)
        => _items.Single(item => item.Id == @event.ItemId).Increase();

    private void When(DomainEvent.CartItemDecreased @event)
        => _items.Single(item => item.Id == @event.ItemId).Decrease();

    private void When(DomainEvent.CartItemRemoved @event)
        => _items.RemoveAll(item => item.Id == @event.ItemId);

    private void When(DomainEvent.CartItemAdded @event)
        => _items.Add(new(@event.ItemId, @event.Product, @event.Quantity));

    private void When(DomainEvent.CreditCardAdded @event)
        => _paymentMethods.Add(new(@event.MethodId, @event.Amount, (CreditCard) @event.CreditCard));

    private void When(DomainEvent.DebitCardAdded @event)
        => _paymentMethods.Add(new(@event.MethodId, @event.Amount, (DebitCard) @event.DebitCard));

    private void When(DomainEvent.PayPalAdded @event)
        => _paymentMethods.Add(new(@event.MethodId, @event.Amount, (PayPal) @event.PayPal));

    private void When(DomainEvent.ShippingAddressAdded @event)
        => Customer.SetShippingAddress(@event.Address);

    private void When(DomainEvent.BillingAddressChanged @event)
        => Customer.SetBillingAddress(@event.Address);
}