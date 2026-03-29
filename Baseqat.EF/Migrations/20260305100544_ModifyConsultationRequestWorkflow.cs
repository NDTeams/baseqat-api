using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Baseqat.EF.Migrations
{
    /// <inheritdoc />
    public partial class ModifyConsultationRequestWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "ConsultantId",
                table: "ConsultationRequests",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "ConsultationCategoryId",
                table: "ConsultationRequests",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ConsultationRequests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationRequests_ConsultationCategoryId",
                table: "ConsultationRequests",
                column: "ConsultationCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationRequests_UserId",
                table: "ConsultationRequests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultationRequests_AspNetUsers_UserId",
                table: "ConsultationRequests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultationRequests_ConsultationCategories_ConsultationCategoryId",
                table: "ConsultationRequests",
                column: "ConsultationCategoryId",
                principalTable: "ConsultationCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsultationRequests_AspNetUsers_UserId",
                table: "ConsultationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsultationRequests_ConsultationCategories_ConsultationCategoryId",
                table: "ConsultationRequests");

            migrationBuilder.DropIndex(
                name: "IX_ConsultationRequests_ConsultationCategoryId",
                table: "ConsultationRequests");

            migrationBuilder.DropIndex(
                name: "IX_ConsultationRequests_UserId",
                table: "ConsultationRequests");

            migrationBuilder.DropColumn(
                name: "ConsultationCategoryId",
                table: "ConsultationRequests");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ConsultationRequests");

            migrationBuilder.AlterColumn<long>(
                name: "ConsultantId",
                table: "ConsultationRequests",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
