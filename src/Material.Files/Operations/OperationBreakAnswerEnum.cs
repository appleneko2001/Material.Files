namespace Material.Files.Operations
{
    public enum OperationBreakAnswerEnum
    {
        /// <summary>
        /// Abort task (cancel queued tasks).
        /// </summary>
        Abort,
        
        /// <summary>
        /// Skip failed task and continue.
        /// </summary>
        Continue,
        
        /// <summary>
        /// Retry failed task.
        /// </summary>
        Retry,
    }

    public enum FileExistsAnswer
    {
        Overwrite = 1,
        Cancel = 2,
        None = 0,
    }
}