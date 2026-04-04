using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Flightr.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefreshCivilAircraftSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 6);

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

            migrationBuilder.InsertData(
                table: "AircraftTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 3, "Cessna 152" },
                    { 4, "Cessna 172" },
                    { 5, "Cessna 182" },
                    { 6, "Cirrus SR20" },
                    { 7, "Cirrus SR22" },
                    { 8, "Diamond DA20" },
                    { 9, "Diamond DA40" },
                    { 10, "Grumman AA-5" },
                    { 11, "Mooney M20" },
                    { 12, "Piper Archer" },
                    { 13, "Piper Cherokee" },
                    { 14, "Piper PA-28" },
                    { 15, "Piper Saratoga" },
                    { 16, "Piper Seminole" },
                    { 17, "Robin DR400" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AircraftTypes",
                keyColumn: "Id",
                keyValue: 6);

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

            migrationBuilder.InsertData(
                table: "AircraftTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 3, "Cessna 172" },
                    { 4, "Cessna 182" },
                    { 5, "Diamond DA40" },
                    { 6, "Mooney M20" },
                    { 7, "Piper Archer" },
                    { 8, "Piper PA-28" },
                    { 9, "Cirrus SR20" }
                });
        }
    }
}
