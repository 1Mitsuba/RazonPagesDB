using Microsoft.EntityFrameworkCore;
using WARazorDB.Models;
using System;

namespace WARazorDB.Data
{
    public class TareaDbContext : DbContext
    {
        public TareaDbContext(DbContextOptions<TareaDbContext> options) : base(options)
        {
        }
        
        public DbSet<Tarea> Tareas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tarea>(entity =>
            {
                // Configurar explícitamente el nombre de la tabla
                entity.ToTable("Tareas");
                
                entity.HasKey(t => t.Id);
                entity.Property(t => t.nombreTarea).IsRequired().HasMaxLength(100);
                entity.Property(t => t.estado).IsRequired().HasMaxLength(20);
                entity.Property(t => t.fechaVencimiento).IsRequired();
                entity.Property(t => t.idUsuario).IsRequired();
                
                // Mapeo explícito de las columnas
                entity.Property(t => t.Id).HasColumnName("Id");
                entity.Property(t => t.nombreTarea).HasColumnName("nombreTarea");
                entity.Property(t => t.fechaVencimiento).HasColumnName("fechaVencimiento");
                entity.Property(t => t.estado).HasColumnName("estado");
                entity.Property(t => t.idUsuario).HasColumnName("idUsuario");
                
                // Datos de inicialización (Seeder) - Comentados para evitar conflictos con datos existentes
                /*
                entity.HasData(
                    new Tarea
                    {
                        Id = 1,
                        nombreTarea = "Completar proyecto de Razor Pages",
                        fechaVencimiento = DateTime.Now.AddDays(7),
                        estado = "pendiente",
                        idUsuario = 1
                    },
                    new Tarea
                    {
                        Id = 2,
                        nombreTarea = "Estudiar para examen de C#",
                        fechaVencimiento = DateTime.Now.AddDays(3),
                        estado = "en curso",
                        idUsuario = 1
                    },
                    new Tarea
                    {
                        Id = 3,
                        nombreTarea = "Entregar informe semanal",
                        fechaVencimiento = DateTime.Now.AddDays(-2),
                        estado = "finalizado",
                        idUsuario = 1
                    },
                    new Tarea
                    {
                        Id = 4,
                        nombreTarea = "Reunión con el equipo",
                        fechaVencimiento = DateTime.Now.AddDays(-5),
                        estado = "cancelado",
                        idUsuario = 1
                    }
                );
                */
            });
            
            base.OnModelCreating(modelBuilder);
        }

        protected TareaDbContext()
        {
        }
    }
}
