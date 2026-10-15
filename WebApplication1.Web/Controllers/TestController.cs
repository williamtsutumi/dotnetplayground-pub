using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Web.Controllers;


// Qualquer coisa pode sair desse controller
public class TesteController : Controller
{
    [HttpGet("/Teste/AngularJS")]
    public IActionResult Index()
    {
        return View();
    }
}
