using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ExerciseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 24, 17, 34, 11, 364, DateTimeKind.Utc).AddTicks(8533));

            migrationBuilder.UpdateData(
                table: "ExerciseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 24, 17, 34, 11, 364, DateTimeKind.Utc).AddTicks(8541));

            migrationBuilder.UpdateData(
                table: "ExerciseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 24, 17, 34, 11, 364, DateTimeKind.Utc).AddTicks(8547));

            migrationBuilder.UpdateData(
                table: "ExerciseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 24, 17, 34, 11, 364, DateTimeKind.Utc).AddTicks(8552));

            migrationBuilder.UpdateData(
                table: "ExerciseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 24, 17, 34, 11, 364, DateTimeKind.Utc).AddTicks(8557));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ApiKey", "CreatedAt", "Email", "FirstName", "IsActive", "LastName", "PasswordHash", "Role", "Username" },
                values: new object[] { 999, null, new DateTime(2026, 5, 24, 17, 34, 11, 968, DateTimeKind.Utc).AddTicks(2161), "admin@gymflow.pl", "Administrator", true, "GymFlow", "$2a$11$z/UDake1S2DYcVAj9pTZheXwWy4SWRRhd3PwvPZ/VJSkce.yJDEmW", 1, "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 999);

            migrationBuilder.UpdateData(
                table: "ExerciseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 6, 15, 13, 606, DateTimeKind.Utc).AddTicks(3219));

            migrationBuilder.UpdateData(
                table: "ExerciseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 6, 15, 13, 606, DateTimeKind.Utc).AddTicks(3226));

            migrationBuilder.UpdateData(
                table: "ExerciseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 6, 15, 13, 606, DateTimeKind.Utc).AddTicks(3230));

            migrationBuilder.UpdateData(
                table: "ExerciseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 6, 15, 13, 606, DateTimeKind.Utc).AddTicks(3234));

            migrationBuilder.UpdateData(
                table: "ExerciseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 6, 15, 13, 606, DateTimeKind.Utc).AddTicks(3238));
        }
    }
}
