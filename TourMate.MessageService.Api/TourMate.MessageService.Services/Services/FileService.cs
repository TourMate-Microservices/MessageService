using TourMate.MessageService.Repositories.Models;
using TourMate.MessageService.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourMate.MessageService.Services.IServices;

namespace TourMate.MessageService.Services.Services
{
    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;
        public FileService(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }
        public async Task<FileStorage> GetFileAsync(string id)
        {
            return await _fileRepository.GetByIdAsync(id);
        }

        public async Task<FileStorage> GetFileOfMessage(int messageId)
        {
            return await _fileRepository.GetFileOfMessageAsync(messageId);
        }

        public async Task<bool> UploadFile(FileStorage fileStorage)
        {
            return await _fileRepository.CreateAsync(fileStorage);
        }
    }
}
