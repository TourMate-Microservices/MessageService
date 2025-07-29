using TourMate.MessageService.Repositories.GenericRepository;
using TourMate.MessageService.Repositories.Models;

namespace TourMate.MessageService.Repositories.IRepositories
{
    public interface IFileRepository : IGenericRepository<FileStorage>
    {
        Task<FileStorage?> GetFileOfMessageAsync(int messageId);
    }
}
