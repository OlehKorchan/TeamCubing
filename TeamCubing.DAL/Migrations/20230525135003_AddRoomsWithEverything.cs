using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamCubing.DAL.Migrations
{
    public partial class AddRoomsWithEverything : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoomSolve",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolveNumber = table.Column<int>(type: "int", nullable: false),
                    Scramble = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoomId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomSolve", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomSolve_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoomSolveResult",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomSolveId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Time = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomSolveResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomSolveResult_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoomSolveResult_RoomSolve_RoomSolveId",
                        column: x => x.RoomSolveId,
                        principalTable: "RoomSolve",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomSolve_RoomId",
                table: "RoomSolve",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomSolveResult_RoomSolveId",
                table: "RoomSolveResult",
                column: "RoomSolveId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomSolveResult_UserId",
                table: "RoomSolveResult",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomSolveResult");

            migrationBuilder.DropTable(
                name: "RoomSolve");
        }
    }
}
