using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Threading.Tasks;

namespace StartStop.Services
{
    public class EmailService
    {
        private readonly string _servidor;
        private readonly int _porta;
        private readonly string _usuario;
        private readonly string _senha;
        private readonly string _remetente;
        private readonly string _nomeRemetente;

        public EmailService(string servidor, int porta, string usuario, string senha, string remetente, string nomeRemetente)
        {
            _servidor = servidor;
            _porta = porta;
            _usuario = usuario;
            _senha = senha;
            _remetente = remetente;
            _nomeRemetente = nomeRemetente;
        }

        public async Task EnviarEmailAsync(string destinatario, string assunto, string mensagem)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_nomeRemetente, _remetente));
            email.To.Add(new MailboxAddress("", destinatario));
            email.Subject = assunto;
            email.Body = new TextPart("plain") { Text = mensagem };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_servidor, _porta, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_usuario, _senha);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
