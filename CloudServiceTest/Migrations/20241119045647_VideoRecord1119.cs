using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudServiceTest.Migrations
{
    /// <inheritdoc />
    public partial class VideoRecord1119 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Thumbnail",
                table: "VideoRecords",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Thumbnail",
                table: "VideoRecords");
        }
    }
}
