using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

public class Logger
{
    private static readonly string LogDirectory = Environment.CurrentDirectory;
    private static readonly string LogFileName = "PDR.log";
    private static readonly string LogFilePath = Path.Combine(LogDirectory, LogFileName);
    private static readonly string ArchiveDirectory = Path.Combine(LogDirectory, "Archives");
    private static readonly TimeSpan MaxLogAge = TimeSpan.FromDays(7);

    static Logger()
    {
        // Create the archive directory if it does not exist
        if (!Directory.Exists(ArchiveDirectory))
        {
            Directory.CreateDirectory(ArchiveDirectory);
        }
    }

    public static void LogToFile(string message)
    {
        try
        {
            // Ensure log directory exists
            Directory.CreateDirectory(LogDirectory);

            // Write to the current log file
            using (StreamWriter sw = new StreamWriter(LogFilePath, true))
            {
                sw.WriteLine($"{DateTime.Now}: {message}");
            }

            // Perform log rotation
            PerformLogRotation();
        }
        catch (Exception ex)
        {
            // Handle logging errors
            Console.WriteLine("Error writing to log file: " + ex.Message);
        }
    }

    private static void PerformLogRotation()
    {
        try
        {
            // Check if the log file is too large or needs rotation
            FileInfo logFileInfo = new FileInfo(LogFilePath);
            if (logFileInfo.Length > 10 * 1024 * 1024) // Rotate if file size exceeds 10 MB
            {
                RotateLogFile();
            }

            // Delete old archives
            DeleteOldArchives();
        }
        catch (Exception ex)
        {
            // Handle errors during log rotation
            LogToFile("Error during log rotation: " + ex.Message);
        }
    }

    private static void RotateLogFile()
    {
        try
        {
            string archiveFileName = $"PDR_{DateTime.Now:yyyyMMdd_HHmmss}.log.zip";
            string archiveFilePath = Path.Combine(ArchiveDirectory, archiveFileName);

            // Compress the current log file into a ZIP archive
            using (FileStream zipToOpen = new FileStream(archiveFilePath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                ZipArchiveEntry readmeEntry = archive.CreateEntry(LogFileName);

                using (Stream entryStream = readmeEntry.Open())
                using (FileStream logFileStream = new FileStream(LogFilePath, FileMode.Open, FileAccess.Read))
                {
                    logFileStream.CopyTo(entryStream);
                }
            }

            // Clear the current log file
            File.WriteAllText(LogFilePath, string.Empty);
        }
        catch (Exception ex)
        {
            // Handle errors during file rotation
            LogToFile("Error rotating log file: " + ex.Message);
        }
    }

    private static void DeleteOldArchives()
    {
        try
        {
            DirectoryInfo dirInfo = new DirectoryInfo(ArchiveDirectory);

            // Get all ZIP files in the archive directory
            FileInfo[] zipFiles = dirInfo.GetFiles("*.zip");
            foreach (FileInfo file in zipFiles)
            {
                // Delete files older than the specified max age
                if (DateTime.Now - file.LastWriteTime > MaxLogAge)
                {
                    file.Delete();
                }
            }
        }
        catch (Exception ex)
        {
            // Handle errors during old archive deletion
            LogToFile("Error deleting old archives: " + ex.Message);
        }
    }
}
