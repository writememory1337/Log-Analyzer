using System;
using System.Collections.Generic;
using System.Linq;
using LogAnalyzer.Core.Interfaces;
using LogAnalyzer.Core.Models;

namespace LogAnalyzer.Core.Services
{
    public class LogAnalyzer : ILogAnalyzer
    {
        public Dictionary<string, int> CountLogLevels(IEnumerable<LogEntry> logs)
        {
            return logs.GroupBy(l => l.Level)
                       .ToDictionary(g => g.Key, g => g.Count());
        }

        public IEnumerable<LogEntry> FindErrorLogs(IEnumerable<LogEntry> logs)
        {
            return logs.Where(l => l.Level.Equals("ERROR", StringComparison.OrdinalIgnoreCase));
        }

        public DateTime? FindFirstOccurrence(IEnumerable<LogEntry> logs, string searchTerm)
        {
            return logs.FirstOrDefault(l => l.Message.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))?.Timestamp;
        }

        public IEnumerable<KeyValuePair<string, int>> GetTopErrors(IEnumerable<LogEntry> logs, int top = 5)
        {
            return logs.Where(l => l.Level.Equals("ERROR", StringComparison.OrdinalIgnoreCase))
                       .GroupBy(l => l.Message)
                       .OrderByDescending(g => g.Count())
                       .Take(top)
                       .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()));
        }

        public double CalculateErrorRate(IEnumerable<LogEntry> logs)
        {
            int totalLogs = logs.Count();
            int errorLogs = FindErrorLogs(logs).Count();
            return totalLogs > 0 ? (double)errorLogs / totalLogs : 0;
        }
    }
}