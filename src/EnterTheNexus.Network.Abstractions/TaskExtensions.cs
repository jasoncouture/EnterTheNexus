namespace EnterTheNexus.Network.Abstractions;

public static class TaskExtensions
{
    public static async void Orphan(this Task task, object? state = null, Action<Exception, object?>? onFaulted = null)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            onFaulted?.Invoke(ex, state);
        }
    }
}