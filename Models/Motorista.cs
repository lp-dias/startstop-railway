using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartStop.Models
{
    public class Motorista
    {
        public int Id { get; set; } // PK da tabela Motoristas

        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Status é obrigatório.")]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Área é obrigatório.")]
        [StringLength(100)]
        public string Area { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Digite um email válido.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Telefone é obrigatório.")]
        [StringLength(20)]
        public string Telefone { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;
        public bool Empregado { get; set; } = true;

        // 🔑 FK para Usuarios
        [Required(ErrorMessage = "O campo Usuário é obrigatório.")]
        public int UsuarioId { get; set; }

        // Propriedade de navegação
        public Usuario? Usuario { get; set; }
    }
}
