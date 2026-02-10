using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MvcCoreProceduresEF.Data;
using MvcCoreProceduresEF.Models;
using System.Data;
using System.Data.Common;

#region PROCEDURES
//create procedure SP_ALL_ENFERMOS
//AS
//	SELECT * FROM ENFERMO
//GO

//CREATE PROCEDURE SP_FIND_ENFERMO
//(@inscripcion nvarchar(50))
//as
//	select * from ENFERMO WHERE INSCRIPCION=@inscripcion
//go

//CREATE PROCEDURE SP_DELETE_ENFERMO
//(@inscripcion nvarchar(50))
//AS
//	DELETE FROM ENFERMO WHERE INSCRIPCION=@inscripcion
//GO
#endregion

namespace MvcCoreProceduresEF.Repositories
{
    public class RepositoryEnfermos
    {
        private EnfermoContext context;

        public RepositoryEnfermos(EnfermoContext context)
        {
            this.context = context;
        }

        public async Task<List<Enfermo>> GetEnfermosAsync()
        {
            //NECESITAMOS UN COMMAND, VAMOS  A  UTILIZAR  un  USING PARA TODO 
            //EL COMMAND, EN SU CREACION, NECESITA  DE  UNA  CADENA  DE  CONEXION (OBJETO) 
            //EL  OBJETO  CONNECTION  NOS  LO  OFRECE  EF, las  conexiones  se crean  a  partir  del  context 
            using (DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
            {
                string sql = "SP_ALL_ENFERMOS";
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;
                //ABRIMOS LA CONEXION A PARTIR DEL COMMAND 
                await com.Connection.OpenAsync();
                //EJECUTAMOS NUESTRO READER 
                DbDataReader reader = await com.ExecuteReaderAsync();
                //DEBEMOS MAPEAR LOS DATOS MANUALMENTE 
                List<Enfermo> enfermos = new List<Enfermo>();

                while (await reader.ReadAsync())
                {
                    Enfermo enfermo = new Enfermo
                    {
                        Inscripcion = reader["INSCRIPCION"].ToString(),
                        Apellido = reader["APELLIDO"].ToString(),
                        Direccion = reader["DIRECCION"].ToString(),
                        FechaNacimiento = DateTime.Parse(reader["FECHA_NAC"].ToString()),
                        Genero = reader["S"].ToString(),
                        Nss = reader["NSS"].ToString()
                    };
                    enfermos.Add(enfermo);
                }

                await reader.CloseAsync();
                await com.Connection.CloseAsync();
                return enfermos;
            }
        }
    }
}
