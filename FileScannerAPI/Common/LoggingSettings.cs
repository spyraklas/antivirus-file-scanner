namespace FileScannerAPI.Common
{
    public class LoggingSettings
    {
        public LogLevel LogLevel { get; set; }

        public string LogPath { get; set; }
    }

    public class LogLevel
    {
        public Microsoft.Extensions.Logging.LogLevel Default { get; set; }

        public Microsoft.Extensions.Logging.LogLevel System { get; set; }

        public Microsoft.Extensions.Logging.LogLevel Microsoft { get; set; }
    }
}
