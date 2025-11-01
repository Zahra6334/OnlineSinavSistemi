using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineSinavSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddCourses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentExams_AspNetUsers_StudentId",
                table: "StudentExams");

            migrationBuilder.DropIndex(
                name: "IX_StudentExams_StudentId",
                table: "StudentExams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseStudents",
                table: "CourseStudents");

            migrationBuilder.DropIndex(
                name: "IX_CourseStudents_CourseId",
                table: "CourseStudents");

            migrationBuilder.AddColumn<DateTime>(
                name: "BaslangicTarihi",
                table: "StudentExams",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "BitisZamani",
                table: "StudentExams",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Tamamlandi",
                table: "StudentExams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Okundu",
                table: "Reminders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "CourseStudents",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseStudents",
                table: "CourseStudents",
                columns: new[] { "CourseId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_StudentId_ExamId",
                table: "StudentExams",
                columns: new[] { "StudentId", "ExamId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentExams_AspNetUsers_StudentId",
                table: "StudentExams",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentExams_AspNetUsers_StudentId",
                table: "StudentExams");

            migrationBuilder.DropIndex(
                name: "IX_StudentExams_StudentId_ExamId",
                table: "StudentExams");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseStudents",
                table: "CourseStudents");

            migrationBuilder.DropColumn(
                name: "BaslangicTarihi",
                table: "StudentExams");

            migrationBuilder.DropColumn(
                name: "BitisZamani",
                table: "StudentExams");

            migrationBuilder.DropColumn(
                name: "Tamamlandi",
                table: "StudentExams");

            migrationBuilder.DropColumn(
                name: "Okundu",
                table: "Reminders");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "CourseStudents",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseStudents",
                table: "CourseStudents",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExams_StudentId",
                table: "StudentExams",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseStudents_CourseId",
                table: "CourseStudents",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentExams_AspNetUsers_StudentId",
                table: "StudentExams",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
