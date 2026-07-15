using MediatR;
using SwiftERP.Api.Authorization;
using SwiftERP.Identity.Domain.Roles;
using SwiftERP.Inventory.Application.Products.AdjustStock;
using SwiftERP.Inventory.Application.Products.CreateProduct;
using SwiftERP.Inventory.Application.Products.GetLowStockProducts;
using SwiftERP.Inventory.Application.PurchaseOrders.CreatePurchaseOrder;
using SwiftERP.Inventory.Application.PurchaseOrders.ReceivePurchaseOrder;

namespace SwiftERP.Api.Endpoints;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var products = app.MapGroup("/api/v1/inventory/products").WithTags("Inventory - Products");

        products.MapPost("/", async (CreateProductCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/v1/inventory/products/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        }).RequireModule(Module.Inventory, AccessLevel.Edit);

        products.MapPut("/{id:guid}/stock", async (Guid id, AdjustStockRequest request, ISender sender) =>
        {
            var result = await sender.Send(new AdjustStockCommand(id, request.NewQuantity));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { error = result.Error });
        }).RequireModule(Module.Inventory, AccessLevel.Edit);

        products.MapGet("/low-stock", async (ISender sender) =>
            Results.Ok(await sender.Send(new GetLowStockProductsQuery())))
            .RequireModule(Module.Inventory, AccessLevel.View);

        var purchaseOrders = app.MapGroup("/api/v1/inventory/purchase-orders").WithTags("Inventory - Purchase Orders");

        purchaseOrders.MapPost("/", async (CreatePurchaseOrderCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/v1/inventory/purchase-orders/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        }).RequireModule(Module.Inventory, AccessLevel.Edit);

        purchaseOrders.MapPost("/{id:guid}/receive", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ReceivePurchaseOrderCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { error = result.Error });
        }).RequireModule(Module.Inventory, AccessLevel.Edit);
    }
}

public record AdjustStockRequest(int NewQuantity);
