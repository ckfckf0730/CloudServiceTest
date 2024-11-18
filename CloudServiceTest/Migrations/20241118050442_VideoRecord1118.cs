using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudServiceTest.Migrations
{
    /// <inheritdoc />
    public partial class VideoRecord1118 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoType",
                table: "VideoRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoType",
                table: "VideoRecords");
        }
    }
}
