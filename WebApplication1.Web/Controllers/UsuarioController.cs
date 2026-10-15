using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using WebApplication1.Application.Records;
using WebApplication1.Application.Services.Usuario;
using WebApplication1.Domain.Usuario.Enums;

namespace WebApplication1.Controllers;

public class UsuarioController : Controller
{
    private readonly IUsuarioService _usuarioService;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UsuarioController(IUsuarioService service, IMapper mapper, IStringLocalizer<SharedResource> localizer)
    {
        _usuarioService = service;
        _mapper = mapper;
        _localizer = localizer;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string usuario, string senha, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        var cadastrado = await _usuarioService.AutenticarAsync(usuario, senha);
        if (cadastrado is null)
        {
            ViewBag.Error = _localizer["InvalidLogin"].Value;
            return View();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, cadastrado.Id.ToString()),
            new Claim(ClaimTypes.Name, cadastrado.NomeUsuario)
        };

        if (cadastrado.Perfis.Any(p => p.Tipo == TipoPerfil.Admin))
        {
            claims.Add(new(ClaimTypes.Role, "Admin"));
        }

        if (cadastrado.Perfis.Any(p => p.Tipo == TipoPerfil.Regular))
        {
            claims.Add(new(ClaimTypes.Role, "Regular"));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl!);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Criar()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Delete()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId) || usuarioId <= 0)
            return Challenge();

        _usuarioService.Deletar(usuarioId);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }


    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Login", "Usuario");
    }

    [HttpPost]
    [AllowAnonymous]
    public JsonResult Salvar(SalvarUsuarioRequest request)
    {
        var resultado = _usuarioService.Salvar(request);

        if (!resultado.Sucesso)
        {
            return Json(new { type = "error", message = _localizer[resultado.CodigoErro!].Value });
        }

        return Json(new { type = "success", message = _localizer["RegistrationSuccess"].Value });
    }
}
