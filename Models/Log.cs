using System;
using System.ComponentModel.DataAnnotations;

namespace StartStop.Models
{
    public class Log
    {
        public int Id { get; set; }

        [Required]
        public int MovimentacaoId { get; set; }
        public Movimentacao Movimentacao { get; set; }

        [Required]
        public DateTime DataCancelamento { get; set; }

        [StringLength(200)]
        public string Motivo { get; set; }
    }
}
