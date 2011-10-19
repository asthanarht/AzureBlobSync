
namespace BlobUploader.Html5.Web.Models
{
    using System;
    using Microsoft.WindowsAzure.StorageClient;

    public class FileUploadModel
    {
        public long BlockCount { get; set; }
        public long FileSize { get; set; }
        public string FileName { get; set; }
        public CloudBlockBlob BlockBlob { get; set; }
        public DateTime StartTime { get; set; }
        public long BlockCounter { get; set; }
        public string UploadStatusMessage { get; set; }
        public bool IsUploadCompleted { get; set; }
    }
}