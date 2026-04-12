using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartStop.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaReservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tabela Reservas já criada manualmente no SQLite
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Reservas");
        }
    }
}
