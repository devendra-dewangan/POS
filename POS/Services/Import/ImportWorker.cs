using POS.Services.Import;

public class ImportWorker : BackgroundService
{
    private readonly ImportQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;

    public ImportWorker(ImportQueue queue, IServiceScopeFactory scopeFactory)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var job = await _queue.DequeueAsync(stoppingToken);

            using var scope = _scopeFactory.CreateScope();

            var importService = scope.ServiceProvider
                .GetRequiredService<IImportService>();

            await importService.ImportPurchaseDataAsync(job.FilePath);
        }
    }
}