namespace SmartX.Client.Services;

//DELCARATION OF AI USAGE
//Gemini. (2026) . Setup of Command History Service used to incorporate a stack data structure. Available at: 
//https://share.gemini.google/O9Qf6fnMtm4B (Accessed: 18/09/2026).


public sealed record CommandHistoryEntry(string Command, string TargetPath, DateTime ExecutedAtUtc);

public sealed class CommandHistoryService
{
    private const int MaximumHistoryEntries = 50;
    private readonly Stack<CommandHistoryEntry> _history = new();
    private readonly object _sync = new();

    public event Action? Changed;

    public IReadOnlyList<CommandHistoryEntry> GetRecentCommands()
    {
        lock (_sync)
        {
            return _history.ToArray();
        }
    }

    public void Record(string command, string targetPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);

        lock (_sync)
        {
            _history.Push(new CommandHistoryEntry(command, targetPath, DateTime.UtcNow));
            if (_history.Count > MaximumHistoryEntries)
            {
                var recentEntries = _history.Take(MaximumHistoryEntries).Reverse().ToArray();
                _history.Clear();
                foreach (var entry in recentEntries)
                {
                    _history.Push(entry);
                }
            }
        }

        Changed?.Invoke();
    }

    public void Clear()
    {
        lock (_sync)
        {
            if (_history.Count == 0)
            {
                return;
            }

            _history.Clear();
        }

        Changed?.Invoke();
    }
}
