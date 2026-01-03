using System.IO;
using System.Linq;
using TNL_DPS_Meter.Core;
using TNL_DPS_Meter.Services.Interfaces;

namespace TNL_DPS_Meter.Services
{
    /// <summary>
    /// Service for monitoring and discovering combat log files
    /// </summary>
    public class FileMonitorService : IFileMonitorService
    {
        public string? GetLatestCombatLogFile()
        {
            try
            {
                if (!Directory.Exists(Constants.Paths.CombatLogPath))
                    return null;

                var logFiles = Directory.GetFiles(Constants.Paths.CombatLogPath, Constants.Paths.CombatLogExtension)
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .ToArray();

                return logFiles.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public System.DateTime GetFileLastWriteTime(string filePath)
        {
            return File.GetLastWriteTime(filePath);
        }
    }
}
