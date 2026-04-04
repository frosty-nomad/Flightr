using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Flightr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAircraftTypesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AircraftTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftTypes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "AircraftTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Beechcraft Bonanza" },
                    { 2, "Cessna 172" },
                    { 3, "Cessna 182" },
                    { 4, "Cirrus SR20" },
                    { 5, "Diamond DA40" },
                    { 6, "Piper PA-28" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AircraftTypes_Name",
                table: "AircraftTypes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AircraftTypes");
        }
    }
}
