using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwiftERP.HR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Tier1_EmployeeDb_Leave_Attendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Present",
                schema: "hr",
                table: "AttendanceRecords");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "hr",
                table: "Employees",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                schema: "hr",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                schema: "hr",
                table: "Employees",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                schema: "hr",
                table: "Employees",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                schema: "hr",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "hr",
                table: "Employees",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClockInUtc",
                schema: "hr",
                table: "AttendanceRecords",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClockOutUtc",
                schema: "hr",
                table: "AttendanceRecords",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeDocuments",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Holidays",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaveBalances",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    TotalDays = table.Column<int>(type: "int", nullable: false),
                    UsedDays = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveBalances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DecisionNote = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_EmployeeId",
                schema: "hr",
                table: "AttendanceRecords",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDocuments_EmployeeId",
                schema: "hr",
                table: "EmployeeDocuments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_EmployeeId_LeaveType_Year",
                schema: "hr",
                table: "LeaveBalances",
                columns: new[] { "EmployeeId", "LeaveType", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_EmployeeId",
                schema: "hr",
                table: "LeaveRequests",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeDocuments",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "Holidays",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "LeaveBalances",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "LeaveRequests",
                schema: "hr");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_EmployeeId",
                schema: "hr",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "Address",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Department",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "hr",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ClockInUtc",
                schema: "hr",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ClockOutUtc",
                schema: "hr",
                table: "AttendanceRecords");

            migrationBuilder.AddColumn<bool>(
                name: "Present",
                schema: "hr",
                table: "AttendanceRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
