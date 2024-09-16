using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chattingApp.Migrations
{
    public partial class updateGroupAddImgUrlAttribute : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "imgUrl",
                table: "group",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imgUrl",
                table: "group");
        }
    }
}
