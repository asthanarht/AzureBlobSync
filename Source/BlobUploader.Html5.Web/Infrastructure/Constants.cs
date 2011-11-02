﻿
namespace BlobUploader.Html5.Web.Infrastructure
{
    /// <summary>
    /// Constants used by the application.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Configuration section key containing connection string.
        /// </summary>
        internal const string ConfigurationSectionKey = "StorageAccountConfiguration";

        /// <summary>
        /// Container where to upload files
        /// </summary>
        internal const string ContainerName = "uploads";

        /// <summary>
        /// Number of bytes in a Kb.
        /// </summary>
        internal const int BytesPerKb = 1024;

        /// <summary>
        /// Name of session element where attributes of file to be uploaded are saved.
        /// </summary>
        internal const string FileAttributesSession = "FileClientAttributes";
    }
}