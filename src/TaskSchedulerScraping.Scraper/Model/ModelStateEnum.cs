namespace TaskSchedulerScraping.Scraper.Model;

/// <summary>
/// Identifier state from model
/// </summary>
public enum ModelStateEnum : sbyte
{
    /// <summary>
    /// Not running yet
    /// </summary>
    NotRunning = -1,

    /// <summary>
    /// Not running yet, but in process to run
    /// </summary>
    WaitingRunning = 0,

    /// <summary>
    /// Running
    /// </summary>
    Running = 1,

    /// <summary>
    /// Waiting to pause
    /// </summary>
    WaitingPause = 2,

    /// <summary>
    /// Paused
    /// </summary>
    Paused = 3,

    /// <summary>
    /// Waiting Dispose
    /// </summary>
    WaitingDispose = 4,

    /// <summary>
    /// Disposed
    /// </summary>
    Disposed = 5

    
}