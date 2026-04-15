using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using StartStop.Data;
using StartStop.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StartStop.Controllers
{
    [Authorize(Roles = "Administrador,Motorista,Tecnico,PCO")]
    public class DashboardController : Controller
    {
        private readonly StartStopContext _context;

        public DashboardController(StartStopContext context)
        {
            _context = context;
        }

        // Atualiza reservas vencidas
        private void AtualizarReservasExpiradas()
        {
            var reservasExpiradas = _context.Reservas
                .Where(r => r.Status == "Ativa" && r.DataFim <= DateTime.Now)
                .ToList();

            foreach (var reserva in reservasExpiradas)
            {
                reserva.Status = "Finalizada";

                var veiculo = _context.Veiculos.Find(reserva.VeiculoId);
                if (veiculo != null)
                    veiculo.Status = "Disponível";
            }

            _context.SaveChanges();
        }

        public async Task<IActionResult> Index()
        {
            AtualizarReservasExpiradas();

            var veiculosDb = await _context.Veiculos
                .Include(v => v.Reservas).ThenInclude(r => r.Usuario)
                .Include(v => v.Movimentacoes).ThenInclude(m => m.Motorista)
                .ToListAsync();

            var veiculos = veiculosDb.Select(v =>
            {
                // Reserva ativa válida (não vencida)
                var reservaAtiva = v.Reservas
                    .Where(r => r.Status == "Ativa" && r.DataFim > DateTime.Now)
                    .OrderByDescending(r => r.DataFim)
                    .FirstOrDefault();

                // Movimentação ativa (retirada em andamento)
                var movimentacaoAtiva = v.Movimentacoes
                    .Where(m => m.Status == "Pendente")
                    .OrderByDescending(m => m.DataRetirada)
                    .FirstOrDefault();

                // Última manutenção
                var dataFimManutencao = _context.Manutencoes
                    .Where(man => man.VeiculoId == v.Id)
                    .OrderByDescending(man => man.DataFim)
                    .Select(man => man.DataFim)
                    .FirstOrDefault();

                return new VeiculoDashboardViewModel
                {
                    Id = v.Id,
                    Placa = v.Placa,
                    Status = v.Status,

                    // Dados da reserva
                    ReservaId = reservaAtiva?.Id,
                    UsuarioNome = reservaAtiva?.Usuario?.Nome,
                    DataInicio = reservaAtiva?.DataInicio,
                    DataFim = reservaAtiva?.DataFim,

                    // Dados da movimentação
                    MovimentacaoId = movimentacaoAtiva?.Id,
                    Motorista = movimentacaoAtiva?.Motorista?.Nome,
                    Destino = movimentacaoAtiva?.Destino,
                    DataRetirada = movimentacaoAtiva?.DataRetirada,

                    // Dados de manutenção
                    DataFimManutencao = dataFimManutencao,

                    // Tipo de indisponibilidade
                    TipoIndisponibilidade = movimentacaoAtiva != null ? "Retirada"
                                        : reservaAtiva != null ? "Reserva"
                                        : null
                };
            }).ToList();

            // Resumo estatístico
            ViewBag.Total = veiculos.Count;
            ViewBag.Disponiveis = veiculos.Count(v => v.Status == "Disponível");
            ViewBag.Indisponiveis = veiculos.Count(v => v.Status == "Indisponível");
            ViewBag.Bloqueados = veiculos.Count(v => v.Status == "Bloqueado");
            ViewBag.Reservados = veiculos.Count(v => v.Status == "Reservado");

            return View(veiculos);
        }
    }
}
