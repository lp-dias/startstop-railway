using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using StartStop.Data;
using StartStop.Models;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Administrador")]
public class MotoristasController : Controller
{
    private readonly StartStopContext _context;

    public MotoristasController(StartStopContext context)
    {
        _context = context;
    }

    // LISTAGEM
    public async Task<IActionResult> Index()
    {
        var motoristas = await _context.Motoristas
            .Include(m => m.Usuario)
            .ToListAsync();

        return View(motoristas);
    }

    // DETALHES
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var motorista = await _context.Motoristas
            .Include(m => m.Usuario)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (motorista == null) return NotFound();

        return View(motorista);
    }

    // CRIAÇÃO (GET)
    public async Task<IActionResult> Create()
    {
        var usuariosDisponiveis = await _context.Usuarios
            .Where(u => !_context.Motoristas
                .Any(m => m.UsuarioId == u.Id && m.Ativo && m.Empregado))
            .Select(u => new { u.Id, u.Nome })
            .ToListAsync();

        ViewBag.UsuariosDisponiveis = new SelectList(usuariosDisponiveis, "Id", "Nome");
        return View();
    }

    // CRIAÇÃO (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Motorista motorista)
    {
        if (_context.Motoristas.Any(m => m.UsuarioId == motorista.UsuarioId && m.Ativo && m.Empregado))
        {
            ModelState.AddModelError("UsuarioId", "Este usuário já está cadastrado como motorista ativo e empregado.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(motorista);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erro ao salvar motorista: {ex.Message}");
            }
        }

        var usuariosDisponiveis = await _context.Usuarios
            .Where(u => !_context.Motoristas
                .Any(m => m.UsuarioId == u.Id && m.Ativo && m.Empregado))
            .Select(u => new { u.Id, u.Nome })
            .ToListAsync();

        ViewBag.UsuariosDisponiveis = new SelectList(usuariosDisponiveis, "Id", "Nome", motorista.UsuarioId);
        return View(motorista);
    }

    // EDIÇÃO (GET)
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var motorista = await _context.Motoristas
            .Include(m => m.Usuario)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (motorista == null) return NotFound();

        return View(motorista);
    }

    // EDIÇÃO (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Motorista motorista)
    {
        if (id != motorista.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(motorista);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Motoristas.Any(e => e.Id == motorista.Id))
                    return NotFound();
                else
                    throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erro ao atualizar motorista: {ex.Message}");
            }
        }
        return View(motorista);
    }

    // EXCLUSÃO (GET)
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var motorista = await _context.Motoristas
            .Include(m => m.Usuario)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (motorista == null) return NotFound();

        return View(motorista);
    }

    // EXCLUSÃO (POST)
[HttpPost, ActionName("DeleteConfirmed")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    var motorista = await _context.Motoristas.FindAsync(id);
    if (motorista != null)
    {
        _context.Motoristas.Remove(motorista);
        await _context.SaveChangesAsync();
    }
    return RedirectToAction(nameof(Index));
}

}
