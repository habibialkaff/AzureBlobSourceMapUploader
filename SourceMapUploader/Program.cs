using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace SourceMapUploader
{
    public class Program
    {
        public static async Task Main()
        {
            Config config;
            using (var configFileStream = File.OpenRead("config.json"))
            {
                config = await JsonSerializer.DeserializeAsync<Config>(configFileStream);
            }

            var blobServiceClient = new BlobServiceClient(config.ConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(config.BlobContainerName);

            var files = Directory.GetFiles(config.SourceFolder, "*.js.map");

            var i = files.Length;

            Console.WriteLine($"Total files: {files.Length}");

            var tasks = files.Select(async x =>
            {
                var fileName = Path.GetFileName(x);

                Console.WriteLine($"Start uploading {fileName}");

                var blobClient = blobContainerClient.GetBlobClient(fileName);
                using (var uploadFileStream = File.OpenRead(x))
                {
                    await blobClient.UploadAsync(uploadFileStream, true);
                }

                Console.WriteLine($"{fileName} is uploaded");
                Console.WriteLine($"Remaining files: {--i}");
            });

            await Task.WhenAll(tasks);
        }
    }

    public class Config
    {
        public string SourceFolder { get; set; }

        public string ConnectionString { get; set; }

        public string BlobContainerName { get; set; }
    }
}
