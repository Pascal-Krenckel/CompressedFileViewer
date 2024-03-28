using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

    public static ObservableCollection<LogEntry> LogEntries { get; set; } = new();

}
public class LogEntry : PropertyChangedBase
{
    public DateTime DateTime { get; set; }

    public string Message { get; set; } = String.Empty;
}

public class CollapsibleLogEntry : LogEntry
{
    public List<LogEntry> Contents { get; set; } = new();
}

public class PropertyChangedBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
            _ = Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }));
    }
}