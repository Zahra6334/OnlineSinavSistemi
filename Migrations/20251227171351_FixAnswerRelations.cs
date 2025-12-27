using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSinavSistemi.Migrations
{
    /// <inheritdoc />
    public partial class FixAnswerRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Choices_SelectedChoiceId",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Answers_StudentExams_StudentExamId",
                table: "Answers");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Choices_SelectedChoiceId",
                table: "Answers",
                column: "SelectedChoiceId",
                principalTable: "Choices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_StudentExams_StudentExamId",
                table: "Answers",
                column: "StudentExamId",
                principalTable: "StudentExams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Choices_SelectedChoiceId",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Answers_StudentExams_StudentExamId",
                table: "Answers");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Choices_SelectedChoiceId",
                table: "Answers",
                column: "SelectedChoiceId",
                principalTable: "Choices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_StudentExams_StudentExamId",
                table: "Answers",
                column: "StudentExamId",
                principalTable: "StudentExams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
