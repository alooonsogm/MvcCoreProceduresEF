using Microsoft.EntityFrameworkCore;
using MvcCoreProceduresEF.Models;

namespace MvcCoreProceduresEF.Data
{
    public class EnfermoContext: DbContext
    {
        public EnfermoContext(DbContextOptions<EnfermoContext> options) : base(options)
        {

        }

        public DbSet<Enfermo> Enfermos { get; set; }
    }
}
