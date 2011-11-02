
namespace BlobUploader.Web
{
    using System;
    using System.Configuration;
    using System.Web.UI;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;

    /// <summary>
    /// Code behind file for test page
    /// </summary>
    public partial class AzureFileUploadTest : System.Web.UI.Page
    {
        /// <summary>
        /// Container where to upload files
        /// </summary>
        private const string ContainerName = "uploads";

        /// <summary>
        /// Configuration section key containing connection string.
        /// </summary>
        private const string ConfigurationSectionKey = "StorageAccountConfiguration";

        /// <summary>
        /// URL of container to upload file
        /// </summary>
        private string containerUrl;

        /// <summary>
        /// Time period for which shared access signature is valid.
        /// </summary>
        private int timeOutMinutes = 10;

        /// <summary>
        /// Gets the SAS URL.
        /// </summary>
        /// <returns>SAS URL of target container</returns>
        public string GetSASUrl()
        {
            return this.containerUrl;
        }

        /// <summary>
        /// Get the number of seconds for which the signature is valid.
        /// </summary>
        /// <returns>Time out interval in seconds</returns>
        public string GetTimeOutSeconds()
        {
            return TimeSpan.FromMinutes(this.timeOutMinutes).TotalSeconds.ToString();
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                var account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings[ConfigurationSectionKey]);
                var blobs = account.CreateCloudBlobClient();
                this.CreateSilverlightPolicy(blobs);
                var container = blobs.GetContainerReference(ContainerName);
                container.CreateIfNotExist();
                var sas = container.GetSharedAccessSignature(new SharedAccessPolicy()
                {
                    Permissions = SharedAccessPermissions.Write,
                    SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(this.timeOutMinutes)
                });

                this.containerUrl = (new UriBuilder(container.Uri)
                {
                    Query = sas.TrimStart('?')
                }).Uri.AbsoluteUri;
            }
        }

        /// <summary>
        /// Creates the silverlight policy file in storage account.
        /// </summary>
        /// <param name="blobs">The blob client.</param>
        private void CreateSilverlightPolicy(CloudBlobClient blobs)
        {
            blobs.GetContainerReference("$root").CreateIfNotExist();
            blobs.GetContainerReference("$root").SetPermissions(
                new BlobContainerPermissions()
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });
            var blob = blobs.GetBlobReference("clientaccesspolicy.xml");
            blob.Properties.ContentType = "text/xml";
            blob.UploadText(@"<?xml version=""1.0"" encoding=""utf-8""?>
                    <access-policy>
                      <cross-domain-access>
                        <policy>
                          <allow-from http-methods=""*"" http-request-headers=""*"">
                            <domain uri=""*"" />
                            <domain uri=""http://*"" />
                          </allow-from>
                          <grant-to>
                            <resource path=""/"" include-subpaths=""true"" />
                          </grant-to>
                        </policy>
                      </cross-domain-access>
                    </access-policy>");
        }
    }
}