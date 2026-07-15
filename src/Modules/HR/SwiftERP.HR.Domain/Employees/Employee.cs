using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Domain.Employees;

public enum EmploymentStatus
{
    Active,
    Terminated
}

public class Employee : Entity
{
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public decimal MonthlySalary { get; private set; }
    public DateOnly HireDate { get; private set; }
    public EmploymentStatus Status { get; private set; }

    public string? PhoneNumber { get; private set; }
    public string? Address { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }

    public string? JobTitle { get; private set; }
    public string? Department { get; private set; }
    public Guid? ManagerId { get; private set; }

    private Employee()
    {
    }

    public Employee(
        string fullName,
        string email,
        decimal monthlySalary,
        DateOnly hireDate,
        string? jobTitle = null,
        string? department = null,
        Guid? managerId = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Employee full name cannot be empty.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Employee email cannot be empty.");
        if (monthlySalary <= 0)
            throw new DomainException("Monthly salary must be positive.");

        FullName = fullName;
        Email = email;
        MonthlySalary = monthlySalary;
        HireDate = hireDate;
        Status = EmploymentStatus.Active;

        JobTitle = jobTitle;
        Department = department;
        SetManager(managerId);
    }

    public void UpdatePersonalInfo(string? phoneNumber, string? address, DateOnly? dateOfBirth)
    {
        PhoneNumber = phoneNumber;
        Address = address;
        DateOfBirth = dateOfBirth;
    }

    public void UpdateJobInfo(string? jobTitle, string? department, Guid? managerId)
    {
        JobTitle = jobTitle;
        Department = department;
        SetManager(managerId);
    }

    private void SetManager(Guid? managerId)
    {
        if (managerId.HasValue && managerId.Value == Id)
            throw new DomainException("An employee cannot be their own manager.");

        ManagerId = managerId;
    }

    public void Terminate()
    {
        if (Status == EmploymentStatus.Terminated)
            throw new DomainException("Employee is already terminated.");

        Status = EmploymentStatus.Terminated;
    }
}
