using CloudServiceTest.Data;
using CloudServiceTest.Models;
using CloudServiceTest.Models.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Identity.Client.Extensions.Msal;
using static System.Formats.Asn1.AsnWriter;

namespace CloudServiceTest
{
    public class DatabaseService
    {
        private readonly ApplicationDbContext _dbContext;

        public ApplicationDbContext Context { get { return _dbContext; } }

		public DatabaseService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<IDbContextTransaction> GetTransactionAsync()
        {
            return _dbContext.Database.BeginTransactionAsync();
        }

        public async Task<int> SaveFileRecordAsync(FileRecord record)
        {
            await _dbContext.FileRecords.AddAsync(record);
            return await _dbContext.SaveChangesAsync();
        }

        public List<FileRecord> LoadFileRecord(string userName)
        {
            return _dbContext.FileRecords.Where(fr => fr.UploadedBy == userName).ToList();
        }

        public async Task<FileRecord?> GetFileRecordAsync(Guid guid)
        {
            return await _dbContext.FileRecords.FirstOrDefaultAsync(fr => fr.Id == guid);
        }

        public async Task<int> DeleteFileRecordAsync(Guid guid)
        {
            return await _dbContext.FileRecords.Where(fr => fr.Id == guid).ExecuteDeleteAsync();
        }
    }
}
