using FileService.Model;
using Microsoft.EntityFrameworkCore;

namespace FileService.Database
{
    public class StorageContext : DbContext
    {
        public StorageContext(DbContextOptions<StorageContext> options) : base(options) { }

        public DbSet<FileMetadata> FileMetadatas { get; set; }
    }
}
