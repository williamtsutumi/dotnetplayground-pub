using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Areas.Admin.Controllers;

public class UsuariosController : Controller
{
    public UsuariosController()
    {
    }

    public IActionResult Index()
    {
        return View();
    }
}
