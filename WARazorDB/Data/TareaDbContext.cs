using Microsoft.EntityFrameworkCore;
using WARazorDB.Models;
namespace WARazorDB.Data
{
    public class TareaDbContext : DbContext
    {
        public TareaDbContext(DbContextOptions<TareaDbContext> options) : base(options)
        {
        }
        public DbSet<Tarea> Tareas { get; set; }

        protected TareaDbContext()
        {
        }
    }
}
