using var cts = new CancellationTokenSource();

Console.CancelKeyPress += async (sender, eventArgs) =>
{
    Console.WriteLine("\nCancellation requested. Shutting down...");
    eventArgs.Cancel = true;
    cts.Cancel();
};

Console.WriteLine("Press Ctrl+C to exit.");

try
{
    while (true)
    {
        cts.Token.ThrowIfCancellationRequested();
        Console.WriteLine("Waiting...");
        Thread.Sleep(500);
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Work has been succesfully cancelled");
}

Console.WriteLine("Application closed gracefully.");
