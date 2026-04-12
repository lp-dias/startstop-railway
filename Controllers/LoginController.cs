using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using StartStop.Data;
using StartStop.Models;
using StartStop.Services; // <- para EmailService
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using StartStop.ViewModels;

namespace StartStop.Controllers
{
    public class LoginController : Controller
    {
        private readonly StartStopContext _context;
        private readonly EmailService _emailService;

        public LoginController(StartStopContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

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
        public IActionResult Index() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (usuario != null && BCrypt.Net.BCrypt.Verify(model.Senha, usuario.SenhaHash))
            {
                AtualizarReservasExpiradas();

                usuario.UltimoLogin = DateTime.Now;
                _context.Update(usuario);
                await _context.SaveChangesAsync();

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                string role = usuario.Perfil switch
                {
                    PerfilUsuario.Administrador => "Administrador",
                    PerfilUsuario.Tecnico => "Tecnico",
                    PerfilUsuario.Motorista => "Motorista",
                    PerfilUsuario.PCO => "PCO",
                    _ => "Motorista"
                };

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nome),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddHours(2)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Usuário ou senha inválidos");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }

        [HttpGet]
        public IActionResult AcessoNegado() => View();

        // 📧 Recuperação de senha
        [HttpGet]
        public IActionResult EsqueciSenha() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EsqueciSenha(string email)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null)
            {
                TempData["MensagemErro"] = "Email não encontrado.";
                return View();
            }

            var token = Guid.NewGuid().ToString();
            usuario.TokenRecuperacao = token;
            usuario.TokenExpira = DateTime.Now.AddMinutes(30);

            _context.Update(usuario);
            await _context.SaveChangesAsync();

            var link = Url.Action("RedefinirSenha", "Login", new { token = token }, Request.Scheme);

            await _emailService.EnviarEmailAsync(email, "Recuperação de senha",
                $"Olá {usuario.Nome},\n\nClique no link abaixo para redefinir sua senha:\n{link}\n\nEste link expira em 30 minutos.");

            TempData["Mensagem"] = "Um link de recuperação foi enviado para seu e-mail.";
            return RedirectToAction("Index", "Login");
        }

        [HttpGet]
        public IActionResult RedefinirSenha(string token)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.TokenRecuperacao == token && u.TokenExpira > DateTime.Now);
            if (usuario == null)
            {
                return RedirectToAction("TokenExpirado");
            }

            return View(new RedefinirSenhaViewModel { Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedefinirSenha(RedefinirSenhaViewModel model)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.TokenRecuperacao == model.Token && u.TokenExpira > DateTime.Now);
            if (usuario == null)
            {
                return RedirectToAction("TokenExpirado");
            }

            if (model.NovaSenha != model.ConfirmarSenha)
            {
                TempData["MensagemErro"] = "As senhas não conferem.";
                return View(model);
            }

            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(model.NovaSenha);
            usuario.TokenRecuperacao = null;
            usuario.TokenExpira = null;

            _context.Update(usuario);
            await _context.SaveChangesAsync();

            TempData["Mensagem"] = "Senha redefinida com sucesso!";
            return RedirectToAction("Index", "Login");
        }

        [HttpGet]
        public IActionResult TokenExpirado()
        {
            return View();
        }
    }
}
