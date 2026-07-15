using MediatR;
using Microsoft.EntityFrameworkCore;
using SwiftERP.Finance.Domain.LedgerEntries;
using SwiftERP.Finance.Infrastructure.Persistence.Configurations;
using SwiftERP.HR.Domain.Attendance;
using SwiftERP.HR.Domain.Employees;
using SwiftERP.HR.Domain.Leave;
using SwiftERP.HR.Domain.Payroll;
using SwiftERP.HR.Domain.Shared;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Infrastructure.Persistence;

/// <summary>
/// Also tracks Finance's LedgerEntry (write-only, ExcludeFromMigrations) so PostPayrollRun can
/// commit the payroll-posted state and the ledger expense in one transaction — same pattern as
/// SwiftERP.Sales.Infrastructure.Persistence.SalesDbContext.
/// </summary>
public class HrDbContext(DbContextOptions<HrDbContext> options, IPublisher publisher)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("hr");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrDbContext).Assembly);

        modelBuilder.ApplyConfiguration(new LedgerEntryConfiguration());
        modelBuilder.Entity<LedgerEntry>().ToTable(tb => tb.ExcludeFromMigrations());
    }

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        var trackedEntities = ChangeTracker.Entries<Entity>().Select(e => e.Entity).ToList();

        await base.SaveChangesAsync(cancellationToken);

        var events = DomainEventCollector.CollectAndClear(trackedEntities);
        foreach (var domainEvent in events)
            await publisher.Publish(domainEvent, cancellationToken);
    }
}
