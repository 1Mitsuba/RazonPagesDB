using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace WARazorDB.Data
{
    public static class DbInitializer
    {
        public static void Initialize(TareaDbContext context, ILogger logger)
        {
            try
            {
                // Asegurar que la base de datos existe, pero no crearla si no existe
                // (no usar EnsureCreated para evitar problemas con migraciones)
                bool dbExists = context.Database.CanConnect();
                
                if (!dbExists)
                {
                    logger.LogInformation("La base de datos no existe. La aplicación utilizará la base de datos existente.");
                    return;
                }
                
                logger.LogInformation("Conectado a la base de datos existente.");
                
                // Verificar si ya existen datos en la base de datos
                if (context.Tareas.Any())
                {
                    logger.LogInformation("La base de datos ya contiene datos. No se inicializarán datos adicionales.");
                    return; // La base de datos ya tiene datos
                }
                
                logger.LogInformation("La base de datos está vacía. No se realizará ninguna acción de inicialización automática.");
                
                // No inicializar datos automáticamente para evitar conflictos con la base de datos existente
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");
            }
        }
    }
}