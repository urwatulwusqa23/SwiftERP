using SwiftERP.HR.Domain.Employees;
using SwiftERP.SharedKernel;

namespace SwiftERP.HR.Domain.Tests.Employees;

public class EmployeeDocumentTests
{
    [Fact]
    public void Constructor_WithValidArgs_Succeeds()
    {
        var document = new EmployeeDocument(
            Guid.NewGuid(), EmployeeDocumentType.Contract, "contract.pdf", "abc/contract.pdf", "application/pdf", 1024);

        Assert.Equal("contract.pdf", document.FileName);
        Assert.Equal(1024, document.SizeBytes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyFileName_Throws(string fileName)
    {
        Assert.Throws<DomainException>(() =>
            new EmployeeDocument(Guid.NewGuid(), EmployeeDocumentType.Other, fileName, "path", "text/plain", 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithNonPositiveSize_Throws(long size)
    {
        Assert.Throws<DomainException>(() =>
            new EmployeeDocument(Guid.NewGuid(), EmployeeDocumentType.Other, "file.txt", "path", "text/plain", size));
    }

    [Fact]
    public void Constructor_WithEmptyContentType_DefaultsToOctetStream()
    {
        var document = new EmployeeDocument(
            Guid.NewGuid(), EmployeeDocumentType.Other, "file.bin", "path", "", 10);

        Assert.Equal("application/octet-stream", document.ContentType);
    }
}
