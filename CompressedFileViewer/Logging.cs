using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressedFileViewer;
public static class Logging
{
    public static void Log(string message)
    {
        Windows.Log.LogEntries.Add(new Windows.LogEntry() { DateTime = DateTime.Now, Message = message });
    }

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

    public static void Log(Exception ex)
    {
        Windows.Log.LogEntries.Add(GetLog(ex, DateTime.Now));
    }

    public static void Log(LogTree logTree)
    {
        var dateTime = DateTime.Now;
        Windows.Log.LogEntries.Add(logTree.ToLogEntry(DateTime.Now));
    }

    public static void Log(string message, string subMessage)
    {
        var entry = new LogTree(message);
        _ = entry.AppendMessage(subMessage);
        Log(entry);
    }

    public class LogTree
    {
        public LogTree(string message) { Message = message; }
        public LogTree(string message, params LogTree[] child)
        {
            Message = message;
            _children.AddRange(child);
        }
        public string Message { get; }

        private List<LogTree> _children = new();
        public LogTree AppendMessage(string message) { _children.Add(new LogTree(message)); return _children.Last(); }

        public Windows.LogEntry ToLogEntry(DateTime now)
        {
            if (_children.Count == 0)
                return new Windows.LogEntry() { DateTime = now, Message = Message };
            return new Windows.CollapsibleLogEntry() 
            { 
                DateTime = now, 
                Message = Message,
                Contents = _children.Select(child =>  child.ToLogEntry(now)).ToList()
            };
        }
    }
}
