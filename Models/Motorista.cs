using System.ComponentModel.DataAnnotations;

namespace StartStop.Models
{
    public class Motorista
    {
        public int Id { get; set; } // PK da tabela Motoristas

        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Status é obrigatório.")]
        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Área é obrigatório.")]
        public string Area { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Digite um email válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Telefone é obrigatório.")]
        public string Telefone { get; set; } = string.Empty;

        public bool Ativo { get; set; }
        public bool Empregado { get; set; }

        // 🔑 FK para Usuarios
        [Required(ErrorMessage = "O campo Usuário é obrigatório.")]
        public int UsuarioId { get; set; }

        // Propriedade de navegação (não obrigatória)
        public Usuario? Usuario { get; set; }
    }
}
