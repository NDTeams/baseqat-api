using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Baseqat.EF.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateInstructorInfoUpdatePayload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "RequestedYearsOfExperience",
                table: "Instructors");

            migrationBuilder.RenameColumn(
                name: "RequestedXUrl",
                table: "Instructors",
                newName: "InfoUpdatePayloadJson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InfoUpdatePayloadJson",
                table: "Instructors",
                newName: "RequestedXUrl");

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

            migrationBuilder.AddColumn<int>(
                name: "RequestedYearsOfExperience",
                table: "Instructors",
                type: "int",
                nullable: true);
        }
    }
}
