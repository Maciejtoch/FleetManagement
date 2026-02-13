using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class StopsFunctionalityAddOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOnMap",
                table: "Stops",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOnMap",
                table: "Stops");
        }
    }
}
