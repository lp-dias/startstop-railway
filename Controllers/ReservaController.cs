using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StartStop.Data;
using StartStop.Models;
using StartStop.ViewModels;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using System;

namespace StartStop.Controllers
{
    // Perfis que podem acessar o controller
    [Authorize(Roles = "Administrador,Motorista,Tecnico,PCO")]
    public class ReservaController : Controller
    {
        private readonly StartStopContext _context;
        private readonly ILogger<ReservaController> _logger;

        public ReservaController(StartStopContext context, ILogger<ReservaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Método auxiliar para atualizar reservas vencidas
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

        [HttpGet]
        public IActionResult AtualizarExpiradas()
        {
            AtualizarReservasExpiradas();
            return Ok("Reservas vencidas atualizadas com sucesso.");
        }

        // Criar reserva → apenas Administrador e PCO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reservar(ReservaViewModel model)
        {
            if (!(User.IsInRole("Administrador") || User.IsInRole("PCO")))
            {
                TempData["MensagemErro"] = "🚫 Você não tem permissão para reservar veículos. Apenas Administradores e PCO podem realizar esta operação.";
                return RedirectToAction("Index", "Dashboard");
            }

            if (!ModelState.IsValid)
            {
                TempData["MensagemErro"] = "Dados inválidos. Verifique os campos e tente novamente.";
                return RedirectToAction("Index", "Dashboard");
            }

            AtualizarReservasExpiradas();

            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (usuarioIdClaim == null)
            {
                TempData["MensagemErro"] = "Usuário não identificado.";
                return RedirectToAction("Index", "Dashboard");
            }

            int usuarioId = int.Parse(usuarioIdClaim.Value);

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            var veiculo = await _context.Veiculos.FindAsync(model.VeiculoId);

            if (usuario == null || veiculo == null)
            {
                TempData["MensagemErro"] = "Usuário ou veículo não encontrado.";
                return RedirectToAction("Index", "Dashboard");
            }

            var reserva = new Reserva
            {
                VeiculoId = model.VeiculoId,
                UsuarioId = usuarioId,
                DataInicio = model.DataInicio,
                DataFim = model.DataFim,
                Status = "Ativa"
            };

            _context.Reservas.Add(reserva);

            veiculo.Status = "Reservado";
            _context.Veiculos.Update(veiculo);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reserva {Id} criada para veículo {Placa} pelo usuário {UsuarioId} em {DataInicio}", reserva.Id, veiculo.Placa, usuarioId, DateTime.Now);

            TempData["Mensagem"] = $"✅ Reserva criada com sucesso para o veículo {veiculo.Placa}, de {model.DataInicio:dd/MM/yyyy HH:mm} até {model.DataFim:dd/MM/yyyy HH:mm}.";
            return RedirectToAction("Index", "Dashboard");
        }

        // Finalizar reserva → apenas Administrador e PCO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(int id)
        {
            if (!(User.IsInRole("Administrador") || User.IsInRole("PCO")))
            {
                TempData["MensagemErro"] = "🚫 Você não tem permissão para finalizar reservas. Apenas Administradores e PCO podem realizar esta operação.";
                return RedirectToAction("Index", "Dashboard");
            }

            AtualizarReservasExpiradas();

            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                reserva.Status = "Finalizada";
                _context.Reservas.Update(reserva);

                var veiculo = await _context.Veiculos.FindAsync(reserva.VeiculoId);
                if (veiculo != null)
                {
                    var reservaFutura = await _context.Reservas
                        .Where(r => r.VeiculoId == veiculo.Id && r.Status == "Ativa" && r.DataInicio >= DateTime.Now)
                        .OrderBy(r => r.DataInicio)
                        .FirstOrDefaultAsync();

                    veiculo.Status = reservaFutura != null ? "Reservado" : "Disponível";
                    _context.Veiculos.Update(veiculo);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Reserva {Id} finalizada para veículo {Placa} em {Data}", reserva.Id, veiculo?.Placa, DateTime.Now);

                TempData["Mensagem"] = $"✅ Reserva do veículo {veiculo?.Placa} finalizada com sucesso.";
            }

            return RedirectToAction("Index", "Dashboard");
        }

        // Listagem de reservas → apenas Administrador
[HttpGet]
[Authorize(Roles = "Administrador")]
public async Task<IActionResult> Index()
{
    AtualizarReservasExpiradas();

    var reservas = await _context.Reservas
        .Include(r => r.Veiculo)
        .Include(r => r.Usuario)
        .OrderByDescending(r => r.DataInicio)
        .ToListAsync();

    return View(reservas);
}


        // Cancelar reserva → apenas Administrador e PCO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            if (!(User.IsInRole("Administrador") || User.IsInRole("PCO")))
            {
                TempData["MensagemErro"] = "🚫 Você não tem permissão para cancelar reservas. Apenas Administradores e PCO podem realizar esta operação.";
                return RedirectToAction("Index", "Dashboard");
            }

            AtualizarReservasExpiradas();

            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                reserva.Status = "Cancelada";
                _context.Reservas.Update(reserva);

                var veiculo = await _context.Veiculos.FindAsync(reserva.VeiculoId);
                if (veiculo != null)
                {
                    var reservaFutura = await _context.Reservas
                        .Where(r => r.VeiculoId == veiculo.Id && r.Status == "Ativa" && r.DataInicio >= DateTime.Now)
                        .OrderBy(r => r.DataInicio)
                        .FirstOrDefaultAsync();

                    if (reservaFutura != null)
                    {
                        veiculo.Status = "Reservado";
                    }
                    else
                    {
                        // 🔎 Verifica se há movimentação em andamento
                        var movimentacaoAberta = await _context.Movimentacoes
                            .AnyAsync(m => m.VeiculoId == veiculo.Id && m.Status == "Ativa");

                        veiculo.Status = movimentacaoAberta ? "Indisponível" : "Disponível";
                    }

                    _context.Veiculos.Update(veiculo);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Reserva {Id} cancelada para veículo {Placa} em {Data}", reserva.Id, veiculo?.Placa, DateTime.Now);

                TempData["Mensagem"] = $"⚠️ Reserva do veículo {veiculo?.Placa} cancelada.";
            }

            return RedirectToAction("Index", "Dashboard");
            

            
        }
    }
}
