using Microsoft.AspNetCore.Mvc;
using MvcCoreProceduresEF.Models;
using MvcCoreProceduresEF.Repositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MvcCoreProceduresEF.Controllers
{
    public class DoctoresController : Controller
    {
        private RepositoryDoctores repo;

        public DoctoresController(RepositoryDoctores repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            List<string> especialidades = await this.repo.GetEspecialidadesAsync();
            ViewData["ESPECIALIDADES"] = especialidades;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string especialidad, int salario, string valueBoton)
        {
            List<string> especialidades = await this.repo.GetEspecialidadesAsync();
            ViewData["ESPECIALIDADES"] = especialidades;

            if (valueBoton == "update")
            {
                await this.repo.UpdateSalarioDoctoresAsync(especialidad, salario);
            }
            else if (valueBoton == "updateProcedure")
            {
                await this.repo.UpdateSalarioSinProcedureAsync(especialidad, salario);
            }

            List<Doctor> doctores = await this.repo.FindDoctoresAsync(especialidad);
            return View(doctores);
        }
    }
}
