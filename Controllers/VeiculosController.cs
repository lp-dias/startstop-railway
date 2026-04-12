using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StartStop.Data;
using StartStop.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StartStop.Controllers
{
    [Authorize] // exige login para qualquer ação
    public class VeiculosController : Controller
    {
        private readonly StartStopContext _context;

        public VeiculosController(StartStopContext context)
        {
            _context = context;
        }

        // GET: Veiculos
        public async Task<IActionResult> Index()
        {
            var veiculos = await _context.Veiculos.ToListAsync();
            return View(veiculos);
        }

        // GET: Veiculos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var veiculo = await _context.Veiculos.FirstOrDefaultAsync(m => m.Id == id);
            if (veiculo == null) return NotFound();

            return View(veiculo);
        }

        // GET: Veiculos/Create
        [Authorize(Roles = "Administrador,Tecnico")]
        public IActionResult Create() => View();

        // POST: Veiculos/Create
        [HttpPost]
        [Authorize(Roles = "Administrador,Tecnico")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Veiculo veiculo)
        {
            if (string.IsNullOrEmpty(veiculo.Status))
                veiculo.Status = "Disponível";

            if (!ModelState.IsValid)
                return View(veiculo);

            veiculo.Bloqueado = false;
            veiculo.DataInicio = null;
            veiculo.DataFim = null;
            veiculo.Oficina = null;

            _context.Veiculos.Add(veiculo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Veiculos/Edit/5
        [Authorize(Roles = "Administrador")] 
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var veiculo = await _context.Veiculos.FindAsync(id);
            if (veiculo == null) return NotFound();

            return View(veiculo);
        }

        // POST: Veiculos/Edit/5
        [HttpPost]
        [Authorize(Roles = "Administrador,Tecnico")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Veiculo veiculo)
        {
            if (id != veiculo.Id) return NotFound();

            var veiculoDb = await _context.Veiculos.FindAsync(id);
            if (veiculoDb == null) return NotFound();

            // 🚫 Impede bloquear veículo reservado
            if (veiculoDb.Status == "Reservado" && veiculo.Bloqueado && !veiculoDb.Bloqueado)
            {
                TempData["ToastMessage"] = "O veículo encontra-se reservado. Para bloqueá-lo, solicite o cancelamento da reserva.";
                return RedirectToAction(nameof(Edit), new { id = veiculoDb.Id });
            }

            if (User.IsInRole("Tecnico"))
            {
                veiculoDb.Bloqueado = veiculo.Bloqueado;
                veiculoDb.DataInicio = veiculo.DataInicio;
                veiculoDb.DataFim = veiculo.DataFim;
                veiculoDb.Oficina = veiculo.Oficina;
                veiculoDb.Status = veiculoDb.Bloqueado ? "Bloqueado" : "Disponível";
            }
            else if (User.IsInRole("Administrador"))
            {
                veiculoDb.Placa = veiculo.Placa;
                veiculoDb.KmAcumulado = veiculo.KmAcumulado;
                veiculoDb.Bloqueado = veiculo.Bloqueado;
                veiculoDb.DataInicio = veiculo.DataInicio;
                veiculoDb.DataFim = veiculo.DataFim;
                veiculoDb.Oficina = veiculo.Oficina;
                veiculoDb.Status = veiculoDb.Bloqueado ? "Bloqueado" : "Disponível";
            }

            _context.Update(veiculoDb);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // BLOQUEAR/DESBLOQUEAR VEÍCULO
        [HttpPost]
        [Authorize(Roles = "Tecnico,Administrador")]
        public async Task<IActionResult> ToggleBloqueio(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);
            if (veiculo == null) return NotFound();

            // 🚫 Impede bloquear veículo reservado
            if (veiculo.Status == "Reservado" && !veiculo.Bloqueado)
            {
                TempData["ToastMessage"] = "O veículo encontra-se reservado. Para bloqueá-lo, solicite o cancelamento da reserva.";
                return RedirectToAction(nameof(Index));
            }

            veiculo.Bloqueado = !veiculo.Bloqueado;

            if (veiculo.Bloqueado)
            {
                veiculo.DataInicio = DateTime.Now;
                veiculo.Status = "Bloqueado"; // ✅ volta a ser Bloqueado
                veiculo.Oficina = "Oficina";
                veiculo.DataFim = null;
            }
            else
            {
                veiculo.DataFim = DateTime.Now;
                veiculo.Status = "Disponível";
                veiculo.Oficina = null;
                veiculo.DataInicio = null;
            }

            _context.Update(veiculo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Veiculos/Delete/5
        [Authorize(Roles = "Administrador")] 
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var veiculo = await _context.Veiculos.FirstOrDefaultAsync(m => m.Id == id);
            if (veiculo == null) return NotFound();

            return View(veiculo);
        }

        // POST: Veiculos/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);
            if (veiculo != null)
            {
                _context.Veiculos.Remove(veiculo);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
