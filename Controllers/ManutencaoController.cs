using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StartStop.Data;
using StartStop.Models;
using System.Threading.Tasks;
using System.Linq;

[Authorize(Roles = "Tecnico,Administrador")] // apenas técnicos e administradores
public class ManutencoesController : Controller
{
    private readonly StartStopContext _context;

    public ManutencoesController(StartStopContext context)
    {
        _context = context;
    }

    // LISTAGEM
    public async Task<IActionResult> Index()
    {
        var manutencoes = await _context.Manutencoes
            .Include(m => m.Veiculo) // traz dados do veículo junto
            .ToListAsync();

        return View(manutencoes);
    }

    // DETALHES
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var manutencao = await _context.Manutencoes
            .Include(m => m.Veiculo)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (manutencao == null) return NotFound();

        return View(manutencao);
    }

    // CRIAÇÃO
    public IActionResult Create()
    {
        ViewBag.Veiculos = _context.Veiculos.ToList(); // dropdown de veículos
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Manutencao manutencao)
    {
        if (ModelState.IsValid)
        {
            _context.Add(manutencao);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Veiculos = _context.Veiculos.ToList();
        return View(manutencao);
    }

    // EDIÇÃO
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var manutencao = await _context.Manutencoes.FindAsync(id);
        if (manutencao == null) return NotFound();

        ViewBag.Veiculos = _context.Veiculos.ToList();
        return View(manutencao);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Manutencao manutencao)
    {
        if (id != manutencao.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(manutencao);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Manutencoes.AnyAsync(e => e.Id == manutencao.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Veiculos = _context.Veiculos.ToList();
        return View(manutencao);
    }

    // EXCLUSÃO
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var manutencao = await _context.Manutencoes
            .Include(m => m.Veiculo)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (manutencao == null) return NotFound();

        return View(manutencao);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var manutencao = await _context.Manutencoes.FindAsync(id);
        if (manutencao != null)
        {
            _context.Manutencoes.Remove(manutencao);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
