namespace CompressedFileViewer;
public static class Logging
{
    public static void Log(string message) => Windows.Log.LogEntries.Add(new Windows.LogEntry() { DateTime = DateTime.Now, Message = message });

    private static Windows.LogEntry GetLog(Exception ex, DateTime dateTime)
    {
        if (ex.InnerException != null)
        {
            Windows.CollapsibleLogEntry entry = new()
            {
                DateTime = dateTime,
                Message = ex.ToString(),
                Contents = [GetLog(ex.InnerException,dateTime)]

            };
            return entry;
        }
        else
        {
            return new Windows.LogEntry() { DateTime = DateTime.Now, Message = ex.ToString() };
        }
    }

    public static void Log(Exception ex) => Windows.Log.LogEntries.Add(GetLog(ex, DateTime.Now));

    public static void Log(LogTree logTree)
    {
        _ = DateTime.Now;
        Windows.Log.LogEntries.Add(logTree.ToLogEntry(DateTime.Now));
    }

    public static void Log(string message, string subMessage)
    {
        LogTree entry = new(message);
        _ = entry.AppendMessage(subMessage);
        Log(entry);
    }

    public class LogTree
    {
        public LogTree(string message) => Message = message;
        public LogTree(string message, params LogTree[] child)
        {
            Message = message;
            _children.AddRange(child);
        }
        public string Message { get; }

        private readonly List<LogTree> _children = [];
        public LogTree AppendMessage(string message) { _children.Add(new LogTree(message)); return _children.Last(); }

        public Windows.LogEntry ToLogEntry(DateTime now) => _children.Count == 0
                ? new Windows.LogEntry() { DateTime = now, Message = Message }
                : new Windows.CollapsibleLogEntry()
                {
                    DateTime = now,
                    Message = Message,
                    Contents = _children.Select(child => child.ToLogEntry(now)).ToList()
                };
    }
}
