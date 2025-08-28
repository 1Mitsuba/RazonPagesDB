namespace WARazorDB.Models
{
    public class Tarea
    {
        public int idTarea { get; set; }
        public string nombreTarea { get; set; }
        public DateTime fechaVencimiento { get; set; }
        public string estado { get; set; }
        public int idUsuario { get; set; }
    }
}
