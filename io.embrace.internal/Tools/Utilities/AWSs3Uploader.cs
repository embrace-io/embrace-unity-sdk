using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Embrace.Tools
{
    /// <summary>
    /// Upload SDK to S3 bucket
    /// </summary>
    public class AWSs3Uploader
    {
        // need to use github secrets or something and remove hard coded keys
        private const string awsBucketName = "embrace-production-downloads";
        private readonly string awsAccessKey;
        private readonly string awsSecretKey;
        private string awsURLBaseVirtual = "";

        public AWSs3Uploader(string awsAccessKey, string awsSecretKey)
        {
            awsURLBaseVirtual = "https://downloads.embrace.io/";
            this.awsAccessKey = awsAccessKey;
            this.awsSecretKey = awsSecretKey;
        }

        public void PostObject(string FileName, string FilePath)
        {
            // Setup current date string used by AWS S3 Header
            string currentAWS3Date = System.DateTime.UtcNow.ToString(
                "ddd, dd MMM yyyy HH:mm:ss ") +
                "GMT";

            string canonicalString =
                "PUT\n\n\n\nx-amz-date:" +
                currentAWS3Date + "\n/" +
                awsBucketName + "/" + FileName;

            // Encode our secret
            UTF8Encoding encode = new UTF8Encoding();
            HMACSHA1 signature = new HMACSHA1();
            signature.Key = encode.GetBytes(awsSecretKey);
            byte[] bytes = encode.GetBytes(canonicalString);
            byte[] moreBytes = signature.ComputeHash(bytes);
            string encodedCanonical = Convert.ToBase64String(moreBytes);

            // Create an AWS3 header:
            string aws3Header = "AWS " +
                awsAccessKey + ":" +
                encodedCanonical;
            
            // Create Webrequest and point it at our newly created URL
            string URL3 = awsURLBaseVirtual + FileName;
            WebRequest requestS3 =
               (HttpWebRequest)WebRequest.Create(URL3);
            requestS3.Headers.Add("Authorization", aws3Header);
            requestS3.Headers.Add("x-amz-date", currentAWS3Date);

            // Read byte data from our local file
            byte[] fileRawBytes = File.ReadAllBytes(FilePath);
            requestS3.ContentLength = fileRawBytes.Length;

            // Set RESTful method on the request
            requestS3.Method = "PUT";

            // Upload the file to AWS via a Stream
            Stream S3Stream = requestS3.GetRequestStream();
            S3Stream.Write(fileRawBytes, 0, fileRawBytes.Length);
            Debug.Log("Sent bytes: " +
                requestS3.ContentLength +
                ", for file: " +
                FileName);
            S3Stream.Close();
        }
    }
}
