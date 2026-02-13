using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
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

//CREATE PROCEDURE SP_INSERT_ENFERMO
//(@apellido nvarchar(50), @direccion nvarchar(50), @fechaNacimiento DATETIME, @genero nvarchar(50), @nss nvarchar(50))
//AS
//	DECLARE @insc INT
//    SELECT @insc = MAX(CAST(INSCRIPCION AS INT)) + 1 FROM ENFERMO
//	insert into ENFERMO values (@insc, @apellido, @direccion, @fechaNacimiento, @genero, @nss)
//GO

//EXEC SP_INSERT_ENFERMO 'Alonso', 'Barcelona', '2024-05-11', 'M', '280862426'
#endregion

namespace MvcCoreProceduresEF.Repositories
{
    public class RepositoryEnfermos
    {
        private HospitalContext context;

        public RepositoryEnfermos(HospitalContext context)
        {
            this.context = context;
        }

        //El DbCommand se usa cuando no esta mapeado el model(el model es distinto a la base de datos)
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

        //De esta forma solo se puede hacer si el model esta mapeado con la base de datos (es igual la bd que el model).
        public async Task<Enfermo> FindEnfermoAsync(string inscripcion)
        {
            //Para llamar a un procedimiento que contiene parametros, la llamada se realiza
            //mediante el nombre del procedure y cada parametro a continuacion en la declaracion del sql.
            //SP_FIND_ENFERMO @inscripcion
            string sql = "SP_FIND_ENFERMO @inscripcion";
            SqlParameter pamInsc = new SqlParameter("@inscripcion", inscripcion);
            //Si los datos que devulven el procedure esta mapeados con un model, podemos utilizar el metodo
            //FromSqlRaw para recuperar directamente el modelo/s. No podemos consultar y extraer a la vez,
            //se debe hacer siempre en dos pasos.
            var consulta = await this.context.Enfermos.FromSqlRaw(sql, pamInsc).ToListAsync();
            //Debemos utilizar asEnumerable para extraer los datos.
            Enfermo enfermo = consulta.FirstOrDefault();
            return enfermo;
        }

        public async Task DeleteEnfermoAsync(string inscripcion)
        {
            string sql = "SP_DELETE_ENFERMO";
            SqlParameter pamInsc = new SqlParameter("@inscripcion", inscripcion);
            using(DbCommand com = this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;
                com.Parameters.Add(pamInsc);
                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();
                com.Parameters.Clear();
            }
        }

        public async Task DeleteEnfermoRawAsync(string inscripcion)
        {
            string sql = "SP_DELETE_ENFERMO @inscripcion";
            SqlParameter pamInsc = new SqlParameter("@inscripcion", inscripcion);
            await this.context.Database.ExecuteSqlRawAsync(sql, pamInsc);
        }

        public async Task InsertEnfermoAsync(string apellido, string direccion, DateTime fechaNaci, string genero, string nss)
        {
            string sql = "SP_INSERT_ENFERMO @apellido, @direccion, @fechaNacimiento, @genero, @nss";
            SqlParameter pamApe = new SqlParameter("@apellido", apellido);
            SqlParameter pamDirec = new SqlParameter("@direccion", direccion);
            SqlParameter pamFecha = new SqlParameter("@fechaNacimiento", fechaNaci);
            SqlParameter pamGene = new SqlParameter("@genero", genero);
            SqlParameter pamNss = new SqlParameter("@nss", nss);
            await this.context.Database.ExecuteSqlRawAsync(sql, pamApe, pamDirec, pamFecha, pamGene, pamNss);
        }
    }
}
