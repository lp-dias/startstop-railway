using System;

namespace StartStop.Models
{
    public class Movimentacao
    {
        public int Id { get; set; }

        public int MotoristaId { get; set; }
        public Motorista Motorista { get; set; } = null!;

        public int VeiculoId { get; set; }
        public Veiculo Veiculo { get; set; } = null!;

        public DateTime DataRetirada { get; set; }
        public DateTime? DataRetorno { get; set; }

        // Destino é obrigatório
        public string Destino { get; set; } = string.Empty;

        public int KmSaida { get; set; }

        // KmRetorno só é obrigatório na devolução
        public int? KmRetorno { get; set; }

        // MotivoCancelamento pode ser nulo
        public string? MotivoCancelamento { get; set; }

        // Valores padrão para evitar nulos
        public string TipoMovimentacao { get; set; } = "Retirada";
        public string Status { get; set; } = "Pendente";

        public int? KmPercorrido { get; set; }
        public int? TempoPosse { get; set; }
    }
}
