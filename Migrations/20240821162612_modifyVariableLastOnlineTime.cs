using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chattingApp.Migrations
{
    public partial class modifyVariableLastOnlineTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lastSeen",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<DateTime>(
                name: "lastOnlineTime",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lastOnlineTime",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "lastSeen",
                table: "AspNetUsers",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
