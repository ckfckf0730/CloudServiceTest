using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

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
    }
}
