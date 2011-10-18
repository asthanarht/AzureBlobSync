
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

        [HttpGet]
        public ActionResult Index()
        {
            Session.Clear();
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings[ConfigurationSectionKey]);
            var container = storageAccount.CreateCloudBlobClient().GetContainerReference(ContainerName);
            container.CreateIfNotExist();
            Session["Container"] = container;
            return View();
        }

        [HttpPost]
        public ActionResult PrepareMetaData(int blocksCount, string fileName, long fileSize)
        {
            Session["BlockCount"] = blocksCount;
            Session["FileName"] = fileName;
            Session["BlockBlob"] = ((CloudBlobContainer)Session["Container"]).GetBlockBlobReference(fileName);
            Session["StartTime"] = DateTime.Now;
            Session["BlockCounter"] = 0;
            return Json(true);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UploadBlock()
        {
            byte[] chunk = new byte[Request.InputStream.Length];
            Request.InputStream.Read(chunk, 0, Convert.ToInt32(Request.InputStream.Length));
            if (Session["BlockBlob"] != null)
            {
                using (var chunkStream = new MemoryStream(chunk))
                {
                    var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0:D4}", Convert.ToInt32(Request.Headers["X-File-Name"]))));
                    try
                    {
                        ((CloudBlockBlob)Session["BlockBlob"]).PutBlock(blockId, chunkStream, null, new BlobRequestOptions() { RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(10)) });
                    }
                    catch (StorageException e)
                    {
                        FileUploadModel model = new FileUploadModel()
                        {
                            IsUploadCompleted = true,
                            UploadStatusMessage = string.Format("Failed to upload file because " + e.Message)
                        };
                        Session.Clear();
                        return View("Index", model);
                    }

                    Session["BlockCounter"] = (int)Session["BlockCounter"] + 1;
                }

                if (((int)Session["BlockCounter"]) == ((int)Session["BlockCount"]))
                {
                    var blockList = Enumerable.Range(1, ((int)Session["BlockCount"])).ToList<int>().ConvertAll<string>(rangeElement => Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0:D4}", rangeElement))));
                    ((CloudBlockBlob)Session["BlockBlob"]).PutBlockList(blockList);
                    var duration = DateTime.Now - ((DateTime)Session["StartTime"]);
                    FileUploadModel model = new FileUploadModel()
                    {
                        IsUploadCompleted = true,
                        UploadStatusMessage = string.Format("{0} has been uploaded in {1}", Session["FileName"].ToString(), duration.TotalSeconds)
                    };
                    Session.Clear();
                    return View("Index", model);
                }
            }

            return Json(true);
        }
    }
}
