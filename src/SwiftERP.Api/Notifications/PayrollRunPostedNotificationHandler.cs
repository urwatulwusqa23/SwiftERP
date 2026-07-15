using MediatR;
using SwiftERP.HR.Domain.Payroll;

namespace SwiftERP.Api.Notifications;

public class PayrollRunPostedNotificationHandler(INotificationPublisher publisher)
    : INotificationHandler<PayrollRunPostedEvent>
{
    public Task Handle(PayrollRunPostedEvent notification, CancellationToken cancellationToken) =>
        publisher.PublishAsync(
            "payroll.processed",
            new
            {
                payrollRunId = notification.PayrollRunId,
                year = notification.Year,
                month = notification.Month,
                total = notification.Total
            },
            cancellationToken);
}
