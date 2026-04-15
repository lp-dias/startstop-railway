using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StartStop.Data;
using StartStop.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

[Authorize]
public class MovimentacoesController : Controller
{
    private readonly StartStopContext _context;
    private readonly ILogger<MovimentacoesController> _logger;

    public MovimentacoesController(StartStopContext context, ILogger<MovimentacoesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // LISTAGEM
   public async Task<IActionResult> Index()
{
    var query = _context.Movimentacoes
        .Include(m => m.Motorista)
        .Include(m => m.Veiculo)
        .AsQueryable();

    // 🔒 Se o usuário for Motorista, mostra apenas suas movimentações
    if (User.IsInRole("Motorista"))
    {
        query = query.Where(m => m.Motorista.Nome == User.Identity.Name);
    }

    var movimentacoes = await query
        .OrderByDescending(m => m.DataRetirada)
        .ToListAsync();

    return View(movimentacoes);
}


    // DETALHES
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var mov = await _context.Movimentacoes
            .Include(m => m.Motorista)
            .Include(m => m.Veiculo)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (mov == null) return NotFound();
        return View(mov);
    }

    // CREATE (GET)
    public async Task<IActionResult> Create()
    {
        await PopularVeiculosDisponiveis();
        return View();
    }

    // RETIRADA
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRetirada(int veiculoId, string destino, int kmSaida)
    {
        if (string.IsNullOrWhiteSpace(destino))
        {
            TempData["MensagemErro"] = "Destino é obrigatório.";
            return RedirectToAction(nameof(Create));
        }

        var motorista = await _context.Motoristas
            .FirstOrDefaultAsync(m => m.Nome == User.Identity.Name && m.Ativo);

        if (motorista == null)
        {
            TempData["MensagemErro"] = "⚠️ Você não está vinculado a um motorista ativo. Contate o administrador.";
            return RedirectToAction(nameof(Create));
        }

        if (motorista.Status == "Indisponível")
        {
            TempData["MensagemErro"] = $"O motorista {motorista.Nome} já está em posse de um veículo e não pode retirar outro.";
            return RedirectToAction(nameof(Create));
        }

        var veiculo = await _context.Veiculos.FindAsync(veiculoId);
        if (veiculo == null || veiculo.Status == "Bloqueado" || veiculo.Status == "Indisponível")
        {
            TempData["MensagemErro"] = "Veículo não disponível para retirada.";
            return RedirectToAction(nameof(Create));
        }

        if (kmSaida < veiculo.KmAcumulado || kmSaida > veiculo.KmAcumulado + 2)
        {
            TempData["MensagemErro"] = "O Km informado não é válido. Verifique e tente novamente.";
            return RedirectToAction(nameof(Create));
        }

        var mov = new Movimentacao
        {
            MotoristaId = motorista.Id,
            VeiculoId = veiculo.Id,
            DataRetirada = DateTime.Now,
            Destino = destino.ToUpperInvariant(),
            KmSaida = kmSaida,
            TipoMovimentacao = "Retirada",
            Status = kmSaida > veiculo.KmAcumulado ? "KmTolerancia" : "Pendente"
        };

        veiculo.Status = "Indisponível";
        motorista.Status = "Indisponível";

        _context.Add(mov);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Movimentação {Id} criada: motorista {MotoristaId} retirou veículo {Placa} em {Data}", mov.Id, motorista.Id, veiculo.Placa, mov.DataRetirada);

        TempData["Mensagem"] = $"Motorista {motorista.Nome} retirou o veículo {veiculo.Placa} às {mov.DataRetirada} para {mov.Destino}.";
        return RedirectToAction(nameof(Index));
    }

    
 // DEVOLUÇÃO
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateDevolucao(int movimentacaoId, int? kmRetorno)
{
    var mov = await _context.Movimentacoes
        .Include(m => m.Veiculo)
        .Include(m => m.Motorista)
        .FirstOrDefaultAsync(m => m.Id == movimentacaoId);

    if (mov == null || mov.Status == "Cancelado")
    {
        TempData["MensagemErro"] = "Movimentação inválida.";
        return RedirectToAction(nameof(Index));
    }
     // 🔒 Verificação: apenas o motorista que retirou pode devolver
    if (mov.Motorista.Nome != User.Identity.Name)
    {
        TempData["MensagemErro"] = "Você não pode devolver um veículo que não retirou.";
        return RedirectToAction(nameof(Index));
    }

    if (!kmRetorno.HasValue)
    {
        TempData["MensagemErro"] = "Km de retorno é obrigatório na devolução.";
        return RedirectToAction(nameof(Index));
    }

    if (kmRetorno.Value < mov.KmSaida)
    {
        TempData["MensagemErro"] = "Km de retorno não pode ser menor que o de saída.";
        return RedirectToAction(nameof(Index));
    }

    mov.DataRetorno = DateTime.Now;
    mov.KmRetorno = kmRetorno.Value;
    mov.KmPercorrido = kmRetorno.Value - mov.KmSaida;
    mov.TempoPosse = (int)(mov.DataRetorno.Value - mov.DataRetirada).TotalMinutes;
    mov.TipoMovimentacao = "Devolucao";
    mov.Status = "Finalizado";

    mov.Veiculo.KmAcumulado = kmRetorno.Value;

    // Busca reserva ativa (em andamento ou futura)
    var reservaAtiva = await _context.Reservas
        .Where(r => r.VeiculoId == mov.VeiculoId && r.Status == "Ativa")
        .OrderBy(r => r.DataInicio)
        .FirstOrDefaultAsync();

    if (reservaAtiva != null)
    {
        mov.Veiculo.Status = "Reservado";
        TempData["Mensagem"] = $"Motorista {mov.Motorista.Nome} devolveu o veículo {mov.Veiculo.Placa}. Há uma reserva ativa, veículo permanece como Reservado.";
    }
    else
    {
        mov.Veiculo.Status = "Disponível";
        TempData["Mensagem"] = $"Motorista {mov.Motorista.Nome} devolveu o veículo {mov.Veiculo.Placa} às {mov.DataRetorno}. " +
                               $"Tempo de posse: {mov.TempoPosse} min. Km percorrido: {mov.KmPercorrido}.";
    }

    mov.Motorista.Status = "Disponível";

    _context.Update(mov);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Movimentação {Id} devolvida: motorista {MotoristaId} devolveu veículo {Placa} em {Data}", 
        mov.Id, mov.MotoristaId, mov.Veiculo.Placa, mov.DataRetorno);

