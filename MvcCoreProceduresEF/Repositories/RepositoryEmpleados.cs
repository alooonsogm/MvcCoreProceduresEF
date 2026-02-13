using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using MvcCoreProceduresEF.Data;
using MvcCoreProceduresEF.Models;
using System.Data;

#region VISTAS/PROCEDURE
//create view V_EMPLEADOS_DEPARTAMENTOS
//as
//	SELECT CAST(ISNULL(ROW_NUMBER() OVER (ORDER BY EMP.APELLIDO), 0) AS INT) AS ID,
//    EMP.APELLIDO, EMP.OFICIO, EMP.SALARIO, DEPT.DNOMBRE AS DEPARTAMENTO, DEPT.LOC AS LOCALIDAD
//	FROM EMP INNER JOIN DEPT ON EMP.DEPT_NO=DEPT.DEPT_NO;
//go

//SELECT * FROM V_EMPLEADOS_DEPARTAMENTOS

//create view V_TRABAJADORES
//AS
//	SELECT EMP_NO AS IDTRABAJADOR, APELLIDO, OFICIO, SALARIO FROM EMP
//	UNION
//	SELECT DOCTOR_NO, APELLIDO, ESPECIALIDAD, SALARIO FROM DOCTOR
//	UNION
//	SELECT EMPLEADO_NO, APELLIDO, FUNCION, SALARIO FROM PLANTILLA
//GO

//SELECT * FROM V_TRABAJADORES

//CREATE PROCEDURE SP_TRABAJADORES_OFICIO
//(@oficio nvarchar(50), @personas int out, @media int out, @suma int out)
//AS
//	select * from V_TRABAJADORES where OFICIO=@oficio
//	select @personas = COUNT(IDTRABAJADOR), @media = AVG(SALARIO), @suma = SUM(SALARIO) FROM V_TRABAJADORES WHERE OFICIO=@oficio
//GO
#endregion

namespace MvcCoreProceduresEF.Repositories
{
    public class RepositoryEmpleados
    {
        HospitalContext context;

        public RepositoryEmpleados(HospitalContext context)
        {
            this.context = context;
        }

        public async Task<List<VistaEmpleado>> GetVistaEmpleadosAsync()
        {
            var consulta = from datos in this.context.Empleados select datos;
            return await consulta.ToListAsync();
        }

        public async Task<TrabajadoresModel> GetTrabajadoresModelAsync()
        {
            //Primero con Linq (sin procedure)
            var consulta = from datos in this.context.Trabajadores select datos;
            TrabajadoresModel model = new TrabajadoresModel();
            model.Trabajadores = await consulta.ToListAsync();
            model.Personas = await consulta.CountAsync();
            model.SumaSalarial = await consulta.SumAsync(z => z.Salario);
            model.MediaSalarial = (int) await consulta.AverageAsync(z => z.Salario);
            return model;
        }

        public async Task<List<string>> GetOficiosAsync()
        {
            var consulta = (from datos in this.context.Trabajadores select datos.Oficio).Distinct();
            return await consulta.ToListAsync();
        }

        public async Task<TrabajadoresModel> GetTrabajadoresModelOficioAsync(string oficio)
        {
            //Ya que tenemos model, vamos a llamarlo con EF
            //La unica diferencia cuando tenemos parametros de salida es indicar la palabra OUT 
            //en la declaracion de las variables
            string sql = "SP_TRABAJADORES_OFICIO @oficio, @personas out, @media out, @suma out";
            SqlParameter pamOfi = new SqlParameter("@oficio", oficio);
            SqlParameter pamPerso = new SqlParameter("@personas", -1);
            pamPerso.Direction = ParameterDirection.Output;
            SqlParameter pamMedia = new SqlParameter("@media", -1);
            pamMedia.Direction = ParameterDirection.Output;
            SqlParameter pamSuma = new SqlParameter("@suma", -1);
            pamSuma.Direction = ParameterDirection.Output;
            //Ejecutamos la consulta con el model FromSqlRaw
            var consulta = this.context.Trabajadores.FromSqlRaw(sql, pamOfi, pamPerso, pamMedia, pamSuma);
            TrabajadoresModel model = new TrabajadoresModel();
            //Hasta que no leemos los datos (reader.Close()), no devuelve los parametros de salida
            //Poner primero la lecura de datos consulta.ToListAsync().
            model.Trabajadores = await consulta.ToListAsync();
            model.Personas = int.Parse(pamPerso.Value.ToString());
            model.MediaSalarial = int.Parse(pamMedia.Value.ToString());
            model.SumaSalarial = int.Parse(pamSuma.Value.ToString());
            return model;
        }
    }
}
