using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chattingApp.Migrations
{
    public partial class addLastSeenToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "lastSeen",
                table: "AspNetUsers",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lastSeen",
                table: "AspNetUsers");
        }
    }
}
