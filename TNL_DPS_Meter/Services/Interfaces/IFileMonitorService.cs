namespace TNL_DPS_Meter.Services.Interfaces
{
    /// <summary>
    /// Interface for combat log file monitoring service
    /// </summary>
    public interface IFileMonitorService
    {
        /// <summary>
        /// Gets path to the latest combat log file
        /// </summary>
        /// <returns>File path or null if no files found</returns>
        string? GetLatestCombatLogFile();

        /// <summary>
        /// Checks if specified file exists
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <returns>true if file exists</returns>
        bool FileExists(string filePath);

        /// <summary>
        /// Gets file last modification time
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <returns>Modification time</returns>
        System.DateTime GetFileLastWriteTime(string filePath);
    }
}
