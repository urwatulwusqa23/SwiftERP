using SwiftERP.HR.Domain.Employees;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Domain.Tests.Employees;

public class EmployeeTests
{
    private static Employee CreateEmployee(decimal salary = 5000m) =>
        new("Jane Doe", "jane@example.com", salary, new DateOnly(2026, 1, 1));

    [Fact]
    public void Constructor_WithValidArgs_StartsActive()
    {
        var employee = CreateEmployee();

        Assert.Equal(EmploymentStatus.Active, employee.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Constructor_WithNonPositiveSalary_Throws(decimal salary)
    {
        Assert.Throws<DomainException>(() => CreateEmployee(salary));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyFullName_Throws(string fullName)
    {
        Assert.Throws<DomainException>(() =>
            new Employee(fullName, "jane@example.com", 5000m, new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void Terminate_WhenActive_SetsStatusTerminated()
    {
        var employee = CreateEmployee();

        employee.Terminate();

        Assert.Equal(EmploymentStatus.Terminated, employee.Status);
    }

    [Fact]
    public void Terminate_WhenAlreadyTerminated_Throws()
    {
        var employee = CreateEmployee();
        employee.Terminate();

        Assert.Throws<DomainException>(employee.Terminate);
    }

    [Fact]
    public void UpdateJobInfo_WithSelfAsManager_Throws()
    {
        var employee = CreateEmployee();

        Assert.Throws<DomainException>(() => employee.UpdateJobInfo(null, null, employee.Id));
    }

    [Fact]
    public void UpdateJobInfo_WithDifferentManager_Succeeds()
    {
        var employee = CreateEmployee();
        var managerId = Guid.NewGuid();

        employee.UpdateJobInfo("Engineer", "Platform", managerId);

        Assert.Equal("Engineer", employee.JobTitle);
        Assert.Equal("Platform", employee.Department);
        Assert.Equal(managerId, employee.ManagerId);
    }

    [Fact]
    public void UpdatePersonalInfo_SetsFields()
    {
        var employee = CreateEmployee();
        var dob = new DateOnly(1995, 6, 1);

        employee.UpdatePersonalInfo("+1-555-0100", "123 Main St", dob);

        Assert.Equal("+1-555-0100", employee.PhoneNumber);
        Assert.Equal("123 Main St", employee.Address);
        Assert.Equal(dob, employee.DateOfBirth);
    }
}
