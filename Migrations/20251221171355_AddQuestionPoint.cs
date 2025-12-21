using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSinavSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Point",
                table: "Questions",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Point",
                table: "Questions");
        }
    }
}
