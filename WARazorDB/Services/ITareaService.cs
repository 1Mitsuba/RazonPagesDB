using WARazorDB.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WARazorDB.Services
{
    public interface ITareaService
    {
        Task<IEnumerable<Tarea>> GetAllTareasAsync();
        Task<IEnumerable<Tarea>> GetTareasActivasAsync();
        Task<IEnumerable<Tarea>> GetTareasByEstadoAsync(string estado);
        Task<Tarea> GetTareaByIdAsync(int id);
        Task<Tarea> CreateTareaAsync(Tarea tarea);
        Task<bool> UpdateTareaAsync(Tarea tarea);
        Task<bool> DeleteTareaAsync(int id);
        Task<bool> CompletarTareaAsync(int id);
        Task<bool> CancelarTareaAsync(int id);
        Task<int> GetCountByEstadoAsync(string estado);
    }
}