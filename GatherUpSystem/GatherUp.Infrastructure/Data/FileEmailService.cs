using System;
using System.IO;
using GatherUp.Core.Interfaces;

namespace GatherUp.Infrastructure.Data
{
    public class FileEmailService : IEmailService
    {
        private readonly string _logFilePath;

        public FileEmailService(string logFolder)
        {
            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);
            _logFilePath = Path.Combine(logFolder, "emails.log");
        }

        public void Send(string toEmail, string subject, string body)
        {
            string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] To: {toEmail} | Subject: {subject}\n{body}\n{new string('-', 60)}\n";
            File.AppendAllText(_logFilePath, entry);
        }
    }
}
