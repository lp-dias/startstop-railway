using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StartStop.Data;
using StartStop.Models;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace StartStop.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly StartStopContext _context;

        public UsuariosController(StartStopContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            return View(await _context.Usuarios.ToListAsync());
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            return View();
        }

        /// POST: Usuarios/Create
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Usuario usuario)
{
    if (string.IsNullOrEmpty(usuario.Senha))
    {
        ModelState.AddModelError("Senha", "O campo Senha é obrigatório.");
    }

  if (ModelState.IsValid)
{
    usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);
    usuario.DataCriacao = DateTime.Now;

    // Não force o perfil, use o que veio do formulário
    // usuario.Perfil já vem preenchido pelo select da View

    _context.Add(usuario);
    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}


    return View(usuario);
}


        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Usuario usuario)
        {
            if (id != usuario.Id) return NotFound();

            ModelState.Remove("SenhaHash");
            ModelState.Remove("Senha");

            if (!User.IsInRole("Administrador"))
            {
                ModelState.Remove("Perfil");

                var perfilDb = (await _context.Usuarios.AsNoTracking()
                                   .FirstOrDefaultAsync(u => u.Id == id))?.Perfil;

                usuario.Perfil = perfilDb ?? usuario.Perfil;
            }

            if (ModelState.IsValid)
            {
                var usuarioDb = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
                if (usuarioDb == null) return NotFound();

                usuario.SenhaHash = !string.IsNullOrEmpty(usuario.Senha)
                    ? BCrypt.Net.BCrypt.HashPassword(usuario.Senha)
                    : usuarioDb.SenhaHash;

                usuario.UltimaAtualizacao = DateTime.Now;

                _context.Update(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // GET: Usuarios/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Usuarios/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Senha, usuario.SenhaHash))
            {
                ModelState.AddModelError("", "Usuário ou senha inválidos.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Perfil.ToString()) // enum convertido para string
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddHours(2)
                });

            return RedirectToAction("Index", "Dashboard");
        }

        // GET: Usuarios/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Usuarios");
        }
    }
}
