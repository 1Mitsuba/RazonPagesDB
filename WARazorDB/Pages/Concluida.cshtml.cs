using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WARazorDB.Data;
using WARazorDB.Models;

namespace WARazorDB.Pages
{
    public class ConcluidaModel : PageModel
    {
        private readonly WARazorDB.Data.TareaDbContext _context;
        private readonly ILogger<ConcluidaModel> _logger;

        public ConcluidaModel(WARazorDB.Data.TareaDbContext context, ILogger<ConcluidaModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IList<Tarea> Tareas { get; set; } = default!;
        
        [BindProperty(SupportsGet = true)]
        public string Buscar { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int PaginaActual { get; set; } = 1;
        
        [BindProperty(SupportsGet = true)]
        public int TamanoPagina { get; set; } = 5;
        
        public int TotalPaginas { get; set; }
        public int TotalTareas { get; set; }
        
        [TempData]
        public string SuccessMessage { get; set; }
        
        [TempData]
        public string ErrorMessage { get; set; }
        
        public List<int> OpcionesTamanoPagina { get; } = new List<int> { 5, 10, 15, 25, 50 };
        
        public async Task OnGetAsync(int pagina = 1, int tamanoPagina = 5, string buscar = "")
        {
            // Validar tamaño de página
            if (!OpcionesTamanoPagina.Contains(tamanoPagina))
            {
                tamanoPagina = 5;
            }
            
            TamanoPagina = tamanoPagina;
            PaginaActual = pagina < 1 ? 1 : pagina;
            Buscar = buscar;
            
            // Consulta base para obtener solo tareas finalizadas
            var consulta = _context.Tareas.Where(t => t.estado.ToLower() == "finalizado");
            
            // Aplicar filtro de búsqueda
            if (!string.IsNullOrEmpty(Buscar))
            {
                consulta = consulta.Where(t => t.nombreTarea.Contains(Buscar));
            }
            
            // Obtener el total de tareas finalizadas
            TotalTareas = await consulta.CountAsync();
            
            // Calcular páginas
            TotalPaginas = (int)Math.Ceiling(TotalTareas / (double)TamanoPagina);
            
            if (PaginaActual > TotalPaginas && TotalPaginas > 0)
            {
                PaginaActual = TotalPaginas;
            }
            
            // Aplicar paginación
            Tareas = await consulta
                .OrderByDescending(t => t.fechaVencimiento)
                .Skip((PaginaActual - 1) * TamanoPagina)
                .Take(TamanoPagina)
                .ToListAsync();
                
            _logger.LogInformation($"Mostrando página {PaginaActual} de {TotalPaginas} de tareas finalizadas, con {TamanoPagina} tareas por página");
        }
    }
}