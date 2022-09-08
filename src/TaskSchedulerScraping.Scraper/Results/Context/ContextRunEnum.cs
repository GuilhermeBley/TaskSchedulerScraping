public enum ContextRunEnum : sbyte
{
    /// <summary>
    /// Should be stopped
    /// </summary>
    Paused = 0,

    /// <summary>
    /// Should be in executing
    /// </summary>
    Running = 1,

    /// <summary>
    /// Should be Disposed
    /// </summary>
    Disposed = 3,

    /// <summary>
    /// Should be Disposed
    /// </summary>
    DisposedWithError = 4,

    /// <summary>
    /// Should be Disposed
    /// </summary>
    DisposedBecauseIsFinished = 5
}