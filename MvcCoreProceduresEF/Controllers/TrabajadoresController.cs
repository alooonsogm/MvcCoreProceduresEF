using Microsoft.AspNetCore.Mvc;
using MvcCoreProceduresEF.Models;
using MvcCoreProceduresEF.Repositories;

namespace MvcCoreProceduresEF.Controllers
{
    public class TrabajadoresController : Controller
    {
        private RepositoryEmpleados repo;

        public TrabajadoresController(RepositoryEmpleados repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            List<string> oficios = await this.repo.GetOficiosAsync();
            ViewData["OFICIOS"] = oficios;
            TrabajadoresModel model = await this.repo.GetTrabajadoresModelAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string oficio)
        {
            List<string> oficios = await this.repo.GetOficiosAsync();
            ViewData["OFICIOS"] = oficios;
            TrabajadoresModel model = await this.repo.GetTrabajadoresModelOficioAsync(oficio);
            return View(model);
        }
    }
}
