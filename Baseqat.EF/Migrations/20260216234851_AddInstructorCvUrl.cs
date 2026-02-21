using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Baseqat.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddInstructorCvUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CvUrl",
                table: "Instructors",
                type: "nvarchar(max)",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CvUrl",
                table: "Instructors");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d",
                column: "ConcurrencyStamp",
                value: "37b6ec73-5d79-42c3-bcaa-6a8efee19f3b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b6b1b6fd-ca5f-4efc-ae56-48bd128a9df7",
                column: "ConcurrencyStamp",
                value: "16413658-c43e-4f31-b511-2d2f56d0437b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c2d3e4f5-a6b7-4c8d-9e0f-1a2b3c4d5e6f",
                column: "ConcurrencyStamp",
                value: "5deb139d-1e6c-4d2a-a8e5-e7c18712ebdb");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d3e4f5a6-b7c8-4d9e-0f1a-2b3c4d5e6f7a",
                column: "ConcurrencyStamp",
                value: "ecea8c99-005a-4e1d-8e2b-22cd5eb58c23");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-4e0f-1a2b-3c4d5e6f7a8b",
                column: "ConcurrencyStamp",
                value: "1205efc2-3c35-436d-b363-c58b2f9b3f6b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f0cfb77b-0890-4124-8598-6ff387e3d64e",
                column: "ConcurrencyStamp",
                value: "feed4c4e-6ff8-4f2e-8662-886bdc76b1f3");
        }
    }
}
