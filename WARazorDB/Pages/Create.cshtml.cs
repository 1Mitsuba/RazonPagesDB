using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WARazorDB.Data;
using WARazorDB.Models;
using Microsoft.Extensions.Logging;

namespace WARazorDB.Pages
{
    public class CreateModel : PageModel
    {
        private readonly WARazorDB.Data.TareaDbContext _context;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(WARazorDB.Data.TareaDbContext context, ILogger<CreateModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Inicializar la tarea con valores predeterminados
            Tarea = new Tarea
            {
                fechaVencimiento = DateTime.Today,
                estado = "pendiente",
                idUsuario = 1 // Usuario predeterminado
            };
            
            return Page();
        }

        [BindProperty]
        public Tarea Tarea { get; set; } = default!;
        
        // Lista de errores personalizados para mostrar al usuario
        public List<string> ValidationErrors { get; set; } = new List<string>();

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Iniciando creación de tarea desde la página Create");
            
            // Validaciones personalizadas adicionales
            if (string.IsNullOrWhiteSpace(Tarea.nombreTarea))
            {
                ModelState.AddModelError("Tarea.nombreTarea", "El nombre de la tarea es obligatorio.");
                ValidationErrors.Add("El nombre de la tarea es obligatorio.");
            } 
            else if (Tarea.nombreTarea.Length < 3)
            {
                ModelState.AddModelError("Tarea.nombreTarea", "El nombre de la tarea debe tener al menos 3 caracteres.");
                ValidationErrors.Add("El nombre de la tarea debe tener al menos 3 caracteres.");
            }
            else if (Tarea.nombreTarea.Length > 100)
            {
                ModelState.AddModelError("Tarea.nombreTarea", "El nombre de la tarea no puede exceder los 100 caracteres.");
                ValidationErrors.Add("El nombre de la tarea no puede exceder los 100 caracteres.");
            }
            
            // Validar que la fecha de vencimiento no sea anterior a hoy
            if (Tarea.fechaVencimiento < DateTime.Today)
            {
                ModelState.AddModelError("Tarea.fechaVencimiento", "La fecha de vencimiento no puede ser anterior a hoy.");
                ValidationErrors.Add("La fecha de vencimiento no puede ser anterior a hoy.");
            }
            
            // Validar formato del estado
            if (string.IsNullOrEmpty(Tarea.estado))
            {
                ModelState.AddModelError("Tarea.estado", "El estado es obligatorio.");
                ValidationErrors.Add("El estado es obligatorio.");
            }
            else if (!new[] { "pendiente", "en curso", "finalizado", "cancelado" }.Contains(Tarea.estado.ToLower()))
            {
                ModelState.AddModelError("Tarea.estado", "Estado inválido. Debe ser: pendiente, en curso, finalizado o cancelado.");
                ValidationErrors.Add("Estado inválido. Debe ser: pendiente, en curso, finalizado o cancelado.");
            }
            
            // Registrar todos los valores recibidos para diagnóstico
            _logger.LogInformation("Datos de tarea recibidos: Nombre={0}, Fecha={1}, Estado={2}, IdUsuario={3}",
                Tarea?.nombreTarea,
                Tarea?.fechaVencimiento,
                Tarea?.estado,
                Tarea?.idUsuario);
            
            // Si el modelo no es válido, mostrar errores y regresar a la página
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido al crear tarea desde la página Create");
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        _logger.LogWarning("Propiedad con error: {0}", state.Key);
                        foreach (var error in state.Value.Errors)
                        {
                            _logger.LogWarning("- Error: {0}", error.ErrorMessage);
                            if (!ValidationErrors.Contains(error.ErrorMessage))
                            {
                                ValidationErrors.Add(error.ErrorMessage);
                            }
                        }
                    }
                }
                
                TempData["ErrorMessage"] = "Error al crear la tarea. Por favor, corrija los errores indicados.";
                return Page();
            }

            // Aseguramos que la fecha de vencimiento no sea anterior a hoy
            if (Tarea.fechaVencimiento < DateTime.Today)
            {
                Tarea.fechaVencimiento = DateTime.Today;
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
                _context.Tareas.Add(Tarea);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Tarea creada correctamente: {0}, ID: {1}, Usuario: {2}", 
                    Tarea.nombreTarea, Tarea.Id, Tarea.idUsuario);
                TempData["SuccessMessage"] = "Tarea creada correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la tarea");
                TempData["ErrorMessage"] = $"Error al crear la tarea: {ex.Message}";
                
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Excepción interna");
                    ModelState.AddModelError(string.Empty, $"Detalle: {ex.InnerException.Message}");
                }
                
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
