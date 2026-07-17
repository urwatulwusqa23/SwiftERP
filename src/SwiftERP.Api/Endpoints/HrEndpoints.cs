using MediatR;
using SwiftERP.Api.Authorization;
using SwiftERP.Identity.Application.Abstractions;
using SwiftERP.Identity.Domain.Roles;
using SwiftERP.HR.Application.Attendance.ClockIn;
using SwiftERP.HR.Application.Attendance.ClockOut;
using SwiftERP.HR.Application.Attendance.GetAttendance;
using SwiftERP.HR.Application.Employees.GetAllEmployees;
using SwiftERP.HR.Application.Employees.GetEmployeeById;
using SwiftERP.HR.Application.Employees.GetEmployeeDocuments;
using SwiftERP.HR.Application.Employees.GetOrgChart;
using SwiftERP.HR.Application.Employees.HireEmployee;
using SwiftERP.HR.Application.Employees.UpdateEmployee;
using SwiftERP.HR.Application.Employees.UploadEmployeeDocument;
using SwiftERP.HR.Application.Payroll.CreatePayrollRun;
using SwiftERP.HR.Application.Payroll.PostPayrollRun;
using SwiftERP.HR.Application.Leave.RequestLeave;
using SwiftERP.HR.Application.Leave.ApproveLeave;
using SwiftERP.HR.Application.Leave.RejectLeave;
using SwiftERP.HR.Application.Leave.GetLeaveBalances;
using SwiftERP.HR.Application.Leave.GetLeaveRequests;
using SwiftERP.HR.Application.Leave.Holidays;
using SwiftERP.HR.Application.Abstractions;
using SwiftERP.HR.Domain.Employees;

namespace SwiftERP.Api.Endpoints;

