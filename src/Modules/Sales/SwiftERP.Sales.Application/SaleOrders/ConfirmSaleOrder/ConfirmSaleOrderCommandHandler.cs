using MediatR;
using SwiftERP.Finance.Domain.LedgerEntries;
using SwiftERP.Sales.Application.Abstractions;
using SwiftERP.Sales.Domain.SaleOrders;
using SwiftERP.Sales.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.Sales.Application.SaleOrders.ConfirmSaleOrder;

/// <summary>
/// The project's core cross-module workflow: confirming a sale must decrement Inventory stock
/// and post a Finance ledger entry together, or not at all. Both <see cref="ISalesInventoryPort"/>
/// and <see cref="ISalesLedgerPort"/> are implemented against the same SalesDbContext instance as
/// <see cref="ISaleOrderRepository"/> (see Sales.Infrastructure DI wiring), so the single
/// <see cref="IUnitOfWork.SaveChangesAsync"/> call below commits all three changes — stock
/// decrement, ledger entry, order confirmation — in one database transaction. If any product has
/// insufficient stock, DecrementStock throws before anything is saved, so nothing partially commits.
/// </summary>
public class ConfirmSaleOrderCommandHandler(
    ISaleOrderRepository saleOrderRepository,
    ISalesInventoryPort inventoryPort,
    ISalesLedgerPort ledgerPort,
    IUnitOfWork unitOfWork) : IRequestHandler<ConfirmSaleOrderCommand, Result>
{
    public async Task<Result> Handle(ConfirmSaleOrderCommand request, CancellationToken cancellationToken)
    {
        var saleOrder = await saleOrderRepository.GetByIdAsync(request.SaleOrderId, cancellationToken);
        if (saleOrder is null)
            return Result.Failure($"Sale order '{request.SaleOrderId}' was not found.");

        foreach (var line in saleOrder.Lines)
        {
            var product = await inventoryPort.GetProductByIdAsync(line.ProductId, cancellationToken);
            if (product is null)
                return Result.Failure($"Product '{line.ProductId}' referenced by sale order line was not found.");

            product.DecrementStock(line.Quantity);
        }

        saleOrder.Confirm();

        var ledgerEntry = new LedgerEntry(
            LedgerEntryType.SaleRevenue,
            saleOrder.Total,
            $"Sale order {saleOrder.Id} confirmed",
            saleOrder.Id);
        ledgerPort.AddEntry(ledgerEntry);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
