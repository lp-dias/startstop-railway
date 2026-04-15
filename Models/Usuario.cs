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
        public int Id { get; set; } // PK

        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Digite um email válido.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Perfil é obrigatório.")]
        public PerfilUsuario Perfil { get; set; }

        // Navegação
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

        [StringLength(255)]
        public string? SenhaHash { get; set; }

        [NotMapped]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [Column(TypeName = "datetime")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime")]
        public DateTime? UltimaAtualizacao { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UltimoLogin { get; set; }

        public bool Ativo { get; set; } = true;

        public string? Observacoes { get; set; }

        // 🔑 Relacionamento 1:N com Motoristas
        public ICollection<Motorista> Motoristas { get; set; } = new List<Motorista>();

        // 🔑 Recuperação de senha
        [StringLength(255)]
        public string? TokenRecuperacao { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? TokenExpira { get; set; }
    }
}
