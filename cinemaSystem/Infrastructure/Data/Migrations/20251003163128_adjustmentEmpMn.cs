using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class adjustmentEmpMn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Staffs_StaffId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_StaffId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "ShiftDate",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "Shifts");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Shifts",
                newName: "DefaultStartTime");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Shifts",
                newName: "DefaultEndTime");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Shifts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShiffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "date", nullable: false),
                    ActualCheckInTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkSchedules_Shifts_ShiffId",
                        column: x => x.ShiffId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkSchedules_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkSchedules_ShiffId",
                table: "WorkSchedules",
                column: "ShiffId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSchedules_StaffId",
                table: "WorkSchedules",
                column: "StaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkSchedules");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Shifts");

            migrationBuilder.RenameColumn(
                name: "DefaultStartTime",
                table: "Shifts",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "DefaultEndTime",
                table: "Shifts",
                newName: "EndTime");

            migrationBuilder.AddColumn<DateTime>(
                name: "ShiftDate",
                table: "Shifts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId",
                table: "Shifts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_StaffId",
                table: "Shifts",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Staffs_StaffId",
                table: "Shifts",
                column: "StaffId",
                principalTable: "Staffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
