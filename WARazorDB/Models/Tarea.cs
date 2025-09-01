using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WARazorDB.Models
{
    [Table("Tareas")]
    public class Tarea
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre de la tarea es obligatorio")]
        [Display(Name = "Nombre de la Tarea")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\s_-]+$", ErrorMessage = "El nombre solo puede contener letras, números, espacios, guiones o guiones bajos")]
        [Column("nombreTarea")]
        public string nombreTarea { get; set; }
        
        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
        [Display(Name = "Fecha de Vencimiento")]
        [DataType(DataType.Date)]
        [Column("fechaVencimiento")]
        [FutureDate(ErrorMessage = "La fecha de vencimiento debe ser igual o posterior a hoy")]
        public DateTime fechaVencimiento { get; set; }
        
        [Required(ErrorMessage = "El estado de la tarea es obligatorio")]
        [Display(Name = "Estado")]
        [RegularExpression("^(pendiente|en curso|finalizado|cancelado)$", ErrorMessage = "Estado inválido, debe ser: pendiente, en curso, finalizado o cancelado")]
        [Column("estado")]
        public string estado { get; set; }
        
        [Required(ErrorMessage = "El ID de usuario es obligatorio")]
        [Display(Name = "ID Usuario")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de usuario debe ser mayor a 0")]
        [Column("idUsuario")]
        public int idUsuario { get; set; }
        
        [NotMapped]
        public string EstadoFormateado 
        {
            get 
            {
                return estado?.ToLower() switch
                {
                    "pendiente" => "Pendiente",
                    "en curso" => "En Curso",
                    "finalizado" => "Finalizado",
                    "cancelado" => "Cancelado",
                    _ => estado
                };
            }
        }
        
        [NotMapped]
        public bool EsActiva => estado?.ToLower() == "pendiente" || estado?.ToLower() == "en curso";
        
        [NotMapped]
        public bool EstaVencida => fechaVencimiento < DateTime.Today && EsActiva;
    }
    
    // Validador personalizado para asegurar que la fecha sea igual o posterior a hoy
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTime date)
            {
                return date.Date >= DateTime.Today;
            }
            
            return false;
        }
    }
}
