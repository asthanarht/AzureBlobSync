
namespace BlobUploader.Html5.Web.Controllers
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using System.Collections.Generic;
    using BlobUploader.Html5.Web.Models;
    using System.Web;

    public class HomeController : Controller
    {
        /// <summary>
        /// Configuration section key containing connection string.
        /// </summary>
        private const string ConfigurationSectionKey = "StorageAccountConfiguration";

        /// <summary>
        /// Container where to upload files
        /// </summary>
        private const string ContainerName = "uploads";
        private int BytesPerKb = 1024;

        [HttpGet]
        public ActionResult Index()
        {
            //Session.Clear();
            return View(new FileUploadModel()
            {
                IsUploadCompleted = false,
                UploadStatusMessage = string.Empty
            });
        }

        [HttpPost]
        public ActionResult PrepareMetaData(int blocksCount, string fileName, long fileSize)
        {
            var container = (CloudStorageAccount.Parse(ConfigurationManager.AppSettings[ConfigurationSectionKey])).CreateCloudBlobClient().GetContainerReference(ContainerName);
            container.CreateIfNotExist();
            Session.Add("FileClientAttributes", new FileUploadModel()
            {
                BlockCount = blocksCount,
                FileName = fileName,
                FileSize = fileSize,
                BlockBlob = container.GetBlockBlobReference(fileName),
                StartTime = DateTime.Now,
                BlockCounter = 0,
                IsUploadCompleted = false,
                UploadStatusMessage = string.Empty
            });
            return Json(true);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UploadBlock()
        {
            byte[] chunk = new byte[Request.InputStream.Length];
            Request.InputStream.Read(chunk, 0, Convert.ToInt32(Request.InputStream.Length));
            if (Session["FileClientAttributes"] != null)
            {
                var model = (FileUploadModel)Session["FileClientAttributes"];
                using (var chunkStream = new MemoryStream(chunk))
                {
                    var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0:D4}", Convert.ToInt32(Request.Headers["X-File-Name"]))));
                    try
                    {
                        model.BlockBlob.PutBlock(blockId, chunkStream, null, new BlobRequestOptions() { RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(10)) });
                    }
                    catch (StorageException e)
                    {
                        Session.Clear();
                        model.IsUploadCompleted = true;
                        model.UploadStatusMessage = string.Format("Failed to upload file because " + e.Message);
                        return View("Index", model);
                    }

                    ++model.BlockCounter;
                }

                if (model.BlockCounter == model.BlockCount)
                {
                    var blockList = Enumerable.Range(1, (int)model.BlockCount).ToList<int>().ConvertAll<string>(rangeElement => Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0:D4}", rangeElement))));
                    model.BlockBlob.PutBlockList(blockList);
                    var duration = DateTime.Now - model.StartTime;
                    model.IsUploadCompleted = true;
                    long fileSizeInKb = model.FileSize / BytesPerKb;
                    string fileSizeMessage = fileSizeInKb > BytesPerKb ? string.Concat((fileSizeInKb / BytesPerKb).ToString(), " MB") : string.Concat(fileSizeInKb.ToString(), " KB");
                    model.UploadStatusMessage = string.Format("File of size {0} has been uploaded in {1} seconds", fileSizeMessage, duration.TotalSeconds);
                    Session.Clear();
                    return View(model);
                }
            }

            return Json(true);
        }
    }
}
