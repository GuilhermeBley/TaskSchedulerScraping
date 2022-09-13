namespace TaskSchedulerScraping.Scraper.Tests.Monitors;

/// <summary>
/// Class used to pause withou lock a thread
/// </summary>
internal class SimpleMonitor
{
    /// <summary>
    /// reset event
    /// </summary>
    private ManualResetEvent _mrse { get; } = new ManualResetEvent(false);

    /// <summary>
    /// Pause thread which enter in method
    /// </summary>
    public void Wait()
    {
        _mrse.Reset();
        _mrse.WaitOne();
    }

    /// <summary>
    /// Pause thread which enter in method
    /// </summary>
    public void Wait(int milliSeconds, Action? ifCancelled = null)
    {
        var cancel = false;

        new Thread(
            () => 
            {
                Thread.Sleep(milliSeconds);
                if (!cancel)
                {
                    ifCancelled?.Invoke();
                    Resume();
                }
            }
        ).Start();

        _mrse.Reset();
        _mrse.WaitOne();
        cancel = true;
    }

    /// <summary>
    /// Resume thread that is in method <see cref="Wait()"/>
    /// </summary>
    public void Resume()
    {
        _mrse.Set();
    }
}