using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using MvcCoreProceduresEF.Data;
using MvcCoreProceduresEF.Models;
using System.Data;
using System.Data.Common;
using static Azure.Core.HttpHeader;
using static System.Runtime.InteropServices.JavaScript.JSType;

#region PROCEDURES
//create procedure SP_ALL_ESPECIALIDADES
//AS
//	SELECT DISTINCT ESPECIALIDAD FROM DOCTOR
//GO

//EXEC SP_ALL_ESPECIALIDADES

//CREATE PROCEDURE SP_UPDATE_DOCTORES
//(@especialidad nvarchar(50), @incremento int)
//AS
//	UPDATE DOCTOR SET SALARIO = SALARIO + @incremento WHERE ESPECIALIDAD=@especialidad
//GO

//EXEC SP_UPDATE_DOCTORES 'Psiquiatría', 5

//CREATE PROCEDURE SP_DOCTORES_ESPECIALIDAD
//(@especialidad nvarchar(50))
//AS
//	SELECT * FROM DOCTOR WHERE ESPECIALIDAD=@especialidad
//GO

//EXEC SP_DOCTORES_ESPECIALIDAD 'Psiquiatría'
#endregion

namespace MvcCoreProceduresEF.Repositories
{
    public class RepositoryDoctores
    {
        private HospitalContext context;

        public RepositoryDoctores(HospitalContext context)
        {
            this.context = context;
        }

        public async Task<List<string>> GetEspecialidadesAsync()
        {
            using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
            {
                string sql = "SP_ALL_ESPECIALIDADES";
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;
                await com.Connection.OpenAsync();
                DbDataReader reader = await com.ExecuteReaderAsync();
                List<string> especialidades = new List<string>();

                while (await reader.ReadAsync())
                {
                    especialidades.Add(reader["ESPECIALIDAD"].ToString());
                }

                await reader.CloseAsync();
                await com.Connection.CloseAsync();
                return especialidades;
            }
        }

        public async Task UpdateSalarioDoctoresAsync(string especialidad, int salario)
        {
            string sql = "SP_UPDATE_DOCTORES @especialidad, @incremento";
            SqlParameter pamEspe = new SqlParameter("@especialidad", especialidad);
            SqlParameter pamSala = new SqlParameter("@incremento", salario);
            await this.context.Database.ExecuteSqlRawAsync(sql, pamEspe, pamSala);
        }

        public async Task UpdateSalarioSinProcedureAsync(string especialidad, int salario)
        {
            //Debemos recuperar los datos a modificar desde le context, no sirve extraer estos datos de un procedure y luego modificarlos con EF
            //Debes hacerlo o con un procedure o si modificas con el context, extraer los datos que modificas con context tambien.
            var consulta = from datos in this.context.Doctores where datos.Especialidad == especialidad select datos;
            List<Doctor> doctores = await consulta.ToListAsync();

            foreach (Doctor doctor in doctores)
            {
                doctor.Salario = doctor.Salario + salario;
            }
            await this.context.SaveChangesAsync();
        }

        public async Task<List<Doctor>> FindDoctoresAsync(string especialidad)
        {
            string sql = "SP_DOCTORES_ESPECIALIDAD @especialidad";
            SqlParameter pamEspe = new SqlParameter("@especialidad", especialidad);
            var consulta = this.context.Doctores.FromSqlRaw(sql, pamEspe);
            List<Doctor> doctores = await consulta.ToListAsync();
            return doctores;
        }
    }
}
