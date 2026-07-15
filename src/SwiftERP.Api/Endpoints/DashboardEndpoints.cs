using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using SwiftERP.Api.Authorization;
using SwiftERP.Finance.Application.LedgerEntries.GetRunningBalance;
using SwiftERP.HR.Application.Employees.GetActiveEmployeeCount;
using SwiftERP.Identity.Domain.Roles;
using SwiftERP.Inventory.Application.Products.GetLowStockProducts;
using SwiftERP.Sales.Application.SaleOrders.GetDraftSaleOrderCount;

namespace SwiftERP.Api.Endpoints;

public static class DashboardEndpoints
{
    public const string CacheKey = "dashboard:summary";
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        // Short TTL as a self-healing safety net on top of the explicit invalidation triggered
        // by ProductLowStockEvent/PayrollRunPostedEvent — see DashboardCacheInvalidationHandler.
        // Not every write invalidates explicitly (e.g. a stock decrement that doesn't cross the
        // reorder threshold), so the TTL bounds how stale the dashboard can ever get.
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
    };

    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/dashboard", async (HttpContext httpContext, ISender sender, IDistributedCache cache, CancellationToken ct) =>
        {
            DashboardSummary summary;
            var cached = await cache.GetStringAsync(CacheKey, ct);
            if (cached is not null)
            {
                summary = JsonSerializer.Deserialize<DashboardSummary>(cached)!;
            }
            else
            {
                var lowStockProducts = await sender.Send(new GetLowStockProductsQuery(), ct);
                var draftSaleOrderCount = await sender.Send(new GetDraftSaleOrderCountQuery(), ct);
                var runningBalance = await sender.Send(new GetRunningBalanceQuery(), ct);
                var activeEmployeeCount = await sender.Send(new GetActiveEmployeeCountQuery(), ct);

                summary = new DashboardSummary(
                    lowStockProducts.Count,
                    draftSaleOrderCount,
                    runningBalance,
                    activeEmployeeCount);

                await cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(summary), CacheOptions, ct);
            }

            // The cached summary always carries the real finance balance — redact it per-caller
            // at read time instead of maintaining a separate cache entry per permission set, so
            // e.g. an Employee (Finance: None) never sees it even though the underlying cache does.
            var canSeeFinance = RequireModuleAccessFilter.HasAccess(httpContext.User, Module.Finance, AccessLevel.View);
            return Results.Ok(summary with { FinanceRunningBalance = canSeeFinance ? summary.FinanceRunningBalance : null });
        }).WithTags("Dashboard").RequireAuthorization();
    }
}

public record DashboardSummary(
    int LowStockProductCount,
    int DraftSaleOrderCount,
    decimal? FinanceRunningBalance,
    int ActiveEmployeeCount);
