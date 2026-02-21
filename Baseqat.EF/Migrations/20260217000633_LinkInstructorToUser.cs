using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Baseqat.EF.Migrations
{
    /// <inheritdoc />
    public partial class LinkInstructorToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Instructors",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d",
                column: "ConcurrencyStamp",
                value: "d3cc8d34-b763-4b80-9676-4ad26ae45f23");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b6b1b6fd-ca5f-4efc-ae56-48bd128a9df7",
                column: "ConcurrencyStamp",
                value: "2e740617-d6da-4e37-ba66-7a67204d9482");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c2d3e4f5-a6b7-4c8d-9e0f-1a2b3c4d5e6f",
                column: "ConcurrencyStamp",
                value: "6dd7ef46-dbfc-4bd8-bfad-98d125c1454d");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d3e4f5a6-b7c8-4d9e-0f1a-2b3c4d5e6f7a",
                column: "ConcurrencyStamp",
                value: "97700999-1fcd-4513-987a-0ba00ff7385a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-4e0f-1a2b-3c4d5e6f7a8b",
                column: "ConcurrencyStamp",
                value: "1dd9a559-e296-47be-9578-1455162fbe16");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f0cfb77b-0890-4124-8598-6ff387e3d64e",
                column: "ConcurrencyStamp",
                value: "863687c2-5313-433b-8e83-226d86712e8f");

            migrationBuilder.CreateIndex(
                name: "IX_Instructors_UserId",
                table: "Instructors",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Instructors_AspNetUsers_UserId",
                table: "Instructors",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Instructors_AspNetUsers_UserId",
                table: "Instructors");

            migrationBuilder.DropIndex(
                name: "IX_Instructors_UserId",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Instructors");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d",
                column: "ConcurrencyStamp",
                value: "63541a15-2820-4769-ad8d-83ea2f1d20fe");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b6b1b6fd-ca5f-4efc-ae56-48bd128a9df7",
                column: "ConcurrencyStamp",
                value: "246f32ee-4b9e-4b62-9764-6bd8d88912b9");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c2d3e4f5-a6b7-4c8d-9e0f-1a2b3c4d5e6f",
                column: "ConcurrencyStamp",
                value: "bbd199f2-92d1-4b62-a42a-3bd6d87082f4");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d3e4f5a6-b7c8-4d9e-0f1a-2b3c4d5e6f7a",
                column: "ConcurrencyStamp",
                value: "061741a8-bc1a-4190-9342-2dc5ff953201");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-4e0f-1a2b-3c4d5e6f7a8b",
                column: "ConcurrencyStamp",
                value: "51f85ced-09b8-4a82-bc4c-9efbed8fe804");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f0cfb77b-0890-4124-8598-6ff387e3d64e",
                column: "ConcurrencyStamp",
                value: "a17c42d5-4832-4deb-9f28-fdaa199ff745");
        }
    }
}
