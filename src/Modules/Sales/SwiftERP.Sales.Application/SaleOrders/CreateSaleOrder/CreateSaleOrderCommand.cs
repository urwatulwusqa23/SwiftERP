using MediatR;
using SwiftERP.SharedKernel;

namespace SwiftERP.Sales.Application.SaleOrders.CreateSaleOrder;

public record CreateSaleOrderLineDto(Guid ProductId, int Quantity, decimal UnitPrice);

public record CreateSaleOrderCommand(
    Guid CustomerId,
    List<CreateSaleOrderLineDto> Lines) : IRequest<Result<Guid>>;
