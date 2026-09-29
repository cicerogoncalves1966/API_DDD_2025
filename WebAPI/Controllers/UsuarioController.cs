using Aplicacao.Interfaces;
using Entidades.Entidades;
using Entidades.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using WebAPI.Models;
using WebAPI.Token;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IAplicacaoUsuario _IAplicacaoUsuario;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UsuarioController(IAplicacaoUsuario IAplicacaoUsuario, 
               SignInManager<ApplicationUser> signInManager,
               UserManager<ApplicationUser> userManager)
        {
            _IAplicacaoUsuario = IAplicacaoUsuario;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [Produces("application/json")]
        [HttpPost("/api/CriarTokenIdentity")]
        public async Task<IActionResult> CriarTokenIdentity([FromBody] LoginModel login)
        {
            if (string.IsNullOrWhiteSpace(login.email) || string.IsNullOrWhiteSpace(login.senha))
                return Unauthorized();

            var resultado = await
                _signInManager.PasswordSignInAsync(login.email, login.senha, false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                var idUsuario = await _IAplicacaoUsuario.RetornaIdUsuario(login.email);
                if (idUsuario == null)
                    return NotFound();

                // Secret_Key - Tem que ser uma chave de pelo menos 32 caracteres ---
                var token = new TokenJWTBuilder()
                 .AddSecurityKey(JwtSecurityKey.Create("Secret_Key-123456789012345678901"))
                 .AddSubject("Empresa - JCDev Net Core")
                 .AddIssuer("Teste.Securiry.Bearer")
                 .AddAudience("Teste.Securiry.Bearer")
                 .AddClaim("idUsuario", idUsuario)
                 .AddExpiry(5)
                 .Builder();

                return Ok(token.value);
            }
            else
            {
                return Unauthorized();
            }
        }

        [AllowAnonymous]
        [Produces("application/json")]
        [HttpPost("/api/AdicionaUsuarioIdentity")]
        public async Task<IActionResult> AdicionaUsuarioIdentity([FromBody] UserModel userModel)
        {
            if (string.IsNullOrWhiteSpace(userModel.email) || string.IsNullOrWhiteSpace(userModel.senha))
                return Ok("Falta alguns dados");

            var user = new ApplicationUser
            {
                UserName = userModel.email,
                Email = userModel.email,
                Celular = userModel.celular,
                Idade = userModel.idade,
                Tipo = TipoUsuario.Operacao,
            };
            var resultado = await _userManager.CreateAsync(user, userModel.senha);

            if (resultado.Errors.Any())
            {
                return Ok(resultado.Errors);
            }

            // Geração de Confirmação caso precise
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // retorno email 
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var resultado2 = await _userManager.ConfirmEmailAsync(user, code);

            if (resultado2.Succeeded)
                return Ok("Usuário Adicionado com Sucesso");
            else
                return Ok("Erro ao confirmar usuários");
        }
    }
}

#region Métodos sem Identity -  não criptografa os dados
//[AllowAnonymous]
//[Produces("application/json")]
//[HttpPost("/api/CriarToken")]
//public async Task<IActionResult> CriarToken([FromBody] userModel userModel)
//{
//    if (string.IsNullOrWhiteSpace(userModel.email) || string.IsNullOrWhiteSpace(userModel.senha))
//        return Unauthorized();

//    var resultado = await _IAplicacaoUsuario.ExisteUsuario(userModel.email, userModel.senha);
//    if (resultado)
//    {
//        var secKey = JwtSecurityKey.Create("Secret_Key-12345678901234567890123456789012");
//        var idUsuario = await _IAplicacaoUsuario.RetornaIdUsuario(userModel.email);
//        if (idUsuario == null)
//            return NotFound();
//        var token = new TokenJWTBuilder()
//        .AddSecurityKey(secKey)
//        .AddSubject("Empresa - Canal Dev Net Core")
//        .AddIssuer("Teste.Securiry.Bearer")
//        .AddAudience("Teste.Securiry.Bearer")
//        .AddClaim("idUsuario", idUsuario)
//        .AddExpiry(5)
//        .Builder();

//        return Ok(token.value);
//    }
//    else
//    {
//        return Unauthorized();
//    }
//}

//[AllowAnonymous]
//[Produces("application/json")]
//[HttpPost("/api/AdicionaUsuario")]
//public async Task<IActionResult> AdicionaUsuario([FromBody] userModel userModel)
//{
//    if (string.IsNullOrWhiteSpace(userModel.email) || string.IsNullOrWhiteSpace(userModel.senha))
//        return Ok("Falta alguns dados");

//    var resultado = await
//        _IAplicacaoUsuario.AdicionaUsuario(userModel.email, userModel.senha, userModel.idade, userModel.celular);

//    if (resultado)
//        return Ok("Usuário Adicionado com Sucesso");
//    else
//        return Ok("Erro ao adicionar usuário");
//}
#endregion