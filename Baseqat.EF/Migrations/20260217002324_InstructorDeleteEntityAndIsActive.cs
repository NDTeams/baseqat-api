using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Baseqat.EF.Migrations
{
    /// <inheritdoc />
    public partial class InstructorDeleteEntityAndIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Instructors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Instructors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Instructors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Instructors",
                type: "bit",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Instructors");

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
        }
    }
}
