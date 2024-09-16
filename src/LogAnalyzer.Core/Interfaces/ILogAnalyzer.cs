using System;
using System.Collections.Generic;
using LogAnalyzer.Core.Models;

namespace LogAnalyzer.Core.Interfaces
{
    public interface ILogAnalyzer
    {
        Dictionary<string, int> CountLogLevels(IEnumerable<LogEntry> logs);
        IEnumerable<LogEntry> FindErrorLogs(IEnumerable<LogEntry> logs);
        DateTime? FindFirstOccurrence(IEnumerable<LogEntry> logs, string searchTerm);
        IEnumerable<KeyValuePair<string, int>> GetTopErrors(IEnumerable<LogEntry> logs, int top = 5);
        double CalculateErrorRate(IEnumerable<LogEntry> logs);
    }
}