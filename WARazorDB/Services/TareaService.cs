using WARazorDB.Models;
using WARazorDB.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace WARazorDB.Services
{
    public class TareaService : ITareaService
    {
        private readonly TareaDbContext _context;
        private readonly ILogger<TareaService> _logger;

        public TareaService(TareaDbContext context, ILogger<TareaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Tarea>> GetAllTareasAsync()
        {
            _logger.LogInformation("Obteniendo todas las tareas");
            
            // Usar AsNoTracking para mejorar el rendimiento y evitar problemas de caché de Entity Framework
            return await _context.Tareas
                .AsNoTracking()
                .OrderByDescending(t => t.fechaVencimiento)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tarea>> GetTareasActivasAsync()
        {
            _logger.LogInformation("Obteniendo tareas activas");
            
            // Usar AsNoTracking para mejorar el rendimiento y evitar problemas de caché de Entity Framework
            return await _context.Tareas
                .AsNoTracking()
                .Where(t => t.estado.ToLower() == "pendiente" || t.estado.ToLower() == "en curso")
                .OrderByDescending(t => t.fechaVencimiento)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tarea>> GetTareasByEstadoAsync(string estado)
        {
            _logger.LogInformation($"Obteniendo tareas por estado: {estado}");
            
            // Usar AsNoTracking para mejorar el rendimiento y evitar problemas de caché de Entity Framework
            return await _context.Tareas
                .AsNoTracking()
                .Where(t => t.estado.ToLower() == estado.ToLower())
                .OrderByDescending(t => t.fechaVencimiento)
                .ToListAsync();
        }

        public async Task<Tarea> GetTareaByIdAsync(int id)
        {
            _logger.LogInformation($"Obteniendo tarea por ID: {id}");
            return await _context.Tareas.FindAsync(id);
        }

        public async Task<Tarea> CreateTareaAsync(Tarea tarea)
        {
            _logger.LogInformation($"Creando nueva tarea: {tarea.nombreTarea}");
            
            // Validaciones adicionales
            if (string.IsNullOrWhiteSpace(tarea.nombreTarea))
            {
                throw new ArgumentException("El nombre de la tarea no puede estar vacío");
            }
            
            if (tarea.fechaVencimiento < DateTime.Today)
            {
                tarea.fechaVencimiento = DateTime.Today;
            }
            
            tarea.estado = tarea.estado.ToLower();
            
            if (tarea.idUsuario <= 0)
            {
                tarea.idUsuario = 1; // Usuario por defecto
            }
            
            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();
            
            // Desconectar la entidad para evitar problemas de caché
            _context.Entry(tarea).State = EntityState.Detached;
            
            return tarea;
        }

        public async Task<bool> UpdateTareaAsync(Tarea tarea)
        {
            _logger.LogInformation($"Actualizando tarea con ID: {tarea.Id}");
            
            try
            {
                // Validaciones adicionales
                if (string.IsNullOrWhiteSpace(tarea.nombreTarea))
                {
                    throw new ArgumentException("El nombre de la tarea no puede estar vacío");
                }
                
                tarea.estado = tarea.estado.ToLower();
                
                // Forzar que Entity Framework detecte los cambios
                _context.Entry(tarea).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                
                // Desconectar la entidad para evitar problemas de caché
                _context.Entry(tarea).State = EntityState.Detached;
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar tarea con ID: {tarea.Id}");
                return false;
            }
        }

        public async Task<bool> DeleteTareaAsync(int id)
        {
            _logger.LogInformation($"Eliminando tarea con ID: {id}");
            
            try
            {
                var tarea = await _context.Tareas.FindAsync(id);
                if (tarea == null)
                {
                    _logger.LogWarning($"No se encontró la tarea con ID: {id} para eliminar");
                    return false;
                }
                
                _context.Tareas.Remove(tarea);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar tarea con ID: {id}");
                return false;
            }
        }

        public async Task<bool> CompletarTareaAsync(int id)
        {
            _logger.LogInformation($"Marcando como completada la tarea con ID: {id}");
            
            try
            {
                var tarea = await _context.Tareas.FindAsync(id);
                if (tarea == null)
                {
                    _logger.LogWarning($"No se encontró la tarea con ID: {id}");
                    return false;
                }
                
                tarea.estado = "finalizado";
                await _context.SaveChangesAsync();
                
                // Desconectar la entidad para evitar problemas de caché
                _context.Entry(tarea).State = EntityState.Detached;
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al completar tarea con ID: {id}");
                return false;
            }
        }

        public async Task<bool> CancelarTareaAsync(int id)
        {
            _logger.LogInformation($"Marcando como cancelada la tarea con ID: {id}");
            
            try
            {
                var tarea = await _context.Tareas.FindAsync(id);
                if (tarea == null)
                {
                    _logger.LogWarning($"No se encontró la tarea con ID: {id}");
                    return false;
                }
                
                tarea.estado = "cancelado";
                await _context.SaveChangesAsync();
                
                // Desconectar la entidad para evitar problemas de caché
                _context.Entry(tarea).State = EntityState.Detached;
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cancelar tarea con ID: {id}");
                return false;
            }
        }

        public async Task<int> GetCountByEstadoAsync(string estado)
        {
            _logger.LogInformation($"Obteniendo conteo de tareas por estado: {estado}");
            return await _context.Tareas
                .AsNoTracking() // Para evitar caché
                .CountAsync(t => t.estado.ToLower() == estado.ToLower());
        }
    }
}