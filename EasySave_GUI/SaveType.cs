using System.Collections.ObjectModel;

namespace EasySave_GUI;

class SaveType : ObservableCollection<string>
{
    public SaveType ()
    {
        Add("Complete");
        Add("Differential");
    }
}