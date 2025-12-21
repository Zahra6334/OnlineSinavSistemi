using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSinavSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddSelectedChoiceNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Answers_SelectedChoiceId",
                table: "Answers",
                column: "SelectedChoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Choices_SelectedChoiceId",
                table: "Answers",
                column: "SelectedChoiceId",
                principalTable: "Choices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Choices_SelectedChoiceId",
                table: "Answers");

            migrationBuilder.DropIndex(
                name: "IX_Answers_SelectedChoiceId",
                table: "Answers");
        }
    }
}
