using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCoordinatesToStops : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Stops",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Stops",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Stops");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Stops");
        }
    }
}
