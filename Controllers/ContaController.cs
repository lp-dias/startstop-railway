using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using StartStop.Data;
using StartStop.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;




namespace StartStop.Controllers
{
    [Authorize] // garante que só usuários logados acessem
    public class ContaController : Controller
    {
        private readonly StartStopContext _context;

        public ContaController(StartStopContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult TrocarSenha()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TrocarSenha(TrocarSenhaViewModel model)
        {
            if (model.NovaSenha != model.ConfirmarSenha)
            {
                TempData["MensagemErro"] = "As senhas não conferem.";
                return View(model);
            }

            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.SenhaAtual, usuario.SenhaHash))
            {
                TempData["MensagemErro"] = "Senha atual incorreta.";
                return View(model);
            }

            // aplica hash na nova senha
            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(model.NovaSenha);

            _context.Update(usuario);
            await _context.SaveChangesAsync();

            TempData["Mensagem"] = "Senha alterada com sucesso!";
            return RedirectToAction("Index", "Home");
        }
    }
}
