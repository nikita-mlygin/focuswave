using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Focuswave.SessionTrackingService.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FocusCycles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FocusCycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FocusCycleSegments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PlannedDuration = table.Column<TimeSpan>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FocusCycleSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FocusCycleSegments_FocusCycles_CycleId",
                        column: x => x.CycleId,
                        principalTable: "FocusCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FocusCycles_UserId",
                table: "FocusCycles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FocusCycleSegments_CycleId_Index",
                table: "FocusCycleSegments",
                columns: new[] { "CycleId", "Index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FocusCycleSegments_CycleId_StartedAt",
                table: "FocusCycleSegments",
                columns: new[] { "CycleId", "StartedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FocusCycleSegments");

            migrationBuilder.DropTable(
                name: "FocusCycles");
        }
    }
}
