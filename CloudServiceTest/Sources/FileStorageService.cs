using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Sas;

namespace CloudServiceTest
{
    public class FileStorageService
    {
        private readonly IConfiguration _configuration;
        private string _connectionString;

		public FileStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration["AzureStorage:ConnectionString"];
        }

        public async Task<Response<ShareFileUploadInfo>> UploadFileAsync(string shareName, string filePath, Stream fileStream)
        {
            // 创建共享文件客户端
            ShareClient shareClient = new ShareClient(_connectionString, shareName);

            // 创建文件共享（如果不存在）
            await shareClient.CreateIfNotExistsAsync();

            // 获取根目录的引用
            ShareDirectoryClient rootDirectory = shareClient.GetRootDirectoryClient();

            // 获取或创建文件的引用
            ShareFileClient fileClient = rootDirectory.GetFileClient(filePath);

            // 上传文件
            await fileClient.CreateAsync(fileStream.Length);
            return await fileClient.UploadRangeAsync(
                new Azure.HttpRange(0, fileStream.Length),
                fileStream
            );
        }

		public async Task<Response<ShareFileUploadInfo>> UploadFileChunkAsync(string shareName, string fileName,int chunkId , byte[] buffer)
		{
			ShareClient shareClient = new ShareClient(_connectionString, shareName);

			await shareClient.CreateIfNotExistsAsync();

			ShareDirectoryClient rootDirectory = shareClient.GetRootDirectoryClient();

            if (chunkId > 0)
            {
				fileName = fileName +"_" + chunkId.ToString();

			}
			ShareFileClient fileClient = rootDirectory.GetFileClient(fileName);

            var fileStream = new MemoryStream(buffer);
			await fileClient.CreateAsync(fileStream.Length);
			return await fileClient.UploadRangeAsync(
				new Azure.HttpRange(0, fileStream.Length),
				fileStream
			);
		}

		public async Task<Stream?> DownloadFileAsync(string shareName, string filePath)
        {
            ShareClient shareClient = new ShareClient(_connectionString, shareName);
            await shareClient.CreateIfNotExistsAsync();
            ShareDirectoryClient rootDirectory = shareClient.GetRootDirectoryClient();
            ShareFileClient fileClient = rootDirectory.GetFileClient(filePath);

            // 检查文件是否存在
            if (!await fileClient.ExistsAsync())
            {
                return null;
            }

            // 下载文件
            var response = await fileClient.DownloadAsync();

            // 获取文件流
            return response.Value.Content;
        }

        public async Task<bool> DeleteFileAsync(string shareName, string filePath)
        {
            ShareClient shareClient = new ShareClient(_connectionString, shareName);

            if(!await shareClient.ExistsAsync())
            {
                return false;
            }

            ShareDirectoryClient rootDirectory = shareClient.GetRootDirectoryClient();
            ShareFileClient fileClient = rootDirectory.GetFileClient(filePath);

            // 上传文件
            var response = await fileClient.DeleteAsync();
            if(response.IsError)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

		public async Task<bool> UploadFileAsBlobAsync(string containerName, string blobName, Stream fileStream)
		{
			var blobServiceClient = new BlobServiceClient(_connectionString);
			var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
			await containerClient.CreateIfNotExistsAsync();
    
            var blockBlobClient = containerClient.GetBlockBlobClient(blobName);

			long fileLength = fileStream.Length;

			const int blockSize = 4 * 1024 * 1024; // 4MB
			var blockList = new List<string>(); 

			byte[] buffer = new byte[blockSize];
			int bytesRead;
			int blockIdCounter = 0;

			while ((bytesRead = await fileStream.ReadAsync(buffer, 0, blockSize)) > 0)
			{
				// 生成一个基于块的唯一标识符（必须是 base64 编码）
				string blockId = Convert.ToBase64String(BitConverter.GetBytes(blockIdCounter));

				// 将块上传到 Blob 存储
				using (var stream = new MemoryStream(buffer, 0, bytesRead))
				{
					await blockBlobClient.StageBlockAsync(blockId, stream);
				}

				blockList.Add(blockId);
				blockIdCounter++;
			}

			// 合并所有块
			await blockBlobClient.CommitBlockListAsync(blockList);

			Console.WriteLine($"Blob File uploaded and merged successfully: {blobName}");
            return true;
		}

        public async Task<bool> DeleteFileAsBlobAsync(string containerName, string blobName)
        {
            try
            {
				var blobServiceClient = new BlobServiceClient(_connectionString);
				var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
				await containerClient.CreateIfNotExistsAsync();

				var blockBlobClient = containerClient.GetBlockBlobClient(blobName);

				var result = await blockBlobClient.DeleteAsync();
				if(result.IsError)
                {
					Console.WriteLine(result.Content);
					return false;
				}
			}
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.Message);
                return false;
            }


			Console.WriteLine($"Blob File deleted successfully: {blobName}");
			return true;
		}

        public string? GetStreamingVideoURL(string containerName, string blobName)
        {
			var blobServiceClient = new BlobServiceClient(_connectionString);
			var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
			var blobClient = containerClient.GetBlobClient(blobName);

			var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));

			return sasUri?.ToString();
		}

	}
}
