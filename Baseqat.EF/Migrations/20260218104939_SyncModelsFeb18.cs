using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Baseqat.EF.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelsFeb18 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d",
                column: "ConcurrencyStamp",
                value: "e3842702-39ac-465b-9857-1f06a0458fae");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b6b1b6fd-ca5f-4efc-ae56-48bd128a9df7",
                column: "ConcurrencyStamp",
                value: "4dab779b-3fdc-477c-b0c3-5ee7ce7ca187");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c2d3e4f5-a6b7-4c8d-9e0f-1a2b3c4d5e6f",
                column: "ConcurrencyStamp",
                value: "914a769b-29be-4662-8a56-6c43afdc46f9");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d3e4f5a6-b7c8-4d9e-0f1a-2b3c4d5e6f7a",
                column: "ConcurrencyStamp",
                value: "a77220f3-6932-4c98-8953-955905d53612");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-4e0f-1a2b-3c4d5e6f7a8b",
                column: "ConcurrencyStamp",
                value: "005eaacd-078a-4010-89da-338939a5302f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f0cfb77b-0890-4124-8598-6ff387e3d64e",
                column: "ConcurrencyStamp",
                value: "ed39a736-39f1-4d3e-b68b-afa868b31682");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d",
                column: "ConcurrencyStamp",
                value: "b97494cf-00e0-49e7-8d02-3463938589a0");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b6b1b6fd-ca5f-4efc-ae56-48bd128a9df7",
                column: "ConcurrencyStamp",
                value: "1d8713a2-604d-44d1-bbe7-df52997b781d");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c2d3e4f5-a6b7-4c8d-9e0f-1a2b3c4d5e6f",
                column: "ConcurrencyStamp",
                value: "382d9867-69b6-4782-8d0c-520679f0cd3a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d3e4f5a6-b7c8-4d9e-0f1a-2b3c4d5e6f7a",
                column: "ConcurrencyStamp",
                value: "2bd7f527-7cef-47c3-bbde-fe47683a791a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e4f5a6b7-c8d9-4e0f-1a2b-3c4d5e6f7a8b",
                column: "ConcurrencyStamp",
                value: "f74a7254-b36c-42b3-a938-0a0262e3ec1e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f0cfb77b-0890-4124-8598-6ff387e3d64e",
                column: "ConcurrencyStamp",
                value: "8ebf4cc1-db1a-466d-ba99-dd601874b3b2");
        }
    }
}
