using System.CommandLine;
using LogAnalyzer.Core.Interfaces;
using LogAnalyzer.Core.Models;
using LogAnalyzer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using System.Text.RegularExpressions;

namespace LogAnalyzer.Console
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var fileOption = new Option<string>(
                name: "--file",
                description: "The log file to analyze."
            );
            fileOption.IsRequired = true;

            var startDateOption = new Option<DateTime?>(
                name: "--start-date",
                description: "The start date for log analysis."
            );

            var endDateOption = new Option<DateTime?>(
                name: "--end-date",
                description: "The end date for log analysis."
            );

            var rootCommand = new RootCommand("Log Analyzer");
            rootCommand.AddOption(fileOption);
            rootCommand.AddOption(startDateOption);
            rootCommand.AddOption(endDateOption);

            rootCommand.SetHandler(async (file, startDate, endDate) =>
            {
                await RunAnalysis(file, startDate, endDate);
            }, fileOption, startDateOption, endDateOption);

            return await rootCommand.InvokeAsync(args);
        }

        static async Task RunAnalysis(string filePath, DateTime? startDate, DateTime? endDate)
        {
            if (!File.Exists(filePath))
            {
                AnsiConsole.Markup($"[red]The file {filePath} does not exist.[/]");
                return;
            }

            var host = CreateHostBuilder().Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<LogAnalyzerDbContext>();
                    context.Database.EnsureCreated();

                    var logAnalyzer = services.GetRequiredService<ILogAnalyzer>();
                    var logRepository = services.GetRequiredService<ILogRepository>();

                    await AnsiConsole.Progress()
                        .StartAsync(async ctx =>
                        {
                            var task = ctx.AddTask("[green]Importing logs[/]");
                            await ImportLogsFromFile(logRepository, filePath, task);
                        });

                    var logs = await logRepository.GetAllLogsAsync();
                    
                    if (startDate.HasValue)
                        logs = logs.Where(l => l.Timestamp >= startDate.Value);
                    if (endDate.HasValue)
                        logs = logs.Where(l => l.Timestamp <= endDate.Value);

                    logs = logs.ToList(); 

                    ShowInteractiveMenu(logAnalyzer, logs);
                }
                catch (Exception ex)
                {
                    AnsiConsole.Markup($"[red]An error occurred: {ex.Message}[/]");
                }
            }
        }

        static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<LogAnalyzerDbContext>(options =>
                        options.UseSqlite("Data Source=loganalyzer.db"));
                    services.AddScoped<ILogAnalyzer, LogAnalyzer.Core.Services.LogAnalyzer>();
                    services.AddScoped<ILogRepository, LogAnalyzer.Infrastructure.Repositories.LogRepository>();
                });

        static async Task ImportLogsFromFile(ILogRepository repository, string filePath, ProgressTask task)
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            task.MaxValue = lines.Length;

            foreach (var line in lines)
            {
                var logEntry = ParseLogLine(line);
                if (logEntry != null)
                {
                    await repository.AddLogAsync(logEntry);
                    task.Increment(1);
                }
            }
        }

        static LogEntry? ParseLogLine(string line)
        {
            var parts = line.Split('|', 3);
            if (parts.Length == 3)
            {
                return new LogEntry
                {
                    Timestamp = DateTime.Parse(parts[0].Trim()),
                    Level = parts[1].Trim(),
                    Message = parts[2].Trim()
                };
            }

            var match = Regex.Match(line, @"^(\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2})\s(\w+):\s(.+)$");
            if (match.Success)
            {
                return new LogEntry
                {
                    Timestamp = DateTime.Parse(match.Groups[1].Value),
                    Level = match.Groups[2].Value,
                    Message = match.Groups[3].Value
                };
            }

            return null;
        }

        static void ShowInteractiveMenu(ILogAnalyzer logAnalyzer, IEnumerable<LogEntry> logs)
        {
            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("What would you like to do?")
                        .PageSize(10)
                        .AddChoices(new[] {
                            "View Log Level Counts",
                            "View Top Errors",
                            "View Error Rate",
                            "Search Logs",
                            "Exit"
                        }));

                switch (choice)
                {
                    case "View Log Level Counts":
                        DisplayLogLevelCounts(logAnalyzer, logs);
                        break;
                    case "View Top Errors":
                        DisplayTopErrors(logAnalyzer, logs);
                        break;
                    case "View Error Rate":
                        DisplayErrorRate(logAnalyzer, logs);
                        break;
                    case "Search Logs":
                        SearchLogs(logs);
                        break;
                    case "Exit":
                        return;
                }

                AnsiConsole.WriteLine();
                AnsiConsole.Markup("[blue]Press any key to continue...[/]");
                System.Console.ReadKey(true);
                AnsiConsole.Clear();
            }
        }

        static void DisplayLogLevelCounts(ILogAnalyzer logAnalyzer, IEnumerable<LogEntry> logs)
        {
            var logLevelCounts = logAnalyzer.CountLogLevels(logs);

            var table = new Table();
            table.AddColumn("Log Level");
            table.AddColumn("Count");

            foreach (var kvp in logLevelCounts)
            {
                table.AddRow(kvp.Key, kvp.Value.ToString());
            }

            AnsiConsole.Write(new Rule("[yellow]Log Level Counts[/]"));
            AnsiConsole.Write(table);
        }

        static void DisplayTopErrors(ILogAnalyzer logAnalyzer, IEnumerable<LogEntry> logs)
        {
            var topErrors = logAnalyzer.GetTopErrors(logs);

            var table = new Table();
            table.AddColumn("Error Message");
            table.AddColumn("Occurrences");

            foreach (var error in topErrors)
            {
                table.AddRow(error.Key, error.Value.ToString());
            }

            AnsiConsole.Write(new Rule("[yellow]Top 5 Errors[/]"));
            AnsiConsole.Write(table);
        }

        static void DisplayErrorRate(ILogAnalyzer logAnalyzer, IEnumerable<LogEntry> logs)
        {
            var errorRate = logAnalyzer.CalculateErrorRate(logs);

            AnsiConsole.Write(new Rule("[yellow]Error Rate[/]"));
            AnsiConsole.Markup($"[green]{errorRate:P2}[/]\n");
        }

        static void SearchLogs(IEnumerable<LogEntry> logs)
        {
            var searchTerm = AnsiConsole.Ask<string>("Enter search term:");
            var results = logs.Where(l => l.Message.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                              .ToList();

            var table = new Table();
            table.AddColumn("Timestamp");
            table.AddColumn("Level");
            table.AddColumn("Message");

            foreach (var log in results)
            {
                table.AddRow(log.Timestamp.ToString(), log.Level, log.Message);
            }

            AnsiConsole.Write(new Rule($"[yellow]Search Results for '{searchTerm}'[/]"));
            AnsiConsole.Write(table);
        }
    }
}