public static class HrEndpoints
{
    public static void MapHrEndpoints(this IEndpointRouteBuilder app)
    {
        var employees = app.MapGroup("/api/v1/hr/employees").WithTags("HR - Employees");

        employees.MapPost("/", async (HireEmployeeCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/v1/hr/employees/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        }).RequireModule(Module.HR, AccessLevel.Edit);

        employees.MapGet("/", async (ISender sender) =>
            Results.Ok(await sender.Send(new GetAllEmployeesQuery())))
            .RequireModule(Module.HR, AccessLevel.View);

        employees.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var profile = await sender.Send(new GetEmployeeByIdQuery(id));
            return profile is not null ? Results.Ok(profile) : Results.NotFound();
        }).RequireSelfOrModule(Module.HR, AccessLevel.View);

        employees.MapPut("/{id:guid}", async (Guid id, UpdateEmployeeRequest request, ISender sender) =>
        {
            var result = await sender.Send(new UpdateEmployeeCommand(
                id, request.PhoneNumber, request.Address, request.DateOfBirth,
                request.JobTitle, request.Department, request.ManagerId));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { error = result.Error });
        }).RequireSelfOrModule(Module.HR, AccessLevel.Edit);

        employees.MapPost("/{id:guid}/attendance/clock-in", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ClockInCommand(id));
            return result.IsSuccess
                ? Results.Created($"/api/v1/hr/employees/{id}/attendance", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        }).RequireSelfOrModule(Module.HR, AccessLevel.Edit);

        employees.MapPost("/{id:guid}/attendance/clock-out", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ClockOutCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { error = result.Error });
        }).RequireSelfOrModule(Module.HR, AccessLevel.Edit);

        employees.MapGet("/{id:guid}/attendance", async (Guid id, ISender sender) =>
            Results.Ok(await sender.Send(new GetAttendanceForEmployeeQuery(id))))
            .RequireSelfOrModule(Module.HR, AccessLevel.View);

        employees.MapPost("/{id:guid}/documents", async (Guid id, HttpRequest httpRequest, ISender sender) =>
        {
            if (!httpRequest.HasFormContentType)
                return Results.BadRequest(new { error = "Expected multipart/form-data." });

            var form = await httpRequest.ReadFormAsync();
            var file = form.Files.GetFile("file");
            if (file is null || file.Length == 0)
                return Results.BadRequest(new { error = "No file was uploaded." });

            if (!Enum.TryParse<EmployeeDocumentType>(form["documentType"], out var documentType))
                return Results.BadRequest(new { error = "Invalid or missing documentType." });

            await using var stream = file.OpenReadStream();
            var result = await sender.Send(new UploadEmployeeDocumentCommand(
                id, documentType, file.FileName, file.ContentType, file.Length, stream));

            return result.IsSuccess
                ? Results.Created($"/api/v1/hr/documents/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        }).DisableAntiforgery().RequireSelfOrModule(Module.HR, AccessLevel.Edit);

        employees.MapGet("/{id:guid}/documents", async (Guid id, ISender sender) =>
            Results.Ok(await sender.Send(new GetEmployeeDocumentsQuery(id))))
            .RequireSelfOrModule(Module.HR, AccessLevel.View);

        app.MapGet("/api/v1/hr/documents/{documentId:guid}/download", async (
            Guid documentId, HttpContext httpContext, ISender sender, IDocumentStorage storage, CancellationToken ct) =>
        {
            var metadata = await sender.Send(new GetEmployeeDocumentByIdQuery(documentId), ct);
            if (metadata is null)
                return Results.NotFound();

            var callerEmployeeId = httpContext.User.FindFirst(SwiftErpClaimTypes.EmployeeId)?.Value;
            var isSelf = Guid.TryParse(callerEmployeeId, out var callerGuid) && callerGuid == metadata.EmployeeId;
            if (!isSelf && !RequireModuleAccessFilter.HasAccess(httpContext.User, Module.HR, AccessLevel.View))
                return Results.Json(new { error = "Requires View access to HR, or must be your own document." }, statusCode: StatusCodes.Status403Forbidden);

            var stream = await storage.OpenReadAsync(metadata.StoragePath, ct);
            return Results.File(stream, metadata.ContentType, metadata.FileName);
        }).WithTags("HR - Employees").RequireAuthorization();

        app.MapGet("/api/v1/hr/org-chart", async (ISender sender) =>
            Results.Ok(await sender.Send(new GetOrgChartQuery())))
            .WithTags("HR - Employees").RequireModule(Module.HR, AccessLevel.View);

        var leave = app.MapGroup("/api/v1/hr/leave-requests").WithTags("HR - Leave");

        leave.MapPost("/", async (RequestLeaveCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/v1/hr/leave-requests/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        }).RequireAuthorization();

        leave.MapPost("/{id:guid}/approve", async (Guid id, HttpContext httpContext, ISender sender) =>
        {
            var (approverEmployeeId, hasFullHr) = ResolveApprover(httpContext);
            var result = await sender.Send(new ApproveLeaveCommand(id, approverEmployeeId, hasFullHr));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { error = result.Error });
        }).RequireModule(Module.HR, AccessLevel.Edit);

        leave.MapPost("/{id:guid}/reject", async (Guid id, RejectLeaveRequest request, HttpContext httpContext, ISender sender) =>
        {
            var (approverEmployeeId, hasFullHr) = ResolveApprover(httpContext);
            var result = await sender.Send(new RejectLeaveCommand(id, request.Note, approverEmployeeId, hasFullHr));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { error = result.Error });
        }).RequireModule(Module.HR, AccessLevel.Edit);

        employees.MapGet("/{id:guid}/leave-requests", async (Guid id, ISender sender) =>
            Results.Ok(await sender.Send(new GetLeaveRequestsQuery(id))))
            .RequireSelfOrModule(Module.HR, AccessLevel.View);

        employees.MapGet("/{id:guid}/leave-balances", async (Guid id, int? year, ISender sender) =>
            Results.Ok(await sender.Send(new GetLeaveBalancesQuery(id, year ?? DateTime.UtcNow.Year))))
            .RequireSelfOrModule(Module.HR, AccessLevel.View);

        var holidays = app.MapGroup("/api/v1/hr/holidays").WithTags("HR - Leave");

        holidays.MapPost("/", async (CreateHolidayCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/v1/hr/holidays/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        }).RequireModule(Module.HR, AccessLevel.Edit);

        holidays.MapGet("/", async (int? year, ISender sender) =>
            Results.Ok(await sender.Send(new GetHolidaysQuery(year ?? DateTime.UtcNow.Year))))
            .RequireAuthorization();

        var payroll = app.MapGroup("/api/v1/hr/payroll-runs").WithTags("HR - Payroll");

        payroll.MapPost("/", async (CreatePayrollRunCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/v1/hr/payroll-runs/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        }).RequireModule(Module.HR, AccessLevel.Full);

        payroll.MapPost("/{id:guid}/post", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new PostPayrollRunCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(new { error = result.Error });
        }).RequireModule(Module.HR, AccessLevel.Full);
    }

    private static (Guid EmployeeId, bool HasFullHrAccess) ResolveApprover(HttpContext httpContext)
    {
        var employeeIdClaim = httpContext.User.FindFirst(SwiftErpClaimTypes.EmployeeId)?.Value;
        Guid.TryParse(employeeIdClaim, out var employeeId);
        var hasFullHr = RequireModuleAccessFilter.HasAccess(httpContext.User, Module.HR, AccessLevel.Full);
        return (employeeId, hasFullHr);
    }
}

public record UpdateEmployeeRequest(
    string? PhoneNumber,
    string? Address,
    DateOnly? DateOfBirth,
    string? JobTitle,
    string? Department,
    Guid? ManagerId);

public record RejectLeaveRequest(string? Note);
