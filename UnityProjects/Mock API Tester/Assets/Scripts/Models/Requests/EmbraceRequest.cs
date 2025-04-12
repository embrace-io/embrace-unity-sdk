using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Embrace.MockAPI.Models
{
    /// <summary>
    /// Abstract class for Embrace requests.
    /// </summary>
    public abstract class EmbraceRequest
    {
        /// <summary>
        /// Converts the request to an HttpContent object.
        /// </summary>
        /// <returns>Object as HttpContent</returns>
        public virtual HttpContent ToHttpContent()
        {
            string json = JsonConvert.SerializeObject(this);
            byte[] compressed = Compress(json);
            return new ByteArrayContent(compressed);
        }

        /// <summary>
        /// Uses a GzipStream to compress the string.
        /// </summary>
        /// <param name="contents">String to compress</param>
        /// <returns>String compressed to byte array</returns>
        protected byte[] Compress(string contents)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(contents);
            using var outputStream = new MemoryStream();
            using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                gzipStream.Write(inputBytes, 0, inputBytes.Length);
            }
            
            return outputStream.ToArray();
        }
    }
}