using Microsoft.EntityFrameworkCore;
using TourMate.MessageService.Repositories.Context;
using TourMate.MessageService.Repositories.GenericRepository;
using TourMate.MessageService.Repositories.IRepositories;
using TourMate.MessageService.Repositories.Models;

namespace TourMate.MessageService.Repositories.Repositories
{
    public class FileRepository : GenericRepository<FileStorage>, IFileRepository
    {
        public FileRepository(TourMateMessageContext context) : base(context)
        {
        }

        public async Task<FileStorage?> GetFileOfMessageAsync(int messageId)
        {
            var result = await _context.FileStorages
                .Include(f => f.Messages)
                .Where(f => f.Messages.Any(m => m.MessageId == messageId))
                .FirstOrDefaultAsync();

            return result;
        }
    }
}
