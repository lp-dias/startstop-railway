using System;

namespace StartStop.ViewModels
{
    public class VeiculoDashboardViewModel
    {
        public int Id { get; set; }           // Identificador único do veículo
        public string Placa { get; set; }     // Placa do veículo
        public string Status { get; set; }    // Disponível, Indisponível ou Bloqueado

        // Data de retorno prevista de reservas ativas
        public DateTime? DataRetorno { get; set; }

        // Informações da movimentação ativa (quando indisponível)
        public string Motorista { get; set; }
        public string Destino { get; set; }
        public DateTime? DataRetirada { get; set; }

        // Data de fim da manutenção (quando bloqueado)
        public DateTime? DataFimManutencao { get; set; }

        // 🚀 Novo campo para diferenciar reserva de retirada
        public string TipoIndisponibilidade { get; set; } // "Reserva" ou "Retirada"

        // 🚀 Adicione esta propriedade
        public string UsuarioNome { get; set; }

        public DateTime? DataInicio { get; set; }
public DateTime? DataFim { get; set; }

// 🚀 Novo campo: Id da reserva ativa
    public int? ReservaId { get; set; }

     public int? MovimentacaoId { get; set; }   // ✅ novo campo

    }
}
