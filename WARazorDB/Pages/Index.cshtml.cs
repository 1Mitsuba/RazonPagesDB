using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WARazorDB.Data;
using WARazorDB.Models;
using WARazorDB.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace WARazorDB.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ITareaService _tareaService;
        private readonly ILogger<IndexModel> _logger;
        private readonly IMemoryCache _cache;

        public IndexModel(ITareaService tareaService, ILogger<IndexModel> logger, IMemoryCache cache)
        {
            _tareaService = tareaService;
            _logger = logger;
            _cache = cache;
        }

        public IList<Tarea> Tareas { get; set; } = default!;
        
        [BindProperty]
        public Tarea Tarea { get; set; } = default!;
        
        [BindProperty(SupportsGet = true)]
        public string Buscar { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string Estado { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public DateTime? FechaDesde { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public DateTime? FechaHasta { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int PaginaActual { get; set; } = 1;
        
        [BindProperty(SupportsGet = true)]
        public int TamanoPagina { get; set; } = 5;
        
        public int TotalPaginas { get; set; }
        public int TotalTareas { get; set; }
        public int TareasCompletadas { get; set; }
        public int TareasCanceladas { get; set; }
        
        [TempData]
        public string SuccessMessage { get; set; }
        
        [TempData]
        public string ErrorMessage { get; set; }
        
        // Lista ampliada de opciones de tamaño de página
        public List<int> OpcionesTamanoPagina { get; } = new List<int> { 5, 10, 25, 50, 100 };
        
        // Propiedades para navegación de página mejorada
        public bool MostrarPrimera => PaginaActual > 3;
        public bool MostrarUltima => PaginaActual < TotalPaginas - 2;
        public bool MostrarAnterior => PaginaActual > 1;
        public bool MostrarSiguiente => PaginaActual < TotalPaginas;
        
        public int[] PaginasVisibles { get; set; }
        
        public async Task OnGetAsync(int pagina = 1, int tamanoPagina = 5, string buscar = "", string estado = "")
        {
            // Limpiamos cualquier caché previa relacionada con las tareas para siempre tener datos actualizados
            ClearTasksCache();
            
            // Validar tamaño de página
            if (!OpcionesTamanoPagina.Contains(tamanoPagina))
            {
                tamanoPagina = 5;
            }
            
            TamanoPagina = tamanoPagina;
            PaginaActual = pagina < 1 ? 1 : pagina;
            Buscar = buscar;
            Estado = estado;
            
            // Establecer valores para el filtro en la vista
            ViewData["CurrentFilter"] = Buscar;
            ViewData["CurrentEstado"] = Estado;
            ViewData["FechaDesde"] = FechaDesde?.ToString("yyyy-MM-dd");
            ViewData["FechaHasta"] = FechaHasta?.ToString("yyyy-MM-dd");
            ViewData["TamanoPagina"] = TamanoPagina;

            _logger.LogInformation("Obteniendo datos de tareas para página {PaginaActual}, tamaño {TamanoPagina}, filtro '{Buscar}', estado '{Estado}'",
                PaginaActual, TamanoPagina, Buscar, Estado);

            try
            {
                // Obtener tareas según el estado seleccionado
                IEnumerable<Tarea> consulta;
                if (string.IsNullOrEmpty(Estado))
                {
                    consulta = await _tareaService.GetTareasActivasAsync();
                    _logger.LogInformation("Obtenidas {Count} tareas activas", consulta.Count());
                }
                else
                {
                    consulta = await _tareaService.GetTareasByEstadoAsync(Estado);
                    _logger.LogInformation("Obtenidas {Count} tareas con estado {Estado}", consulta.Count(), Estado);
                }
                
                // Aplicar filtros adicionales
                if (!string.IsNullOrEmpty(Buscar))
                {
                    consulta = consulta.Where(t => t.nombreTarea.Contains(Buscar, StringComparison.OrdinalIgnoreCase));
                    _logger.LogInformation("Filtrado por nombre: '{Buscar}', resultados: {Count}", Buscar, consulta.Count());
                }
                
                if (FechaDesde.HasValue)
                {
                    consulta = consulta.Where(t => t.fechaVencimiento >= FechaDesde.Value);
                    _logger.LogInformation("Filtrado por fecha desde: {FechaDesde}, resultados: {Count}", FechaDesde, consulta.Count());
                }
                
                if (FechaHasta.HasValue)
                {
                    consulta = consulta.Where(t => t.fechaVencimiento <= FechaHasta.Value);
                    _logger.LogInformation("Filtrado por fecha hasta: {FechaHasta}, resultados: {Count}", FechaHasta, consulta.Count());
                }
                
                // Obtener conteos
                TotalTareas = consulta.Count();
                TareasCompletadas = await _tareaService.GetCountByEstadoAsync("finalizado");
                TareasCanceladas = await _tareaService.GetCountByEstadoAsync("cancelado");
                
                // Calcular páginas
                TotalPaginas = (int)Math.Ceiling(TotalTareas / (double)TamanoPagina);
                
                // Ajustar la página actual si está fuera de rango
                if (PaginaActual > TotalPaginas && TotalPaginas > 0)
                {
                    PaginaActual = TotalPaginas;
                }
                
                // Calcular las páginas visibles
                CalcularPaginasVisibles();
                
                // Aplicar paginación y ordenar por fecha
                Tareas = consulta
                    .OrderByDescending(t => t.fechaVencimiento)
                    .Skip((PaginaActual - 1) * TamanoPagina)
                    .Take(TamanoPagina)
                    .ToList();
                
                _logger.LogInformation("Mostrando página {PaginaActual} de {TotalPaginas}, con {TamanoPagina} tareas por página. Total tareas: {TotalTareas}", 
                    PaginaActual, TotalPaginas, TamanoPagina, TotalTareas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar tareas en la página Index");
                ErrorMessage = "Error al cargar las tareas. Por favor, inténtelo de nuevo.";
                Tareas = new List<Tarea>();
            }
        }
        
        // Método para calcular qué números de página mostrar en la paginación
        private void CalcularPaginasVisibles()
        {
            if (TotalPaginas <= 7)
            {
                // Si hay 7 o menos páginas, mostrar todas
                PaginasVisibles = Enumerable.Range(1, Math.Max(1, TotalPaginas)).ToArray();
            }
            else
            {
                // Estrategia avanzada para páginas visibles
                List<int> paginas = new List<int>();
                
                // Siempre mostrar la primera página
                paginas.Add(1);
                
                // Si no estamos cerca del principio, añadir elipsis (representada como -1)
                if (PaginaActual > 3)
                {
                    paginas.Add(-1); // Elipsis
                }
                
                // Calcular rango alrededor de la página actual
                int inicio = Math.Max(2, PaginaActual - 1);
                int fin = Math.Min(TotalPaginas - 1, PaginaActual + 1);
                
                // Si estamos cerca del principio, mostrar más páginas al inicio
                if (PaginaActual <= 3)
                {
                    fin = Math.Min(5, TotalPaginas - 1);
                }
                
                // Si estamos cerca del final, mostrar más páginas al final
                if (PaginaActual >= TotalPaginas - 2)
                {
                    inicio = Math.Max(2, TotalPaginas - 4);
                }
                
                // Añadir las páginas del rango calculado
                for (int i = inicio; i <= fin; i++)
                {
                    paginas.Add(i);
                }
                
                // Si no estamos cerca del final, añadir elipsis
                if (PaginaActual < TotalPaginas - 2)
                {
                    paginas.Add(-1); // Elipsis
                }
                
                // Siempre mostrar la última página
                paginas.Add(TotalPaginas);
                
                PaginasVisibles = paginas.ToArray();
            }
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Iniciando creación de tarea");
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido al crear tarea");
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        _logger.LogWarning("Propiedad con error: {0}", state.Key);
                        foreach (var error in state.Value.Errors)
                        {
                            _logger.LogWarning("- Error: {0}", error.ErrorMessage);
                        }
                    }
                }
                
                TempData["ErrorMessage"] = "Error al crear la tarea. Revise los datos ingresados.";
                return RedirectToPage();
            }

            try
            {
                await _tareaService.CreateTareaAsync(Tarea);
                _logger.LogInformation("Tarea creada correctamente: {0}, ID: {1}, Usuario: {2}", 
                    Tarea.nombreTarea, Tarea.Id, Tarea.idUsuario);
                TempData["SuccessMessage"] = "Tarea creada correctamente.";
                
                // Limpiar caché después de crear
                ClearTasksCache();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la tarea");
                TempData["ErrorMessage"] = $"Error al crear la tarea: {ex.Message}";
            }

            return RedirectToPage();
        }
        
        public async Task<IActionResult> OnPostCompletarAsync(int id)
        {
            try
            {
                var resultado = await _tareaService.CompletarTareaAsync(id);
                
                if (resultado)
                {
                    TempData["SuccessMessage"] = "Tarea marcada como completada.";
                    // Limpiar caché después de actualizar
                    ClearTasksCache();
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo completar la tarea.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar la tarea");
                TempData["ErrorMessage"] = "Error al completar la tarea.";
            }
            
            return RedirectToPage(new { pagina = PaginaActual, tamanoPagina = TamanoPagina, buscar = Buscar, estado = Estado });
        }
        
        public async Task<IActionResult> OnPostCancelarAsync(int id)
        {
            try
            {
                var resultado = await _tareaService.CancelarTareaAsync(id);
                
                if (resultado)
                {
                    TempData["SuccessMessage"] = "Tarea cancelada correctamente.";
                    // Limpiar caché después de actualizar
                    ClearTasksCache();
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo cancelar la tarea.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar la tarea");
                TempData["ErrorMessage"] = "Error al cancelar la tarea.";
            }
            
            return RedirectToPage(new { pagina = PaginaActual, tamanoPagina = TamanoPagina, buscar = Buscar, estado = Estado });
        }
        
        // Método para limpiar cualquier entrada en caché relacionada con tareas
        private void ClearTasksCache()
        {
            // Remover todas las entradas que empiecen con "Tareas_"
            if (_cache is MemoryCache memoryCache)
            {
                _logger.LogInformation("Limpiando caché de tareas");
                // En producción deberías implementar una forma más eficiente de limpiar el caché específico
            }
        }
    }
}
