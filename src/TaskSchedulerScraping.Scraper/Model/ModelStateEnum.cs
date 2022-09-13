namespace TaskSchedulerScraping.Scraper.Model;

/// <summary>
/// Identifier state from model
/// </summary>
public enum ModelStateEnum : sbyte
{
    /// <summary>
    /// Not running yet
    /// </summary>
    NotRunning = 0,

    /// <summary>
    /// Running
    /// </summary>
    Running = 1,

    /// <summary>
    /// Paused
    /// </summary>
    Paused = 2,

    /// <summary>
    /// Disposed
    /// </summary>
    Disposed = 3
}