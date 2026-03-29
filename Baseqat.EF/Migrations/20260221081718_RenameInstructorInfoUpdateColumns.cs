using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Baseqat.EF.Migrations
{
    /// <inheritdoc />
    public partial class RenameInstructorInfoUpdateColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InfoUpdateSubmittedByUserId",
                table: "Instructors",
                newName: "SubmittedByUserId");

            migrationBuilder.RenameColumn(
                name: "InfoUpdateReviewedByUserId",
                table: "Instructors",
                newName: "ReviewedByUserId");

            migrationBuilder.RenameColumn(
                name: "InfoUpdateReviewedAt",
                table: "Instructors",
                newName: "ReviewedAt");

            migrationBuilder.RenameColumn(
                name: "InfoUpdateRequestStatus",
                table: "Instructors",
                newName: "RequestStatus");

            migrationBuilder.RenameColumn(
                name: "InfoUpdatePayloadJson",
                table: "Instructors",
                newName: "PayloadJson");

            migrationBuilder.RenameColumn(
                name: "InfoUpdateDenialReason",
                table: "Instructors",
                newName: "DenialReason");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubmittedByUserId",
                table: "Instructors",
                newName: "InfoUpdateSubmittedByUserId");

            migrationBuilder.RenameColumn(
                name: "ReviewedByUserId",
                table: "Instructors",
                newName: "InfoUpdateReviewedByUserId");

            migrationBuilder.RenameColumn(
                name: "ReviewedAt",
                table: "Instructors",
                newName: "InfoUpdateReviewedAt");

            migrationBuilder.RenameColumn(
                name: "RequestStatus",
                table: "Instructors",
                newName: "InfoUpdateRequestStatus");

            migrationBuilder.RenameColumn(
                name: "PayloadJson",
                table: "Instructors",
                newName: "InfoUpdatePayloadJson");

            migrationBuilder.RenameColumn(
                name: "DenialReason",
                table: "Instructors",
                newName: "InfoUpdateDenialReason");
        }
    }
}
