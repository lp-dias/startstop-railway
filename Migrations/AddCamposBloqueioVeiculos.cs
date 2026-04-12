using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace StartStop.Migrations
{
    public partial class AddCamposBloqueioVeiculos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Bloqueado",
                table: "Veiculos",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataInicio",
                table: "Veiculos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataFim",
                table: "Veiculos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Oficina",
                table: "Veiculos",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Bloqueado", table: "Veiculos");
            migrationBuilder.DropColumn(name: "DataInicio", table: "Veiculos");
            migrationBuilder.DropColumn(name: "DataFim", table: "Veiculos");
            migrationBuilder.DropColumn(name: "Oficina", table: "Veiculos");
        }
    }
}
