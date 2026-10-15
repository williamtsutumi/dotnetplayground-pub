using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Security.Claims;
using WebApplication1.Application.Services.Feed;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class HomeController : Controller
{
    private readonly IFeedService _feedService;
    private readonly IMapper _mapper;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IFeedService feedService, IMapper mapper, ILogger<HomeController> logger)
    {
        _feedService = feedService;
        _mapper = mapper;
        _logger = logger;
    }


    //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    //[OutputCache(PolicyName = "AnonymousOnly", Duration = 300)]
    [AllowAnonymous]
    public IActionResult Index(int pagina = 1, string ordem = "maisantigos")
    {
        ViewData["PostContentReduced"] = true;

        var model = PrivateBuscarFeedInicial(pagina, ordem);
        return View(model);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult BuscarFeedInicial(int pagina = 1, string ordem = "maisantigos")
    {
        var model = PrivateBuscarFeedInicial(pagina, ordem);
        return Json(model);
    }

    private FeedModel PrivateBuscarFeedInicial(int pagina = 1, string ordem = "maisantigos")
    {
        var model = new FeedModel();

        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id);
        var resultado = _feedService.Buscar(id, pagina, ordem);
        model = _mapper.Map<FeedModel>(resultado.Valor);
        model.Ordem = ordem;

        return model;
    }

    public JsonResult Teste()
    {
        return Json(new { status = "success", message = "teste deu certo" });
    }
}
