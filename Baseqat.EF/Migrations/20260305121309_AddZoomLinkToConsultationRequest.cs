using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Baseqat.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddZoomLinkToConsultationRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ZoomLink",
                table: "ConsultationRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZoomLink",
                table: "ConsultationRequests");
        }
    }
}
