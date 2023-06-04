using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamCubing.DAL.Migrations
{
    public partial class AddUserListToRooms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConnectedUserNames",
                table: "Room",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectedUserNames",
                table: "Room");
        }
    }
}
