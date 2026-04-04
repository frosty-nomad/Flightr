using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Flightr.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAircraftTypesSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Cessna 150");

            migrationBuilder.UpdateData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Cessna 172");

            migrationBuilder.UpdateData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Cessna 182");

            migrationBuilder.UpdateData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Mooney M20");

            migrationBuilder.InsertData(
                table: "AircraftTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 7, "Piper Archer" },
                    { 8, "Piper PA-28" },
                    { 9, "Cirrus SR20" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.UpdateData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Cessna 172");

            migrationBuilder.UpdateData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Cessna 182");

            migrationBuilder.UpdateData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Cirrus SR20");

            migrationBuilder.UpdateData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Piper PA-28");
        }
    }
}
