using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrack.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_005 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InstructorId",
                table: "StreamPosts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_StreamPosts_InstructorId",
                table: "StreamPosts",
                column: "InstructorId");

            migrationBuilder.AddForeignKey(
                name: "FK_StreamPosts_Users_InstructorId",
                table: "StreamPosts",
                column: "InstructorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StreamPosts_Users_InstructorId",
                table: "StreamPosts");

            migrationBuilder.DropIndex(
                name: "IX_StreamPosts_InstructorId",
                table: "StreamPosts");

            migrationBuilder.DropColumn(
                name: "InstructorId",
                table: "StreamPosts");
        }
    }
}
