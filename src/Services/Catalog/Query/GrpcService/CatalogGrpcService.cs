using Application.Abstractions;
using Contracts.Abstractions.Paging;
using Contracts.Services.Catalog;
using Contracts.Services.Catalog.Protobuf;
using Grpc.Core;

namespace GrpcService;

public class CatalogGrpcService : CatalogService.CatalogServiceBase
{
    private readonly IInteractor<Query.GetCatalogItemDetails, Projection.CatalogItemDetails> _getCatalogItemDetailsInteractor;
    private readonly IInteractor<Query.ListCatalogItemsCards, IPagedResult<Projection.CatalogItemCard>> _listCatalogItemsCardsInteractor;
    private readonly IInteractor<Query.ListCatalogItemsListItems, IPagedResult<Projection.CatalogItemListItem>> _listCatalogItemsListItemsInteractor;
    private readonly IInteractor<Query.ListCatalogsGridItems, IPagedResult<Projection.CatalogGridItem>> _listCatalogsGridItemsInteractor;

    public CatalogGrpcService(
        IInteractor<Query.GetCatalogItemDetails, Projection.CatalogItemDetails> getCatalogItemDetailsInteractor,
        IInteractor<Query.ListCatalogItemsCards, IPagedResult<Projection.CatalogItemCard>> listCatalogItemsCardsInteractor,
        IInteractor<Query.ListCatalogItemsListItems, IPagedResult<Projection.CatalogItemListItem>> listCatalogItemsListItemsInteractor,
        IInteractor<Query.ListCatalogsGridItems, IPagedResult<Projection.CatalogGridItem>> listCatalogsGridItemsInteractor)
    {
        _getCatalogItemDetailsInteractor = getCatalogItemDetailsInteractor;
        _listCatalogItemsCardsInteractor = listCatalogItemsCardsInteractor;
        _listCatalogItemsListItemsInteractor = listCatalogItemsListItemsInteractor;
        _listCatalogsGridItemsInteractor = listCatalogsGridItemsInteractor;
    }

    public override async Task<CatalogItemDetailsResponse> GetCatalogItemDetails(GetCatalogItemDetailsRequest request, ServerCallContext context)
    {
        var itemDetails = await _getCatalogItemDetailsInteractor.InteractAsync(request, context.CancellationToken);
        return itemDetails is null ? new() { NotFound = new() } : new() { CatalogItemDetails = itemDetails };
    }

    public override async Task<CatalogsGridItemsPagedResponse> ListCatalogsGridItems(ListCatalogsGridItemsRequest request, ServerCallContext context)
    {
        var pagedResult = await _listCatalogsGridItemsInteractor.InteractAsync(request, context.CancellationToken);

        return pagedResult!.Items.Any()
            ? new()
            {
                CatalogGridItems =
                {
                    Items = { pagedResult.Items.Select(gridItem => (CatalogGridItem)gridItem) },
                    Page = pagedResult.Page
                }
            }
            : new() { NoContent = new() };
    }

    public override async Task<CatalogItemsListItemsPagedResponse> ListCatalogItemsListItems(ListCatalogItemsListItemsRequest request, ServerCallContext context)
    {
        var pagedResult = await _listCatalogItemsListItemsInteractor.InteractAsync(request, context.CancellationToken);

        return pagedResult!.Items.Any()
            ? new()
            {
                CatalogListItems =
                {
                    Items = { pagedResult.Items.Select(gridItem => (CatalogItemListItem)gridItem) },
                    Page = pagedResult.Page
                }
            }
            : new() { NoContent = new() };
    }

    public override async Task<CatalogItemsCardsPagedResponse> ListCatalogItemsCards(ListCatalogItemsCardsRequest request, ServerCallContext context)
    {
        var pagedResult = await _listCatalogItemsCardsInteractor.InteractAsync(request, context.CancellationToken);

        return pagedResult!.Items.Any()
            ? new()
            {
                CatalogItemCards =
                {
                    Items = { pagedResult.Items.Select(gridItem => (CatalogItemCard)gridItem) },
                    Page = pagedResult.Page
                }
            }
            : new() { NoContent = new() };
    }
}