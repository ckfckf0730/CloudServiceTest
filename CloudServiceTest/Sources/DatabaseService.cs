using CloudServiceTest.Data;
using CloudServiceTest.Models;
using CloudServiceTest.Models.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public void SaveFileRecord(FileRecord record)
        {
            _dbContext.FileRecords.Add(record);
            _dbContext.SaveChanges();
            //var test = _dbContext.FileRecords.FirstOrDefault(fr => fr.Id == record.Id);
        }

        public async Task SaveFileRecordAsync(FileRecord record)
        {
            await _dbContext.FileRecords.AddAsync(record);
            await _dbContext.SaveChangesAsync();
        }
    }
}