    return RedirectToAction(nameof(Index));
}

    // CANCELAR RETIRADA
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarRetirada(int movimentacaoId, string motivo)
    {
        var mov = await _context.Movimentacoes
            .Include(m => m.Veiculo)
            .Include(m => m.Motorista)
            .FirstOrDefaultAsync(m => m.Id == movimentacaoId);

        if (mov == null || mov.Status != "Pendente")
        {
            TempData["MensagemErro"] = "Somente retiradas pendentes podem ser canceladas.";
            return RedirectToAction(nameof(Index));
        }

        mov.Status = "Cancelado";
        mov.MotivoCancelamento = motivo;

        mov.Veiculo.Status = "Disponível";
        mov.Motorista.Status = "Disponível";

        _context.Update(mov);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Movimentação {Id} cancelada: motorista {MotoristaId}, veículo {Placa}, motivo: {Motivo}, data: {Data}", mov.Id, mov.MotoristaId, mov.Veiculo.Placa, motivo, DateTime.Now);

        TempData["Mensagem"] = $"Retirada cancelada por {User.Identity.Name} em {DateTime.Now}. Veículo {mov.Veiculo.Placa} e motorista {mov.Motorista.Nome} liberados.";
        return RedirectToAction(nameof(Index));
    }

    // RELATÓRIO POR PERÍODO
    public async Task<IActionResult> RelatorioPeriodo(DateTime inicio, DateTime fim)
    {
        var registros = await _context.Movimentacoes
            .Include(m => m.Motorista)
            .Include(m => m.Veiculo)
            .Where(m => m.DataRetirada >= inicio && m.DataRetirada <= fim)
            .OrderByDescending(m => m.DataRetirada)
            .ToListAsync();

        return View(registros);
    }

        // RELATÓRIO POR MOTORISTA
    public async Task<IActionResult> RelatorioMotorista(int? motoristaId)
    {
        ViewBag.Motoristas = new SelectList(_context.Motoristas, "Id", "Nome");

        var query = _context.Movimentacoes
            .Include(m => m.Motorista)
            .Include(m => m.Veiculo)
            .AsQueryable();

        if (motoristaId.HasValue)
        {
            query = query.Where(m => m.MotoristaId == motoristaId.Value);
        }

        var registros = await query
            .OrderByDescending(m => m.DataRetirada)
            .ToListAsync();

        _logger.LogInformation("Relatório de movimentações por motorista {MotoristaId} gerado em {Data}", motoristaId, DateTime.Now);

        return View(registros);
    }

    // RELATÓRIO POR PLACA
    public async Task<IActionResult> RelatorioPlaca(string placa)
    {
        ViewBag.Placas = new SelectList(_context.Veiculos.Select(v => v.Placa).ToList());

        var query = _context.Movimentacoes
            .Include(m => m.Motorista)
            .Include(m => m.Veiculo)
            .AsQueryable();

        if (!string.IsNullOrEmpty(placa))
        {
            query = query.Where(m => m.Veiculo.Placa == placa);
        }

        var registros = await query
            .OrderByDescending(m => m.DataRetirada)
            .ToListAsync();

        _logger.LogInformation("Relatório de movimentações por placa {Placa} gerado em {Data}", placa, DateTime.Now);

        return View(registros);
    }

    // RELATÓRIO COMBINADO (Placa + Motorista)
    public async Task<IActionResult> RelatorioCombinado(string placa, int? motoristaId)
    {
        ViewBag.Placas = new SelectList(_context.Veiculos.Select(v => v.Placa).ToList());
        ViewBag.Motoristas = new SelectList(_context.Motoristas, "Id", "Nome");

        var query = _context.Movimentacoes
            .Include(m => m.Motorista)
            .Include(m => m.Veiculo)
            .AsQueryable();

        if (!string.IsNullOrEmpty(placa))
        {
            query = query.Where(m => m.Veiculo.Placa == placa);
        }

        if (motoristaId.HasValue)
        {
            query = query.Where(m => m.MotoristaId == motoristaId.Value);
        }

        var registros = await query
            .OrderByDescending(m => m.DataRetirada)
            .ToListAsync();

        _logger.LogInformation("Relatório combinado gerado para placa {Placa} e motorista {MotoristaId} em {Data}", placa, motoristaId, DateTime.Now);

        return View(registros);
    }

    // Método auxiliar para popular veículos disponíveis
    private async Task PopularVeiculosDisponiveis()
    {
        var veiculos = await _context.Veiculos
            .Where(v => v.Status == "Disponível")
            .Select(v => new { v.Id, v.Placa })
            .ToListAsync();

        ViewBag.VeiculosDisponiveis = new SelectList(veiculos, "Id", "Placa");
    }
}
