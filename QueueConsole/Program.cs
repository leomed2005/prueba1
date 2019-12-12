using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.Storage.Blob;

namespace QueueConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .Build();

        CloudStorageAccount myClient =  CloudStorageAccount.Parse (config["connectionstring"]);
        CloudQueueClient queueClient = myClient.CreateCloudQueueClient ();

        CloudQueue queue = queueClient.GetQueueReference("filaprocesos");
        CloudQueueMessage peekMessage = queue.PeekMessage();

        CloudBlobClient blobClient = myClient.CreateCloudBlobClient();
        CloudBlobContainer container = blobClient.GetContainerReference("contenedor registros");
        container.CreateIfNotExists();

        foreach (CloudQueueMessage item in queue.GetMessages(20, TimeSpan.FromSeconds(100)))
        {    
            string filePath = string.Format (@"Log{0}.txt" , item.Id);
            TextWriter tempFile = File.CreateText (filePath);
            var message = queue.GetMessage().AsString;
            tempFile.WriteLine(message);
            Console.WriteLine("archivo creado");
            tempFile.Close(); 
          
            using(var fileStream = System.IO.File.OpenRead(filePath))
            {
                CloudBlockBlob myBlob = container.GetBlockBlobReference(string.Format("Log{0}.txt", item.Id));
                myBlob.UploadFromStream(fileStream);
                Console.WriteLine("Blob creado");
            }
            
            queue.DeleteMessage(item);
        }

        Console.ReadLine();
    }
 }        
 }
