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

        public UsuarioController(IAplicacaoUsuario IAplicacaoUsuario, SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _IAplicacaoUsuario = IAplicacaoUsuario;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [Produces("application/json")]
        [HttpPost("/api/CriarToken")]
        public async Task<IActionResult> CriarToken([FromBody] Login login)
        {
            if (string.IsNullOrWhiteSpace(login.email) || string.IsNullOrWhiteSpace(login.senha))
                return Unauthorized();

            var resultado = await _IAplicacaoUsuario.ExisteUsuario(login.email, login.senha);
            if (resultado)
            {
                var token = new TokenJWTBuilder()
                    .AddSecurityKey(JwtSecurityKey.Create("Secret_Key-12345678"))
                .AddSubject("Empresa - Canal Dev Net Core")
                .AddIssuer("Teste.Securiry.Bearer")
                .AddAudience("Teste.Securiry.Bearer")
                .AddClaim("UsuarioAPINumero", "1")
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
        [HttpPost("/api/AdicionaUsuario")]
        public async Task<IActionResult> AdicionaUsuario([FromBody] Login login)
        {
            if (string.IsNullOrWhiteSpace(login.email) || string.IsNullOrWhiteSpace(login.senha))
                return Ok("Falta alguns dados");

            var resultado = await
                _IAplicacaoUsuario.AdicionaUsuario(login.email, login.senha, login.idade, login.celular);

            if (resultado)
                return Ok("Usuário Adicionado com Sucesso");
            else
                return Ok("Erro ao adicionar usuário");
        }


        [AllowAnonymous]
        [Produces("application/json")]
        [HttpPost("/api/CriarTokenIdentity")]
        public async Task<IActionResult> CriarTokenIdentity([FromBody] Login login)
        {
            if (string.IsNullOrWhiteSpace(login.email) || string.IsNullOrWhiteSpace(login.senha))
                return Unauthorized();

            var resultado = await
                _signInManager.PasswordSignInAsync(login.email, login.senha, false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                var token = new TokenJWTBuilder()
                 .AddSecurityKey(JwtSecurityKey.Create("Secret_Key-12345678"))
                 .AddSubject("Empresa - Canal Dev Net Core")
                 .AddIssuer("Teste.Securiry.Bearer")
                 .AddAudience("Teste.Securiry.Bearer")
                 .AddClaim("UsuarioAPINumero", "1")
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
        public async Task<IActionResult> AdicionaUsuarioIdentity([FromBody] Login login)
        {
            if (string.IsNullOrWhiteSpace(login.email) || string.IsNullOrWhiteSpace(login.senha))
                return Ok("Falta alguns dados");

            var user = new ApplicationUser
            {
                UserName = login.email,
                Email = login.email,
                Celular = login.celular,
                Tipo = TipoUsuario.Operacao,
            };
            var resultado = await _userManager.CreateAsync(user, login.senha);

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

#region - Escopo de código com validações externas
//using Aplicacao.Interfaces;
//using Entidades.Entidades;
//using Entidades.Enums;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.WebUtilities;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using WebAPI.Models;

//namespace WebAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]

//    public class UsuarioController : ControllerBase
//    {
//        private readonly IAplicacaoUsuario _IAplicacaoUsuario;
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;

//        public UsuarioController(IAplicacaoUsuario IAplicacaoUsuario, SignInManager<ApplicationUser> signInManager,
//            UserManager<ApplicationUser> userManager)
//        {
//            _IAplicacaoUsuario = IAplicacaoUsuario;
//            _userManager = userManager;
//            _signInManager = signInManager;
//        }

//        [AllowAnonymous]
//        [Produces("application/json")]
//        [HttpPost("/api/CriarToken")]
//        public async Task<IActionResult> CriarToken([FromBody] Login login)
//        {
//            if (string.IsNullOrWhiteSpace(login.email) || string.IsNullOrWhiteSpace(login.senha))
//                return Unauthorized();

//            var resultado = await _IAplicacaoUsuario.ExisteUsuario(login.email, login.senha);
//            if (resultado)
//            {
//                var claims = new[]
//                {
//                    new Claim(JwtRegisteredClaimNames.Sub, login.email),
//                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
//                };

//                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Secret_Key-12345678"));
//                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//                var token = new JwtSecurityToken(
//                    issuer: "Teste.Securiry.Bearer",
//                    audience: "Teste.Securiry.Bearer",
//                    claims: claims,
//                    expires: DateTime.Now.AddMinutes(30),
//                    signingCredentials: creds);

//                var meuToken = new JwtSecurityTokenHandler().WriteToken(token);
//                return Ok(meuToken);
//            }
//            else
//            {
//                return Unauthorized();
//            }
//        }

//        [AllowAnonymous]
//        [Produces("application/json")]
//        [HttpPost("/api/AdicionaUsuario")]
//        public async Task<IActionResult> AdicionaUsuario([FromBody] Login login)
//        {
//            if (string.IsNullOrWhiteSpace(login.email) || string.IsNullOrWhiteSpace(login.senha))
//                return Ok("Falta alguns dados");

//            var resultado = await
//                _IAplicacaoUsuario.AdicionaUsuario(login.email, login.senha, login.idade, login.celular);

//            if (resultado)
//                return Ok("Usuário Adicionado com Sucesso");
//            else
//                return Ok("Erro ao adicionar usuário");
//        }

//        [AllowAnonymous]
//        [Produces("application/json")]
//        [HttpPost("/api/CriarTokenIdentity")]
//        public async Task<IActionResult> CriarTokenIdentity([FromBody] Login login)
//        {
//            if (string.IsNullOrWhiteSpace(login.email) || string.IsNullOrWhiteSpace(login.senha))
//                return Unauthorized();

//            var resultado = await
//                _signInManager.PasswordSignInAsync(login.email, login.senha, false, lockoutOnFailure: false);

//            if (resultado.Succeeded)
//            {
//                var claims = new[]
//                {
//                    new Claim(JwtRegisteredClaimNames.Sub, login.email),
//                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
//                };

//                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Secret_Key-12345678"));
//                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//                var token = new JwtSecurityToken(
//                    issuer: "Teste.Securiry.Bearer",
//                    audience: "Teste.Securiry.Bearer",
//                    claims: claims,
//                    expires: DateTime.Now.AddMinutes(30),
//                    signingCredentials: creds);

//                var meuToken = new JwtSecurityTokenHandler().WriteToken(token);

//                return Ok(meuToken);
//            }
//            else
//            {
//                return Unauthorized();
//            }
//        }

//        [AllowAnonymous]
//        [Produces("application/json")]
//        [HttpPost("/api/AdicionaUsuarioIdentity")]
//        public async Task<IActionResult> AdicionaUsuarioIdentity([FromBody] Login login)
//        {
//            if (string.IsNullOrWhiteSpace(login.email) || string.IsNullOrWhiteSpace(login.senha))
//                return Ok("Falta alguns dados");

//            var user = new ApplicationUser
//            {
//                UserName = login.email,
//                Email = login.email,
//                Celular = login.celular,
//                Tipo = TipoUsuario.Operacao,
//            };
//            var resultado = await _userManager.CreateAsync(user, login.senha);

//            if (resultado.Errors.Any())
//            {
//                return Ok(resultado.Errors);
//            }

//            // Geração de Confirmação caso precise
//            var userId = await _userManager.GetUserIdAsync(user);
//            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
//            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

//            // retorno email 
//            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
//            var resultado2 = await _userManager.ConfirmEmailAsync(user, code);

//            if (resultado2.Succeeded)
//                return Ok("Usuário Adicionado com Sucesso");
//            else
//                return Ok("Erro ao confirmar usuários");

//        }
//    }
//}
#endregion