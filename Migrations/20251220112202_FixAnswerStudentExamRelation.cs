using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSinavSistemi.Migrations
{
    /// <inheritdoc />
    public partial class FixAnswerStudentExamRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Answers_StudentExams_StudentExamId1",
                table: "Answers");

            migrationBuilder.DropIndex(
                name: "IX_Answers_StudentExamId1",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "StudentExamId1",
                table: "Answers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "StudentExams",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "StudentExams",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentExamId1",
                table: "Answers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Answers_StudentExamId1",
                table: "Answers",
                column: "StudentExamId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_StudentExams_StudentExamId1",
                table: "Answers",
                column: "StudentExamId1",
                principalTable: "StudentExams",
                principalColumn: "Id");
        }
    }
}
