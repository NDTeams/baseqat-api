using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Baseqat.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseIdToSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseSections_Courses_CourseId",
                table: "CourseSections");

            migrationBuilder.AlterColumn<long>(
                name: "CourseId",
                table: "CourseSections",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseRequirements_CourseId",
                table: "CourseRequirements",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseRequirements_Courses_CourseId",
                table: "CourseRequirements",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSections_Courses_CourseId",
                table: "CourseSections",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseRequirements_Courses_CourseId",
                table: "CourseRequirements");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSections_Courses_CourseId",
                table: "CourseSections");

            migrationBuilder.DropIndex(
                name: "IX_CourseRequirements_CourseId",
                table: "CourseRequirements");

            migrationBuilder.AlterColumn<long>(
                name: "CourseId",
                table: "CourseSections",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSections_Courses_CourseId",
                table: "CourseSections",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }
    }
}
