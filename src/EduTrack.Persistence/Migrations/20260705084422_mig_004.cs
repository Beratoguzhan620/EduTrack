using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_004 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "StreamPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "StreamPosts");
        }
    }
}
