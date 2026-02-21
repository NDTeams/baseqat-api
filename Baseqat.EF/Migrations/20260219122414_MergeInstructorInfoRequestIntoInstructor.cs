using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Baseqat.EF.Migrations
{
    /// <inheritdoc />
    public partial class MergeInstructorInfoRequestIntoInstructor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstructorInfoUpdateRequests");

            migrationBuilder.AddColumn<string>(
                name: "InfoUpdateDenialReason",
                table: "Instructors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfoUpdateRequestStatus",
                table: "Instructors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InfoUpdateReviewedAt",
                table: "Instructors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfoUpdateReviewedByUserId",
                table: "Instructors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfoUpdateSubmittedByUserId",
                table: "Instructors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedBio",
                table: "Instructors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedFacebookUrl",
                table: "Instructors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestedGender",
                table: "Instructors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedInstagramUrl",
                table: "Instructors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedLinkedInUrl",
                table: "Instructors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedName",
                table: "Instructors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedTitle",
                table: "Instructors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestedXUrl",
                table: "Instructors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestedYearsOfExperience",
                table: "Instructors",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InfoUpdateDenialReason",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "InfoUpdateRequestStatus",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "InfoUpdateReviewedAt",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "InfoUpdateReviewedByUserId",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "InfoUpdateSubmittedByUserId",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "RequestedBio",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "RequestedFacebookUrl",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "RequestedGender",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "RequestedInstagramUrl",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "RequestedLinkedInUrl",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "RequestedName",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "RequestedTitle",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "RequestedXUrl",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "RequestedYearsOfExperience",
                table: "Instructors");

            migrationBuilder.CreateTable(
                name: "InstructorInfoUpdateRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstructorId = table.Column<long>(type: "bigint", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DenialReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FacebookUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    InstagramUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    LinkedInUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubmittedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    XUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstructorInfoUpdateRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstructorInfoUpdateRequests_Instructors_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Instructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstructorInfoUpdateRequests_InstructorId",
                table: "InstructorInfoUpdateRequests",
                column: "InstructorId");
        }
    }
}
