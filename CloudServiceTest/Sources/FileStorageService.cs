using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Configuration;

namespace CloudServiceTest
{
    public class FileStorageService
    {
        private readonly IConfiguration _configuration;

        public FileStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task UploadFileAsync(string shareName, string filePath, Stream fileStream)
        {
            // 读取连接字符串
            string connectionString = _configuration["AzureStorage:ConnectionString"];

            // 创建共享文件客户端
            ShareClient shareClient = new ShareClient(connectionString, shareName);

            // 创建文件共享（如果不存在）
            await shareClient.CreateIfNotExistsAsync();

            // 获取根目录的引用
            ShareDirectoryClient rootDirectory = shareClient.GetRootDirectoryClient();

            // 获取或创建文件的引用
            ShareFileClient fileClient = rootDirectory.GetFileClient(filePath);

            // 上传文件
            await fileClient.CreateAsync(fileStream.Length);
            await fileClient.UploadRangeAsync(
                new Azure.HttpRange(0, fileStream.Length),
                fileStream
            );
        }

    }
}
