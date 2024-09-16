using System.Collections.Generic;
using System.Threading.Tasks;
using LogAnalyzer.Core.Models;

namespace LogAnalyzer.Core.Interfaces
{
    public interface ILogRepository
    {
        Task<IEnumerable<LogEntry>> GetAllLogsAsync();
        Task AddLogAsync(LogEntry logEntry);
        Task AddLogsAsync(IEnumerable<LogEntry> logEntries);
    }
}