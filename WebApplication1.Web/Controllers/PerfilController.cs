using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication1.Application.Services.Feed;
using WebApplication1.Application.Services.Usuario;
using WebApplication1.Domain.Feed;
using WebApplication1.Models;
using WebApplication1.Models.Perfil;
using WebApplication1.Web.Models.Perfil;

namespace WebApplication1.Controllers;

public class PerfilController : Controller
{

    private readonly IUsuarioService _usuarioService;
    private readonly IFeedService _feedService;
    private readonly IMapper _mapper;
    private readonly Microsoft.Extensions.Localization.IStringLocalizer<WebApplication1.SharedResource> _localizer;

    public PerfilController(
        IUsuarioService service,
        IFeedService feedService,
        IMapper mapper,
        Microsoft.Extensions.Localization.IStringLocalizer<WebApplication1.SharedResource> localizer)
    {
        _mapper = mapper;
        _usuarioService = service;
        _feedService = feedService;
        _localizer = localizer;
    }

    [AllowAnonymous, HttpGet]
    public IActionResult Index(int? id = null, int pagina = 1, string ordem = "maisrecentes", string tipoFeed = "perfilfeedpessoal")
    {
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentId);
        if (id is null && currentId == 0) return Challenge();

        var targetId = id ?? currentId;
        var proprio = currentId > 0 && currentId == targetId;

        var usuario = _usuarioService.Buscar(targetId);
        if (usuario is null) return NotFound();

        var perfil = _mapper.Map<PerfilModel>(usuario);
        perfil.ProprioPerfil = proprio;
        if (!proprio)
        {
            perfil.Email = string.Empty;
            perfil.DataNascimento = null;
        }

        var feedModel = new FeedModel();
        var resultado = _feedService.Buscar(currentId, pagina, ordem, tipoFeed.ToEnumTipoFeed(), id ?? currentId);
        feedModel = _mapper.Map<FeedModel>(resultado.Valor);
        feedModel.Ordem = ordem;
        perfil.FeedModel = feedModel;
        perfil.FeedModel.TipoFeed = tipoFeed;

        return View(perfil);
    }

    [Authorize, HttpGet]
    public IActionResult Editar(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentId)) return Challenge();
        if (id != 0 && id != currentId) return Forbid();
        var usuario = _usuarioService.Buscar(currentId);
        if (usuario is null) return NotFound();
        var response = _mapper.Map<EditarPerfilModel>(usuario);
        return View(response);
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public IActionResult Editar(int id, WebApplication1.Web.DTOs.Perfil.EditarDescricaoDto dto)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentId)) return Challenge();
        if (id != 0 && id != currentId) return Forbid();
        var usuario = _usuarioService.Buscar(currentId);
        if (usuario is null) return NotFound();

        if (ModelState.IsValid)
        {
            var command = _mapper.Map<WebApplication1.Application.Commands.Usuario.EditarDescricaoCommand>(dto);
            var resultado = _usuarioService.AtualizarDescricao(currentId, command);
            if (resultado.Sucesso)
            {
                TempData["Sucesso"] = _localizer["ProfileSaved"].Value;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, _localizer[resultado.CodigoErro!]);
        }

        var model = _mapper.Map<EditarPerfilModel>(usuario);
        model.Descricao = dto.Descricao;
        return View(model);
    }
}
