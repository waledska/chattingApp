using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chattingApp.Migrations
{
    public partial class modifyOnUserModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "imgURL",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imgURL",
                table: "AspNetUsers");
        }
    }
}
