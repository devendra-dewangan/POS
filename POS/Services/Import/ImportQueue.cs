using System.Threading.Channels;

public class ImportQueue
{
    private readonly Channel<ImportJob> _queue = Channel.CreateUnbounded<ImportJob>();

    public async Task QueueAsync(ImportJob job)
    {
        await _queue.Writer.WriteAsync(job);
    }

    public async Task<ImportJob> DequeueAsync(CancellationToken token)
    {
        return await _queue.Reader.ReadAsync(token);
    }
}

public class ImportJob
{
    public string FilePath { get; set; } = string.Empty;
}