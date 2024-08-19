using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace CompressedFileViewer.Windows;
/// <summary>
/// Interaktionslogik für AboutDialog.xaml
/// </summary>
public partial class AboutDialog : Window
{
    public AboutDialog()
    {
        InitializeComponent();
        Assembly = Assembly.GetCallingAssembly();

    }
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Info?.Dispose();
    }
    public AboutDialog(Assembly assembly)
    {
        InitializeComponent();
        Assembly = assembly;
    }

    private Assembly assembly;

    public Assembly Assembly
    {
        get => assembly;
        [MemberNotNull(nameof(assembly))]
        set
        {
            assembly = value;
            Info?.Dispose();
            DataContext = Info = new AssemblyInfo(assembly);
        }
    }



    private static readonly DependencyProperty AssemblyInfoProperty = DependencyProperty.Register(nameof(AssemblyInfo),typeof(AssemblyInfo),typeof(AboutDialog),new PropertyMetadata(OnAssemblyInfoChanged));

    private static void OnAssemblyInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        AboutDialog aboutDialog = (AboutDialog)d;
        System.Drawing.Icon? icon = aboutDialog.Info.Icon;
        if (aboutDialog.Icon != null)
            aboutDialog.IconDisplay.Source = aboutDialog.Icon;
        else if (icon != null)
            aboutDialog.IconDisplay.Source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

    }

    private AssemblyInfo Info { get => (AssemblyInfo)GetValue(AssemblyInfoProperty); set => SetValue(AssemblyInfoProperty, value); }

    private class AssemblyInfo : IDisposable
    {
        public AssemblyInfo(Assembly assembly)
        {
            Assembly = assembly;


            Title = Assembly.GetCustomAttributes<AssemblyTitleAttribute>().FirstOrDefault()?.Title;

            TargetFramework = Assembly.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()?.FrameworkDisplayName;

            Company = Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;

            Configuration = Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration;

            Copyright = Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;


            CultureName = Assembly.GetCustomAttribute<AssemblyCultureAttribute>()?.Culture;

            Description = Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

            FileVersion = Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;

            ProductName = Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;

            Version = Assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? Assembly.GetName().Version?.ToString();
            Icon = Icon.ExtractAssociatedIcon(Assembly.Location);
        }

        public Assembly Assembly { get; }
        public string? Title { get; }
        public string? TargetFramework { get; }
        public string? Company { get; }
        public string? Configuration { get; }
        public string? Copyright { get; }
        public string? CultureName { get; }
        public string? Description { get; }
        public string? FileVersion { get; }
        public string? ProductName { get; }
        public string? Version { get; }
        public Icon? Icon { get; }

        public void Dispose() => Icon?.Dispose();
    }

    private void Button_Click(object sender, RoutedEventArgs e) => Close();
}
