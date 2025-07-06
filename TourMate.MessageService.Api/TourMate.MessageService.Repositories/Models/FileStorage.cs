using System;
using System.Collections.Generic;

namespace TourMate.MessageService.Repositories.Models;

public partial class FileStorage
{
    public string Id { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string DownloadUrl { get; set; } = null!;

    public DateTime? UploadTime { get; set; }
}
