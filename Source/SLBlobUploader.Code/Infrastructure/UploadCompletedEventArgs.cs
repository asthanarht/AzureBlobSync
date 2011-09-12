
namespace SLBlobUploader.Code.Infrastructure
{
    using System;

    /// <summary>
    /// Event arguments for upload completed event.
    /// </summary>
    public class UploadCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the uploaded file.
        /// </summary>
        /// <value>The uploaded file.</value>
        public UserFile UploadedFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the reason.
        /// </summary>
        /// <value>The reason.</value>
        public Constants.UploadCompleteReason Reason
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage
        {
            get;
            set;
        }
    }
}
