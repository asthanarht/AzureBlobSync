
namespace SLBlobUploader.Code.Infrastructure
{
    using System.Net;

    /// <summary>
    /// State to be propagated between web requests
    /// </summary>
    internal class AsyncWebRequestState
    {
        /// <summary>
        /// Gets or sets the state of the web request.
        /// </summary>
        /// <value>The state of the web request.</value>
        internal WebRequest WebRequestState
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the request payload.
        /// </summary>
        /// <value>The request payload.</value>
        internal DataPacket RequestPayload
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the file to upload.
        /// </summary>
        /// <value>The file to upload.</value>
        internal UserFile FileToUpload
        {
            get;
            set;
        }
    }
}
