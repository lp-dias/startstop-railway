using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartStop.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenExpiraToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TokenExpira",
                table: "Usuarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenRecuperacao",
                table: "Usuarios",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenExpira",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TokenRecuperacao",
                table: "Usuarios");
        }
    }
}
