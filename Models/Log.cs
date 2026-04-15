using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StartStop.Models
{
    public class Log
    {
        public int Id { get; set; }

        [Required]
        public int MovimentacaoId { get; set; }
        public Movimentacao Movimentacao { get; set; } = null!;

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime DataCancelamento { get; set; }

        [Required(ErrorMessage = "O motivo é obrigatório.")]
        [StringLength(200)]
        public string Motivo { get; set; } = string.Empty;
    }
}
