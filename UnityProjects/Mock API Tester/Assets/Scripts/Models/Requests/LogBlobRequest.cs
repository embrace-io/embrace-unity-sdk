using System.IO;
using System.IO.Compression;
using System.Net.Http;
using Newtonsoft.Json;

namespace Embrace.MockAPI.Models
{
    /// <summary>
    /// Log Blob Request model for the Mock API.
    /// </summary>
    public class LogBlobRequest : EmbraceRequest
    {
        [JsonProperty("application_exits")]
        public Message Payload { get; set; }
        
        /// <summary>
        /// Data model for the Log Blob Request.
        /// </summary>
        public class Message
        {
            [JsonProperty("description")]
            public string Description;
            
            [JsonProperty("status")]
            public string Status;
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="status">Status to send</param>
        public LogBlobRequest(string message, string status)
        {
            Payload = new Message
            {
                Description = message,
                Status = status
            };
        }

        /// <summary>
        /// Converts the payload to a byte array and compresses it using GZip.
        /// </summary>
        /// <returns>HttpContent</returns>
        public override HttpContent ToHttpContent()
        {
            string json = JsonConvert.SerializeObject(Payload);
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

            using var outputStream = new MemoryStream();
            using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                gzipStream.Write(jsonBytes, 0, jsonBytes.Length);
            }

            byte[] compressed = outputStream.ToArray();
            return new ByteArrayContent(compressed);
        }

    }
}