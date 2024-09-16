using System.Collections.Generic;
using System.Threading.Tasks;
using LogAnalyzer.Core.Interfaces;
using LogAnalyzer.Core.Models;
using LogAnalyzer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LogAnalyzer.Infrastructure.Repositories
{
    public class LogRepository : ILogRepository
    {
        private readonly LogAnalyzerDbContext _context;

        public LogRepository(LogAnalyzerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LogEntry>> GetAllLogsAsync()
        {
            return await _context.LogEntries.ToListAsync();
        }

        public async Task AddLogAsync(LogEntry logEntry)
        {
            await _context.LogEntries.AddAsync(logEntry);
            await _context.SaveChangesAsync();
        }

        public async Task AddLogsAsync(IEnumerable<LogEntry> logEntries)
        {
            await _context.LogEntries.AddRangeAsync(logEntries);
            await _context.SaveChangesAsync();
        }
    }
}