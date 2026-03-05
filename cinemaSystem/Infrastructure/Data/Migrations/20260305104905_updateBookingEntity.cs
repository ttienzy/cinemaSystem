using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateBookingEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Staffs_StaffId",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "StaffId",
                table: "Bookings",
                newName: "CinemaId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_StaffId",
                table: "Bookings",
                newName: "IX_Bookings_CinemaId");

            migrationBuilder.AddColumn<Guid>(
                name: "CinemaId1",
                table: "Staffs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Promotions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MaxUsagePerUser",
                table: "Promotions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SpecificCinemaId",
                table: "Promotions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SpecificMovieId",
                table: "Promotions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Promotions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_CinemaId1",
                table: "Staffs",
                column: "CinemaId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Cinemas_CinemaId",
                table: "Bookings",
                column: "CinemaId",
                principalTable: "Cinemas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_Cinemas_CinemaId1",
                table: "Staffs",
                column: "CinemaId1",
                principalTable: "Cinemas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Cinemas_CinemaId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_Cinemas_CinemaId1",
                table: "Staffs");

            migrationBuilder.DropIndex(
                name: "IX_Staffs_CinemaId1",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "CinemaId1",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "MaxUsagePerUser",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "SpecificCinemaId",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "SpecificMovieId",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Promotions");

            migrationBuilder.RenameColumn(
                name: "CinemaId",
                table: "Bookings",
                newName: "StaffId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_CinemaId",
                table: "Bookings",
                newName: "IX_Bookings_StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Staffs_StaffId",
                table: "Bookings",
                column: "StaffId",
                principalTable: "Staffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
