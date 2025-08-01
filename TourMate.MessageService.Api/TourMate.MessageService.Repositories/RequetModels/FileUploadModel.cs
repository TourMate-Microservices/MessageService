﻿using TourMate.MessageService.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TourMate.MessageService.Repositories.RequestModels
{
    public class FileUploadModel
    {
        public string FileName { get; set; }
        public string DownloadUrl { get; set; }
        public FileStorage Convert() => new FileStorage
        {
            FileId = 0, // Assuming FileId is auto-generated by the database
            FileName = FileName,
            UploadTime = DateTime.UtcNow,
            DownloadUrl = DownloadUrl
        };
    }
}
