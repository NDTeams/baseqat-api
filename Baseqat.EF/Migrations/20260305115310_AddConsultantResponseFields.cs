using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Baseqat.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultantResponseFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsultantNotes",
                table: "ConsultationRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConsultantResponse",
                table: "ConsultationRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuggestedDate",
                table: "ConsultationRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuggestedTime",
                table: "ConsultationRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsultantNotes",
                table: "ConsultationRequests");

            migrationBuilder.DropColumn(
                name: "ConsultantResponse",
                table: "ConsultationRequests");

            migrationBuilder.DropColumn(
                name: "SuggestedDate",
                table: "ConsultationRequests");

            migrationBuilder.DropColumn(
                name: "SuggestedTime",
                table: "ConsultationRequests");
        }
    }
}
