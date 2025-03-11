using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Career_Tracker_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddJsonColumnsToCV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SkillsJson",
                table: "CVs",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ExperiencesJson",
                table: "CVs",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CvFile",
                table: "CVs",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "CVs",
                keyColumn: "SkillsJson",
                keyValue: null,
                column: "SkillsJson",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "SkillsJson",
                table: "CVs",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "CVs",
                keyColumn: "ExperiencesJson",
                keyValue: null,
                column: "ExperiencesJson",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ExperiencesJson",
                table: "CVs",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "CVs",
                keyColumn: "CvFile",
                keyValue: null,
                column: "CvFile",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "CvFile",
                table: "CVs",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
