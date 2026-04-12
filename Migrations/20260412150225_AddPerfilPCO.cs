using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartStop.Migrations
{
    public partial class AddPerfilPCO : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nenhuma alteração estrutural necessária.
            // Se quiser, pode atualizar dados existentes:
            // migrationBuilder.Sql("UPDATE Usuarios SET Perfil = 2 WHERE Perfil IS NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nenhum rollback necessário.
        }
    }
}
