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
                    logger.LogInformation("La base de datos no existe. La aplicaci�n utilizar� la base de datos existente.");
                    return;
                }
                
                logger.LogInformation("Conectado a la base de datos existente.");
                
                // Verificar si ya existen datos en la base de datos
                if (context.Tareas.Any())
                {
                    logger.LogInformation("La base de datos ya contiene datos. No se inicializar�n datos adicionales.");
                    return; // La base de datos ya tiene datos
                }
                
                logger.LogInformation("La base de datos est� vac�a. No se realizar� ninguna acci�n de inicializaci�n autom�tica.");
                
                // No inicializar datos autom�ticamente para evitar conflictos con la base de datos existente
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ocurri� un error al inicializar la base de datos.");
            }
        }
    }
}