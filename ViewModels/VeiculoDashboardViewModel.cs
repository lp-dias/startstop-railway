using System;
using System.ComponentModel.DataAnnotations;

namespace StartStop.ViewModels
{
    public class VeiculoDashboardViewModel
    {
        public int Id { get; set; } // Identificador único do veículo

        [Required]
        [StringLength(10)]
        public string Placa { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Disponível"; // Disponível, Indisponível ou Bloqueado

        // Data de retorno prevista de reservas ativas
        public DateTime? DataRetorno { get; set; }

        // Informações da movimentação ativa (quando indisponível)
        public string Motorista { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;

        public DateTime? DataRetirada { get; set; }

        // Data de fim da manutenção (quando bloqueado)
        public DateTime? DataFimManutencao { get; set; }

        // Diferencia reserva de retirada
        [StringLength(20)]
        public string TipoIndisponibilidade { get; set; } = string.Empty; // "Reserva" ou "Retirada"

        public string UsuarioNome { get; set; } = string.Empty;

        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }

        // Id da reserva ativa
        public int? ReservaId { get; set; }

        // Id da movimentação ativa
        public int? MovimentacaoId { get; set; }
    }
}
