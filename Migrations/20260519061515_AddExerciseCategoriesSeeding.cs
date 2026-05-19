using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GymFlow.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseCategoriesSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ExerciseCategories",
                columns: new[] { "Id", "CreatedAt", "Description", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 5, 19, 6, 15, 13, 606, DateTimeKind.Utc).AddTicks(3219), "Ćwiczenia na budowanie siły mięśniowej", "Siła" },
                    { 2, new DateTime(2026, 5, 19, 6, 15, 13, 606, DateTimeKind.Utc).AddTicks(3226), "Ćwiczenia na wytrzymałość sercowo-naczyniową", "Kardio" },
                    { 3, new DateTime(2026, 5, 19, 6, 15, 13, 606, DateTimeKind.Utc).AddTicks(3230), "Ćwiczenia na rozciąganie i elastyczność", "Elastyczność" },
                    { 4, new DateTime(2026, 5, 19, 6, 15, 13, 606, DateTimeKind.Utc).AddTicks(3234), "Ćwiczenia funkcjonalne dla całego ciała", "Funkcjonalne" },
                    { 5, new DateTime(2026, 5, 19, 6, 15, 13, 606, DateTimeKind.Utc).AddTicks(3238), "Ćwiczenia jogi", "Yoga" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExerciseCategories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ExerciseCategories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ExerciseCategories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ExerciseCategories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ExerciseCategories",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
