using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AutoStation.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleFuelsToColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FuelColumns_Fuels_FuelId",
                table: "FuelColumns");

            migrationBuilder.DropIndex(
                name: "IX_FuelColumns_FuelId",
                table: "FuelColumns");

            migrationBuilder.DropColumn(
                name: "FuelId",
                table: "FuelColumns");

            migrationBuilder.CreateTable(
                name: "FuelColumnFuels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FuelColumnId = table.Column<int>(type: "int", nullable: false),
                    FuelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelColumnFuels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuelColumnFuels_FuelColumns_FuelColumnId",
                        column: x => x.FuelColumnId,
                        principalTable: "FuelColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FuelColumnFuels_Fuels_FuelId",
                        column: x => x.FuelId,
                        principalTable: "Fuels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "FuelColumnFuels",
                columns: new[] { "Id", "FuelColumnId", "FuelId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 2, 2 },
                    { 3, 3, 3 },
                    { 4, 4, 4 }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "VerificationCodeExpiry",
                value: new DateTime(2025, 12, 27, 14, 39, 24, 600, DateTimeKind.Local).AddTicks(7601));

            migrationBuilder.CreateIndex(
                name: "IX_FuelColumnFuels_FuelColumnId_FuelId",
                table: "FuelColumnFuels",
                columns: new[] { "FuelColumnId", "FuelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FuelColumnFuels_FuelId",
                table: "FuelColumnFuels",
                column: "FuelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FuelColumnFuels");

            migrationBuilder.AddColumn<int>(
                name: "FuelId",
                table: "FuelColumns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "FuelColumns",
                keyColumn: "Id",
                keyValue: 1,
                column: "FuelId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "FuelColumns",
                keyColumn: "Id",
                keyValue: 2,
                column: "FuelId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "FuelColumns",
                keyColumn: "Id",
                keyValue: 3,
                column: "FuelId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "FuelColumns",
                keyColumn: "Id",
                keyValue: 4,
                column: "FuelId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "VerificationCodeExpiry",
                value: new DateTime(2025, 12, 27, 14, 26, 1, 651, DateTimeKind.Local).AddTicks(6080));

            migrationBuilder.CreateIndex(
                name: "IX_FuelColumns_FuelId",
                table: "FuelColumns",
                column: "FuelId");

            migrationBuilder.AddForeignKey(
                name: "FK_FuelColumns_Fuels_FuelId",
                table: "FuelColumns",
                column: "FuelId",
                principalTable: "Fuels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
