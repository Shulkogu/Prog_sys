using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EasySave_GUI.ListView;

namespace EasySave_GUI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class HomeWindow : Window
{
    public HomeWindow()
    {
        InitializeComponent();
        
        var FrenchImage = new BitmapImage(new Uri("pack://application:,,,/EasySave_GUI;component/icon/france.png"));
        Resources["MyDynamicImage"] = FrenchImage;
        var EnglishImage = new BitmapImage(new Uri("pack://application:,,,/EasySave_GUI;component/icon/royaume-uni.png"));
        Resources["MyDynamicImage"] = EnglishImage;
        var IconIcon = new BitmapImage(new Uri("pack://application:,,,/EasySave_GUI;component/icon/36020.ico"));
        Resources["MyDynamicImage"] = IconIcon;
        var HomeImage = new BitmapImage(new Uri("pack://application:,,,/EasySave_GUI;component/icon/accueil.png"));
        Resources["MyDynamicImage"] = HomeImage;
        var SaveImage = new BitmapImage(new Uri("pack://application:,,,/EasySave_GUI;component/icon/disquette.png"));
        Resources["MyDynamicImage"] = SaveImage;
        var LogsImage = new BitmapImage(new Uri("pack://application:,,,/EasySave_GUI;component/icon/un-journal.png"));
        Resources["MyDynamicImage"] = LogsImage;
        var SettingsImage = new BitmapImage(new Uri("pack://application:,,,/EasySave_GUI;component/icon/parametres.png"));
        Resources["MyDynamicImage"] = SettingsImage;
        var CreateImage = new BitmapImage(new Uri("pack://application:,,,/EasySave_GUI;component/icon/creer.png"));
        Resources["MyDynamicImage"] = CreateImage;
        var ModifyImage = new BitmapImage(new Uri("pack://application:,,,/EasySave_GUI;component/icon/bouton-modifier.png"));
        Resources["MyDynamicImage"] = ModifyImage;
    }
}