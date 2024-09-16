# Log Analyzer

Log Analyzer is a powerful, flexible command-line tool for parsing and analyzing log files. It supports multiple log formats and provides a user-friendly interface for exploring log data.

## Features

- **Multiple Log Formats**: Log Analyzer can parse logs in different formats:
  - `timestamp | level | message`
  - `timestamp level: message`
- **Colorful Console Output**: Utilizes Spectre.Console for a visually appealing and easy-to-read interface.
- **Interactive Menu**: Navigate through different analysis options with an interactive console menu.
- **Flexible Date Filtering**: Analyze logs within a specific date range.
- **Database Storage**: Logs are stored in a SQLite database for efficient querying and analysis.
- **Various Analysis Options**:
  - View log level counts
  - Display top errors
  - Calculate error rates
  - Search logs with custom terms

## Setup

1. Ensure you have .NET 6.0 or later installed on your system.
2. Clone the repository:
   ```
   git clone https://github.com/writememory1337/Log-Analyzer.git
   ```
3. Navigate to the project directory:
   ```
   cd Log-Analyzer
   ```
4. Restore the required packages:
   ```
   dotnet restore
   ```

## Usage

Run the Log Analyzer with the following command:

```
dotnet run --project src/LogAnalyzer.Console -- --file <path_to_log_file> [--start-date <start_date>] [--end-date <end_date>]
```

Options:
- `--file`: (Required) Path to the log file to analyze.
- `--start-date`: (Optional) Start date for log analysis (format: YYYY-MM-DD).
- `--end-date`: (Optional) End date for log analysis (format: YYYY-MM-DD).

Example:
```
dotnet run --project src/LogAnalyzer.Console -- --file logs/application.log --start-date 2023-01-01 --end-date 2023-12-31
```

After running the command, you'll be presented with an interactive menu to choose various analysis options.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.