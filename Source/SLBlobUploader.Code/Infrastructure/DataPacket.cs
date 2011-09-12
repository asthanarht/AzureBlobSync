
namespace SLBlobUploader.Code.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Data packet to be transmitted.
    /// </summary>
    public class DataPacket
    {
        /// <summary>
        /// Size of each packet to transmit.
        /// </summary>
        private int packetSize = Constants.ChunkSize;

        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        /// <value>The payload.</value>
        public byte[] Payload
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the is transported.
        /// </summary>
        /// <value>The is transported.</value>
        public bool? IsTransported
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the serial number.
        /// </summary>
        /// <value>The serial number.</value>
        public string SerialNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the retry count.
        /// </summary>
        /// <value>The retry count.</value>
        public int RetryCount
        {
            get;
            set;
        }

        /// <summary>
        /// Transforms the stream to packets.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <returns>List of packets split out from stream</returns>
        public List<DataPacket> TransformStreamToPackets(Stream sourceStream)
        {
            int bytesToRead = 0;
            int serialNumber = 1;
            byte[] buffer = new byte[this.packetSize];
            var dataBlocks = new List<DataPacket>();
            while ((bytesToRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                var payloadArray = new byte[bytesToRead];
                Array.Copy(buffer, payloadArray, bytesToRead);
                dataBlocks.Add(new DataPacket()
                {
                    IsTransported = false,
                    Payload = payloadArray,
                    RetryCount = 0,
                    SerialNumber = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0:D4}", serialNumber++)))
                });
            }

            return dataBlocks;
        }
    }
}
