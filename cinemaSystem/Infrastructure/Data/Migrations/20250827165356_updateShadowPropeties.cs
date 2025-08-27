using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateShadowPropeties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingTickets_Bookings_BookingId",
                table: "BookingTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_ConcessionSaleItems_ConcessionSales_ConcessionSaleId",
                table: "ConcessionSaleItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_Cinemas_CinemaId",
                table: "Equipments");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Cinemas_CinemaId",
                table: "InventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceLogs_Equipments_EquipmentId",
                table: "MaintenanceLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_MovieCastCrews_Movies_MovieId",
                table: "MovieCastCrews");

            migrationBuilder.DropForeignKey(
                name: "FK_MovieCertifications_Movies_MovieId",
                table: "MovieCertifications");

            migrationBuilder.DropForeignKey(
                name: "FK_MovieCopyrights_Movies_MovieId",
                table: "MovieCopyrights");

            migrationBuilder.DropForeignKey(
                name: "FK_MovieGenres_Movies_MovieId",
                table: "MovieGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Screens_Cinemas_CinemaId",
                table: "Screens");

            migrationBuilder.DropForeignKey(
                name: "FK_Seats_Screens_ScreenId",
                table: "Seats");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Staffs_StaffId",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_ShowtimePricings_Showtimes_ShowtimeId",
                table: "ShowtimePricings");

            migrationBuilder.DropForeignKey(
                name: "FK_Showtimes_Screens_ScreenId",
                table: "Showtimes");

            migrationBuilder.AlterColumn<Guid>(
                name: "ShowtimeId",
                table: "ShowtimePricings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StaffId",
                table: "Shifts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ScreenId",
                table: "Seats",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CinemaId",
                table: "Screens",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "BookingId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MovieId",
                table: "MovieGenres",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MovieId",
                table: "MovieCopyrights",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MovieId",
                table: "MovieCertifications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MovieId",
                table: "MovieCastCrews",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "EquipmentId",
                table: "MaintenanceLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ConcessionSaleId",
                table: "ConcessionSaleItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "BookingId",
                table: "BookingTickets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingTickets_Bookings_BookingId",
                table: "BookingTickets",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConcessionSaleItems_ConcessionSales_ConcessionSaleId",
                table: "ConcessionSaleItems",
                column: "ConcessionSaleId",
                principalTable: "ConcessionSales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_Cinemas_CinemaId",
                table: "Equipments",
                column: "CinemaId",
                principalTable: "Cinemas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Cinemas_CinemaId",
                table: "InventoryItems",
                column: "CinemaId",
                principalTable: "Cinemas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceLogs_Equipments_EquipmentId",
                table: "MaintenanceLogs",
                column: "EquipmentId",
                principalTable: "Equipments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovieCastCrews_Movies_MovieId",
                table: "MovieCastCrews",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovieCertifications_Movies_MovieId",
                table: "MovieCertifications",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovieCopyrights_Movies_MovieId",
                table: "MovieCopyrights",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovieGenres_Movies_MovieId",
                table: "MovieGenres",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Screens_Cinemas_CinemaId",
                table: "Screens",
                column: "CinemaId",
                principalTable: "Cinemas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_Screens_ScreenId",
                table: "Seats",
                column: "ScreenId",
                principalTable: "Screens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Staffs_StaffId",
                table: "Shifts",
                column: "StaffId",
                principalTable: "Staffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShowtimePricings_Showtimes_ShowtimeId",
                table: "ShowtimePricings",
                column: "ShowtimeId",
                principalTable: "Showtimes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Showtimes_Screens_ScreenId",
                table: "Showtimes",
                column: "ScreenId",
                principalTable: "Screens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingTickets_Bookings_BookingId",
                table: "BookingTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_ConcessionSaleItems_ConcessionSales_ConcessionSaleId",
                table: "ConcessionSaleItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_Cinemas_CinemaId",
                table: "Equipments");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Cinemas_CinemaId",
                table: "InventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceLogs_Equipments_EquipmentId",
                table: "MaintenanceLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_MovieCastCrews_Movies_MovieId",
                table: "MovieCastCrews");

            migrationBuilder.DropForeignKey(
                name: "FK_MovieCertifications_Movies_MovieId",
                table: "MovieCertifications");

            migrationBuilder.DropForeignKey(
                name: "FK_MovieCopyrights_Movies_MovieId",
                table: "MovieCopyrights");

            migrationBuilder.DropForeignKey(
                name: "FK_MovieGenres_Movies_MovieId",
                table: "MovieGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Screens_Cinemas_CinemaId",
                table: "Screens");

            migrationBuilder.DropForeignKey(
                name: "FK_Seats_Screens_ScreenId",
                table: "Seats");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Staffs_StaffId",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_ShowtimePricings_Showtimes_ShowtimeId",
                table: "ShowtimePricings");

            migrationBuilder.DropForeignKey(
                name: "FK_Showtimes_Screens_ScreenId",
                table: "Showtimes");

            migrationBuilder.AlterColumn<Guid>(
                name: "ShowtimeId",
                table: "ShowtimePricings",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "StaffId",
                table: "Shifts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ScreenId",
                table: "Seats",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "CinemaId",
                table: "Screens",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "BookingId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "MovieId",
                table: "MovieGenres",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "MovieId",
                table: "MovieCopyrights",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "MovieId",
                table: "MovieCertifications",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "MovieId",
                table: "MovieCastCrews",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "EquipmentId",
                table: "MaintenanceLogs",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConcessionSaleId",
                table: "ConcessionSaleItems",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "BookingId",
                table: "BookingTickets",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingTickets_Bookings_BookingId",
                table: "BookingTickets",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConcessionSaleItems_ConcessionSales_ConcessionSaleId",
                table: "ConcessionSaleItems",
                column: "ConcessionSaleId",
                principalTable: "ConcessionSales",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_Cinemas_CinemaId",
                table: "Equipments",
                column: "CinemaId",
                principalTable: "Cinemas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Cinemas_CinemaId",
                table: "InventoryItems",
                column: "CinemaId",
                principalTable: "Cinemas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceLogs_Equipments_EquipmentId",
                table: "MaintenanceLogs",
                column: "EquipmentId",
                principalTable: "Equipments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovieCastCrews_Movies_MovieId",
                table: "MovieCastCrews",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovieCertifications_Movies_MovieId",
                table: "MovieCertifications",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovieCopyrights_Movies_MovieId",
                table: "MovieCopyrights",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovieGenres_Movies_MovieId",
                table: "MovieGenres",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Screens_Cinemas_CinemaId",
                table: "Screens",
                column: "CinemaId",
                principalTable: "Cinemas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_Screens_ScreenId",
                table: "Seats",
                column: "ScreenId",
                principalTable: "Screens",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Staffs_StaffId",
                table: "Shifts",
                column: "StaffId",
                principalTable: "Staffs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShowtimePricings_Showtimes_ShowtimeId",
                table: "ShowtimePricings",
                column: "ShowtimeId",
                principalTable: "Showtimes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Showtimes_Screens_ScreenId",
                table: "Showtimes",
                column: "ScreenId",
                principalTable: "Screens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
