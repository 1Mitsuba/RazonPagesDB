using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WARazorDB.Data;
using WARazorDB.Models;

namespace WARazorDB.Pages
{
    public class EditModel : PageModel
    {
        private readonly WARazorDB.Data.TareaDbContext _context;
        private readonly ILogger<EditModel> _logger;

        public EditModel(WARazorDB.Data.TareaDbContext context, ILogger<EditModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Tarea Tarea { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Tareas == null)
            {
                return NotFound();
            }

            var tarea =  await _context.Tareas.FirstOrDefaultAsync(m => m.Id == id);
            if (tarea == null)
            {
                return NotFound();
            }
            Tarea = tarea;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Iniciando edición de tarea ID: {0}", Tarea?.Id);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido al editar tarea");
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
                return Page();
            }
            
            // Aseguramos que el estado esté en minúsculas para consistencia
            Tarea.estado = Tarea.estado.ToLower();
            
            // Asignar idUsuario si no se ha establecido
            if (Tarea.idUsuario <= 0)
            {
                Tarea.idUsuario = 1; // Usuario predeterminado
            }

            try
            {
                _context.Attach(Tarea).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Tarea editada correctamente: {0}, ID: {1}, Usuario: {2}", 
                    Tarea.nombreTarea, Tarea.Id, Tarea.idUsuario);
                TempData["SuccessMessage"] = "Tarea actualizada correctamente.";
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error de concurrencia al editar tarea");
                
                if (!TareaExists(Tarea.Id))
                {
                    return NotFound();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error de concurrencia. La tarea ha sido modificada por otro usuario.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar tarea");
                ModelState.AddModelError(string.Empty, $"Error al editar tarea: {ex.Message}");
                return Page();
            }

            return RedirectToPage("./Index");
        }

        private bool TareaExists(int id)
        {
          return (_context.Tareas?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
