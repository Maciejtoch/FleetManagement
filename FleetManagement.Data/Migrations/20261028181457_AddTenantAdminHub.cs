using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantAdminHub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Vehicles",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "VehicleLocations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Stops",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "ServiceRecords",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "LocationShareSessions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "DailyReportStops",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "DailyReports",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Domain = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    PlanType = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    SubscriptionEndsAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SuspensionReason = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AdminUserId = table.Column<string>(type: "TEXT", nullable: false),
                    TenantId = table.Column<int>(type: "INTEGER", nullable: true),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AspNetUsers_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_TenantId",
                table: "Vehicles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLocations_TenantId",
                table: "VehicleLocations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Stops_TenantId",
                table: "Stops",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRecords_TenantId",
                table: "ServiceRecords",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationShareSessions_TenantId",
                table: "LocationShareSessions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReportStops_TenantId",
                table: "DailyReportStops",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_TenantId",
                table: "DailyReports",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TenantId",
                table: "AspNetUsers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AdminUserId",
                table: "AuditLogs",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TenantId",
                table: "AuditLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Domain",
                table: "Tenants",
                column: "Domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Slug",
                table: "Tenants",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Tenants_TenantId",
                table: "AspNetUsers",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyReports_Tenants_TenantId",
                table: "DailyReports",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyReportStops_Tenants_TenantId",
                table: "DailyReportStops",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationShareSessions_Tenants_TenantId",
                table: "LocationShareSessions",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRecords_Tenants_TenantId",
                table: "ServiceRecords",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stops_Tenants_TenantId",
                table: "Stops",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLocations_Tenants_TenantId",
                table: "VehicleLocations",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Tenants_TenantId",
                table: "Vehicles",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Tenants_TenantId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_DailyReports_Tenants_TenantId",
                table: "DailyReports");

            migrationBuilder.DropForeignKey(
                name: "FK_DailyReportStops_Tenants_TenantId",
                table: "DailyReportStops");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationShareSessions_Tenants_TenantId",
                table: "LocationShareSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRecords_Tenants_TenantId",
                table: "ServiceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Stops_Tenants_TenantId",
                table: "Stops");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLocations_Tenants_TenantId",
                table: "VehicleLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Tenants_TenantId",
                table: "Vehicles");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_TenantId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_VehicleLocations_TenantId",
                table: "VehicleLocations");

            migrationBuilder.DropIndex(
                name: "IX_Stops_TenantId",
                table: "Stops");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRecords_TenantId",
                table: "ServiceRecords");

            migrationBuilder.DropIndex(
                name: "IX_LocationShareSessions_TenantId",
                table: "LocationShareSessions");

            migrationBuilder.DropIndex(
                name: "IX_DailyReportStops_TenantId",
                table: "DailyReportStops");

            migrationBuilder.DropIndex(
                name: "IX_DailyReports_TenantId",
                table: "DailyReports");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TenantId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "VehicleLocations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Stops");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ServiceRecords");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "LocationShareSessions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DailyReportStops");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DailyReports");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetUsers");
        }
    }
}
