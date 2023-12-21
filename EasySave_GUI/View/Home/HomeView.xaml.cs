using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EasySave_GUI.JobList;

namespace EasySave_GUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class HomeView : Window
{
    public HomeView()
    {
        InitializeComponent();
    }
    private void btnEnglish_Click(object sender, RoutedEventArgs e)
    {
        SwitchLanguage(Model.Language.English);
    }
    private void btnFrench_Click(object sender, RoutedEventArgs e)
    {
        SwitchLanguage(Model.Language.French);
    }
    
    public void SwitchLanguage(Model.Language language)
    {
        ResourceDictionary dictionary = new ResourceDictionary();
        switch (language)
        {
            case Model.Language.French:
                dictionary.Source = new Uri("Languages/StringResources.fr.xaml", UriKind.Relative);
                break;
            default:
                dictionary.Source = new Uri("Languages/StringResources.en.xaml", UriKind.Relative);
                break;
        }
        this.Resources.MergedDictionaries.Add(dictionary);
    }
    private void GUI_Closed(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Application.Current.Shutdown();
        Thread thread = new Thread(() =>
        {
            Thread.Sleep(2000);
            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.Kill();
        });
        thread.Start();
    }
}