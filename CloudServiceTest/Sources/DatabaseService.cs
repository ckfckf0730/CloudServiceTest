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

        public DatabaseService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<IDbContextTransaction> GetTransactionAsync()
        {
            return _dbContext.Database.BeginTransactionAsync();
        }

        public int SaveFileRecord(FileRecord record)
        {
            _dbContext.FileRecords.Add(record);
            return _dbContext.SaveChanges();
            //var test = _dbContext.FileRecords.FirstOrDefault(fr => fr.Id == record.Id);
        }

        public async Task SaveFileRecordAsync(FileRecord record)
        {
            await _dbContext.FileRecords.AddAsync(record);
            await _dbContext.SaveChangesAsync();
        }

        public List<FileRecord> LoadFileRecord(string userName)
        {
            return _dbContext.FileRecords   .Where(fr => fr.UploadedBy == userName).ToList();
        }
    }
}
