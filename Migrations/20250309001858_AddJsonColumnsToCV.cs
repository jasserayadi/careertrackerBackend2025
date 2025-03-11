using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Career_Tracker_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddJsonColumnsToCV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Experiences",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "CVs");

            migrationBuilder.AddColumn<string>(
                name: "ExperiencesJson",
                table: "CVs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SkillsJson",
                table: "CVs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExperiencesJson",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "SkillsJson",
                table: "CVs");

            migrationBuilder.AddColumn<string>(
                name: "Experiences",
                table: "CVs",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "CVs",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
