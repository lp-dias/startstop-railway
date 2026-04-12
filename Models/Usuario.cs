using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartStop.Models
{
    public enum PerfilUsuario
    {
        [Display(Name = "Administrador")]
        Administrador = 0,

        [Display(Name = "Técnico")]
        Tecnico = 1,

        [Display(Name = "Motorista")]
        Motorista = 2,
        
        PCO = 3
    }

    public class Usuario
    {
        public int Id { get; set; } // PK da tabela Usuarios

        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Digite um email válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Perfil é obrigatório.")]
        public PerfilUsuario Perfil { get; set; }

        // Navegação
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

        public string? SenhaHash { get; set; }

        [NotMapped]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? UltimaAtualizacao { get; set; }
        public DateTime? UltimoLogin { get; set; }
        public bool Ativo { get; set; } = true;
        public string? Observacoes { get; set; }

        // 🔑 Relacionamento 1:N com Motoristas
        public ICollection<Motorista> Motoristas { get; set; } = new List<Motorista>();

        // 🔑 Campos para recuperação de senha
        public string? TokenRecuperacao { get; set; }
        public DateTime? TokenExpira { get; set; }
    }
}
