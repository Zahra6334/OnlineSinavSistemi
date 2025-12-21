using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSinavSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAutoGradedToExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAutoGraded",
                table: "Exams",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAutoGraded",
                table: "Exams");
        }
    }
}
