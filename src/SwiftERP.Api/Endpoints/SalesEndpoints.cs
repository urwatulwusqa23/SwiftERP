using MediatR;
using SwiftERP.Api.Authorization;
using SwiftERP.Identity.Domain.Roles;
using SwiftERP.Sales.Application.SaleOrders.ConfirmSaleOrder;
using SwiftERP.Sales.Application.SaleOrders.CreateSaleOrder;
using SwiftERP.Sales.Application.SaleOrders.GetSaleOrder;
using SwiftERP.Sales.Application.SaleOrders.GetSaleOrders;
using SwiftERP.Sales.Application.SaleOrders.MarkSaleOrderPaid;

namespace SwiftERP.Api.Endpoints;

public static class SalesEndpoints
{
    public static void MapSalesEndpoints(this IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/api/v1/sales/orders").WithTags("Sales - Orders");

        orders.MapPost("/", async (CreateSaleOrderCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/v1/sales/orders/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        }).RequireModule(Module.Sales, AccessLevel.Edit);

        orders.MapGet("/", async (ISender sender) =>
            Results.Ok(await sender.Send(new GetSaleOrdersQuery())))
            .RequireModule(Module.Sales, AccessLevel.View);

        orders.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var order = await sender.Send(new GetSaleOrderQuery(id));
            return order is not null ? Results.Ok(order) : Results.NotFound();
        }).RequireModule(Module.Sales, AccessLevel.View);

        orders.MapPost("/{id:guid}/confirm", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ConfirmSaleOrderCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { error = result.Error });
        }).RequireModule(Module.Sales, AccessLevel.Edit);

        orders.MapPost("/{id:guid}/mark-paid", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new MarkSaleOrderPaidCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { error = result.Error });
        }).RequireModule(Module.Sales, AccessLevel.Edit);
    }
}
