﻿using Application.EventSourcing.EventStore.Events;
using Domain.Entities.ShoppingCarts;
using JsonNet.ContractResolvers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace Infrastructure.EventSourcing.Accounts.EventStore.Configurations
{
    public class AccountSnapshotConfiguration : IEntityTypeConfiguration<AccountSnapshot>
    {
        public void Configure(EntityTypeBuilder<AccountSnapshot> builder)
        {
            builder.HasKey(snapshot => new { snapshot.AggregateVersion, snapshot.AggregateId });

            builder
                .Property(storeEvent => storeEvent.AggregateVersion)
                .IsRequired();

            builder
                .Property(snapshot => snapshot.AggregateId)
                .IsRequired();

            builder
                .Property(snapshot => snapshot.AggregateName)
                .HasMaxLength(30)
                .IsUnicode(false)
                .IsRequired();

            builder
                .Property(snapshot => snapshot.AggregateState)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasConversion(
                    account => JsonConvert.SerializeObject(account),
                    jsonString => JsonConvert.DeserializeObject<ShoppingCart>(jsonString,
                        new JsonSerializerSettings { ContractResolver = new PrivateSetterContractResolver() }))
                .IsRequired();
        }
    }
}