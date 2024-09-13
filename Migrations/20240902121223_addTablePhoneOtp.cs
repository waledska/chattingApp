using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chattingApp.Migrations
{
    public partial class addTablePhoneOtp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhoneOtps",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    phoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    otp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    validTo = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneOtps", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhoneOtps");
        }
    }
}
