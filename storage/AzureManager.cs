using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace MKP.Azure {
    public class AzureManager {
        private CloudStorageAccount StorageAccount;
        private CloudBlobClient BlobClient;

        public AzureManager(string ConnectionString) {
            StorageAccount = CloudStorageAccount.Parse(ConnectionString);
            BlobClient = StorageAccount.CreateCloudBlobClient();
        }

        public bool CreateContainer(string ContainerName, bool IsPublic = true) {
            CloudBlobContainer Container = BlobClient.GetContainerReference(ContainerName);
            Container.CreateIfNotExists();

            if (IsPublic)
            {
                Container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            }

            return true;

        }

        public bool RemoveContainer(string ContainerName) {
            CloudBlobContainer Container = BlobClient.GetContainerReference(ContainerName);
            return Container.DeleteIfExists();
        }

        public List<CloudBlobContainer> GetContainers() {
            var Containers = new List<CloudBlobContainer>();

            foreach(var c in BlobClient.ListContainers()){
                Containers.Add(c);
            }

            return Containers;
        }

        public void ClearStorage() {
            var Containers = GetContainers();

            foreach (var c in Containers) {
                c.DeleteIfExists();
            }
        }

        public void AddBlob(string ContainerName, string BlobName, string MimeType, string File) {
            CloudBlobContainer Container = BlobClient.GetContainerReference(ContainerName);
            CloudBlockBlob BlockBlob = Container.GetBlockBlobReference(BlobName);

            using (var fs = System.IO.File.OpenRead(File)) {
                BlockBlob.UploadFromStream(fs);
                BlockBlob.Properties.ContentType = MimeType;
                BlockBlob.SetProperties();
            }
        }

        public void AddBlob(string ContainerName, string BlobName, string MimeType, byte[] bytes)
        {
            CloudBlobContainer Container = BlobClient.GetContainerReference(ContainerName);
            CloudBlockBlob BlockBlob = Container.GetBlockBlobReference(BlobName);

            using (var ms = new System.IO.MemoryStream(bytes)) {
                BlockBlob.UploadFromStream(ms);
                BlockBlob.Properties.ContentType = MimeType;
                BlockBlob.SetProperties();
            }
        }

        public void AddBlob(string ContainerName, string BlobName, string MimeType, Uri File) {
            CloudBlobContainer Container = BlobClient.GetContainerReference(ContainerName);
            CloudBlockBlob BlockBlob = Container.GetBlockBlobReference(BlobName);

            var req = HttpWebRequest.Create(File.AbsoluteUri);

            using (var str = req.GetResponse().GetResponseStream()) { 
                BlockBlob.UploadFromStream(str);
                BlockBlob.Properties.ContentType = MimeType;
                BlockBlob.SetProperties();
            }

        }

        public List<CloudBlockBlob> GetBlobs(string ContainerName) {
            var Blobs = new List<CloudBlockBlob>();
            var Container = BlobClient.GetContainerReference(ContainerName);

            foreach (var item in Container.ListBlobs(null, false)) {
                if (item.GetType() == typeof(CloudBlockBlob)) {
                    var b = (CloudBlockBlob) item;
                    Blobs.Add(b);
                }
            }

            return Blobs;
        }
    }
}
