using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateShowtimesAR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CinemaId",
                table: "Showtimes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "FinalPrice",
                table: "ShowtimePricings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "MovieCopyrights",
                type: "int",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Showtimes_CinemaId",
                table: "Showtimes",
                column: "CinemaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Showtimes_Cinemas_CinemaId",
                table: "Showtimes",
                column: "CinemaId",
                principalTable: "Cinemas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Showtimes_Cinemas_CinemaId",
                table: "Showtimes");

            migrationBuilder.DropIndex(
                name: "IX_Showtimes_CinemaId",
                table: "Showtimes");

            migrationBuilder.DropColumn(
                name: "CinemaId",
                table: "Showtimes");

            migrationBuilder.DropColumn(
                name: "FinalPrice",
                table: "ShowtimePricings");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "MovieCopyrights",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
