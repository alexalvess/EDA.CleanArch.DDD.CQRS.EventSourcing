﻿using Application.Abstractions.Services;
using Application.Services.PayPal;
using Application.Services.PayPal.Http;
using Domain.Entities.PaymentMethods;

namespace Infrastructure.HttpClients.PayPal;

public class PayPalPaymentService : PaymentService, IPayPalPaymentService
{
    private readonly IPayPalHttpClient _client;

    public PayPalPaymentService(IPayPalHttpClient client)
    {
        _client = client;
    }

    public override Task<IPaymentResult> HandleAsync(Func<IPaymentService, IPaymentMethod, CancellationToken, Task<IPaymentResult>> behaviorProcessor, IPaymentMethod method,
            CancellationToken cancellationToken)
        // TODO - Review namespace
        => method is Domain.Entities.PaymentMethods.PayPal.PayPal payPalPaymentMethod
            ? behaviorProcessor(this, payPalPaymentMethod, cancellationToken)
            : base.HandleAsync(behaviorProcessor, method, cancellationToken);

    public override async Task<IPaymentResult> AuthorizeAsync(IPaymentMethod method, CancellationToken cancellationToken)
    {
        Requests.PaypalAuthorizePayment request = new()
        {
            // TODO - Use method to hydrate  
        };

        var response = await _client.AuthorizeAsync(request, cancellationToken);
        return response.ActionResult;
    }

    public override async Task<IPaymentResult> CancelAsync(IPaymentMethod method, CancellationToken cancellationToken)
    {
        Requests.PaypalCancelPayment request = new()
        {
            // TODO - Use method to hydrate  
        };

        var response = await _client.CancelAsync(Guid.NewGuid(), request, cancellationToken);
        return response.ActionResult;
    }

    public override async Task<IPaymentResult> RefundAsync(IPaymentMethod method, CancellationToken cancellationToken)
    {
        Requests.PaypalRefundPayment request = new()
        {
            // TODO - Use method to hydrate  
        };

        var response = await _client.RefundAsync(Guid.NewGuid(), request, cancellationToken);
        return response.ActionResult;
    }
}