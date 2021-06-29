using curso.web.mvc.Models.Usuarios;
using curso.web.mvc.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Refit;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace curso.web.mvc.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }
        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar(RegistrarUsuarioViewModelInput registrarUsuarioViewModelInput)
        {
            try
            {
                var usuario = await _usuarioService.Registrar(registrarUsuarioViewModelInput);

                ModelState.AddModelError("", $"Os dados foram cadastrados com sucesso para o login {usuario.Login}");
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            //var httpClientHandler = new HttpClientHandler();
            //httpClientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            
            //var httpClient = new HttpClient(httpClientHandler);
            //httpClient.BaseAddress = new Uri("https://localhost:5001/");
            //var registrarUsuarioViewModelInputJson = JsonConvert.SerializeObject(registrarUsuarioViewModelInput);
            //var httpContent = new StringContent(registrarUsuarioViewModelInputJson, Encoding.UTF8, "application/json");
            //var httpPost = httpClient.PostAsync("/api/v1/usuario/registrar", httpContent).GetAwaiter().GetResult();

            //if (httpPost.StatusCode == System.Net.HttpStatusCode.Created)
            //{
            //    ModelState.AddModelError("", "Os dados foram cadastrados com sucesso");
            //}
            //else
            //{
            //    ModelState.AddModelError("", "Erro ao cadastrar");
            //}

            return View();
        }

        public IActionResult Logar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logar(LoginViewModelInput loginViewModelInput)
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                var usuario = await _usuarioService.Logar(loginViewModelInput);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Usuario.Codigo.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Usuario.Login),
                    new Claim(ClaimTypes.Email, usuario.Usuario.Email),
                    new Claim("token", usuario.Token),
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authPropertier = new AuthenticationProperties
                {
                    ExpiresUtc = new DateTimeOffset(DateTime.UtcNow.AddDays(1))
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                ModelState.AddModelError("", $"O usuário está autenticado  {usuario.Token}");
            }
            catch (ApiException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View();
        }

        public IActionResult EfetuarLogout()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction($"{nameof(Logar)}");
        }
    }
}
