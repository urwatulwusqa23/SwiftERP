using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwiftERP.SharedKernel;
using SwiftERP.HR.Application.Abstractions;
using SwiftERP.HR.Domain.Attendance;
using SwiftERP.HR.Domain.Employees;
using SwiftERP.HR.Domain.Leave;
using SwiftERP.HR.Domain.Payroll;
using SwiftERP.HR.Domain.Shared;
using SwiftERP.HR.Infrastructure.Persistence;
using SwiftERP.HR.Infrastructure.Persistence.Repositories;
using SwiftERP.HR.Infrastructure.Storage;

namespace SwiftERP.HR.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddHrInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // See SwiftERP.Inventory.Infrastructure.DependencyInjection for why the connection string
        // is read lazily inside the options delegate rather than as a captured variable.
        services.AddDbContext<HrDbContext>(options => options.UseNpgsql(
            PostgresConnectionString.Normalize(
                configuration.GetConnectionString("SwiftErpDb")
                    ?? throw new InvalidOperationException("Connection string 'SwiftErpDb' is not configured."))));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<HrDbContext>());
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeDocumentRepository, EmployeeDocumentRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IPayrollRunRepository, PayrollRunRepository>();
        services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<IHolidayRepository, HolidayRepository>();
        services.AddScoped<IHrLedgerPort, HrScopedLedgerRepository>();
        services.AddSingleton<IDocumentStorage, LocalDiskDocumentStorage>();

        return services;
    }
}
