using MediatR;
using SwiftERP.Api.Authorization;
using SwiftERP.Finance.Application.LedgerEntries.GetRunningBalance;
using SwiftERP.Identity.Domain.Roles;

namespace SwiftERP.Api.Endpoints;

public static class FinanceEndpoints
{
    public static void MapFinanceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/finance/ledger/balance", async (ISender sender) =>
            Results.Ok(new { balance = await sender.Send(new GetRunningBalanceQuery()) }))
            .WithTags("Finance - Ledger")
            .RequireModule(Module.Finance, AccessLevel.View);
    }
}
