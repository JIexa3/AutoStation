using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoStation.Migrations
{
    /// <inheritdoc />
    public partial class jkzczx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "VerificationCodeExpiry",
                value: new DateTime(2026, 3, 30, 14, 23, 43, 555, DateTimeKind.Local).AddTicks(1964));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "VerificationCodeExpiry",
                value: new DateTime(2025, 12, 27, 14, 39, 24, 600, DateTimeKind.Local).AddTicks(7601));
        }
    }
}
