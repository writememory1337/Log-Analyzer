using LogAnalyzer.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace LogAnalyzer.Infrastructure.Data
{
    public class LogAnalyzerDbContext : DbContext
    {
        public LogAnalyzerDbContext(DbContextOptions<LogAnalyzerDbContext> options) : base(options) { }

        public DbSet<LogEntry> LogEntries { get; set; }
    }
}