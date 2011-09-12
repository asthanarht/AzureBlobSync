
namespace SLBlobUploader.Control
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using SLBlobUploader.Code.Infrastructure;

    /// <summary>
    /// Code file for BlobUpload control.
    /// </summary>
    public partial class MainPage : UserControl
    {
        /// <summary>
        /// Maximum file size in Kb allowed to be uploaded.
        /// </summary>
        private const long MaxFileSizeKb = Constants.MaxFileSizeKB;

        /// <summary>
        /// Text to appear on upload button.
        /// </summary>
        private const string UploadButtonText = "Upload";

        /// <summary>
        /// Text to appear on cancel button.
        /// </summary>
        private const string CancelButtonText = "Cancel";

        /// <summary>
        /// Number of bytes per Kb.
        /// </summary>
        private const long BytesPerKb = 1024;

        /// <summary>
        /// List of files to upload.
        /// </summary>
        private List<UserFile> files = null;

        /// <summary>
        /// Shared access signature URL of the container where to upload files.
        /// </summary>
        private string sasUrl;

        /// <summary>
        /// Timer to time operation.
        /// </summary>
        private DateTime operationStartTime;

        /// <summary>
        /// User file to upload.
        /// </summary>
        private UserFile userFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        /// <param name="sasUrl">The shared access signature URL.</param>
        public MainPage(string sasUrl)
        {
            this.sasUrl = sasUrl;
            this.InitializeComponent();
        }

        /// <summary>
        /// Called when Browse file button is clicked.
        /// </summary>
        /// <param name="sender">The sender of event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnBrowseFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            ////Keeping multi select off for v1.0
            fileDialog.Multiselect = false;
            if ((bool)fileDialog.ShowDialog())
            {
                if (this.files == null)
                {
                    this.files = new List<UserFile>();
                }

                this.txtFileName.Text = fileDialog.File.Name.Substring(
                    0, fileDialog.File.Name.Length > 30 ? 30 : fileDialog.File.Name.Length);
                this.userFile = new UserFile();
                this.userFile.FileName = fileDialog.File.Name;
                this.userFile.FileStream = fileDialog.File.OpenRead();
                this.userFile.UIDispatcher = this.Dispatcher;
                this.userFile.UploadContainerUrl = new Uri(this.sasUrl);
                this.userFile.UploadCompletedEvent += new EventHandler<UploadCompletedEventArgs>(this.OnUploadCompleted);
                if ((this.userFile.FileStream.Length / BytesPerKb) <= MaxFileSizeKb && this.userFile.FileStream.Length > 0)
                {
                    this.files.Add(this.userFile);
                }
                else
                {
                    this.lblMessage.Text = this.userFile.FileStream.Length > 0 ?
                        string.Format(CultureInfo.CurrentCulture, ApplicationResources.IllegalMaxFileSize, MaxFileSizeKb / BytesPerKb) : string.Format(ApplicationResources.IllegalMinFileSize, MaxFileSizeKb / 1024);
                    this.lblMessage.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Called when upload completes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SLBlobUploader.Code.Infrastructure.UploadCompletedEventArgs"/> instance containing the event data.</param>
        private void OnUploadCompleted(object sender, UploadCompletedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
                {
                    switch (e.Reason)
                    {
                        case Constants.UploadCompleteReason.UploadCommitted:
                            var duration = DateTime.Now - this.operationStartTime;
                            var fileSizeInKB = (float)userFile.FileStream.Length / BytesPerKb;
                            var fileSizeMessage = fileSizeInKB > BytesPerKb ? string.Concat(((float)fileSizeInKB / BytesPerKb).ToString(), " MB") : string.Concat(fileSizeInKB.ToString(), " KB");
                            this.lblMessage.Text = string.Format(
                                CultureInfo.CurrentCulture, ApplicationResources.SuccessfulUpload, fileSizeMessage, duration.TotalSeconds);
                            break;
                        case Constants.UploadCompleteReason.ErrorOccurred:
                            this.lblMessage.Text = string.Format(CultureInfo.CurrentCulture, ApplicationResources.UploadFailed, e.ErrorMessage);
                            break;
                        case Constants.UploadCompleteReason.UserCancelled:
                            this.lblMessage.Text = string.Format(CultureInfo.CurrentCulture, ApplicationResources.UploadCancelled);
                            break;
                        default:
                            this.lblMessage.Text = string.Format(CultureInfo.CurrentCulture, ApplicationResources.UnknownErrorOccured);
                            break;
                    }

                    this.btnUpload.Content = UploadButtonText;
                    this.prgUpload.IsIndeterminate = false;
                });
        }

        /// <summary>
        /// Called when upload file button is clicked.
        /// </summary>
        /// <param name="sender">The sender of event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnUploadFile(object sender, RoutedEventArgs e)
        {
            Button callerButton = sender as Button;
            if (callerButton.Content.ToString().Equals(UploadButtonText, StringComparison.OrdinalIgnoreCase))
            {
                callerButton.Content = CancelButtonText;
                this.prgUpload.IsIndeterminate = true;
                this.operationStartTime = DateTime.Now;
                this.lblMessage.Text = string.Empty;
                if (this.files != null)
                {
                    foreach (var file in this.files)
                    {
                        file.Upload(string.Empty);
                    }
                }
                else
                {
                    this.lblMessage.Text = ApplicationResources.NoFileSelected;
                    this.prgUpload.IsIndeterminate = false;
                    this.btnUpload.Content = UploadButtonText;
                }
            }
            else
            {
                this.userFile.CancelUpload();
            }

            this.files = null;
        }
    }
}