using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace CompressedFileViewer.Windows;
/// <summary>
/// Interaktionslogik für Log.xaml <br/>
/// see 
/// </summary>
public partial class Log : Window
{

    public Log()
    {
        InitializeComponent();


        DataContext = LogEntries;

    }

    public static ObservableCollection<LogEntry> LogEntries { get; set; } = [];

}
public class LogEntry : PropertyChangedBase
{
    public DateTime DateTime { get; set; }

    public string Message { get; set; } = string.Empty;
}

public class CollapsibleLogEntry : LogEntry
{
    public List<LogEntry> Contents { get; set; } = [];
}

public class PropertyChangedBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
            _ = Application.Current.Dispatcher.BeginInvoke(() =>
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            });
    }
}