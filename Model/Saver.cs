using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Model
{
    internal class Saver
    {
        private string Name;
        private string SourcePath;
        private string TargetPath;
        private Savetype SaveType;
        private string[] RelativeFilePaths;
        private List<Existingsave>? ExistingSaves;
        private Logger Logger;
        public Saver(string Name, string SourcePath, string TargetPath, Savetype SaveType, ref Logger Logger)
        {
            this.Name = Name;
            this.SourcePath = SourcePath;
            this.TargetPath = TargetPath;
            this.SaveType = SaveType;
            // The next 5 lines retrieve the paths of all the files in the source folder and its sub-folders
            // To do so, the native GetFiles method is used, but, as it retrieves full paths, the source paths are then individually trimmed
            // using the native GetRelativePath method.
            RelativeFilePaths = Directory.GetFiles(this.SourcePath, "*", SearchOption.AllDirectories);
            for (int i = 0; i < RelativeFilePaths.Length; i++)
            {
                RelativeFilePaths[i] = Path.GetRelativePath(this.SourcePath, RelativeFilePaths[i]).Replace(@"\", @"\\");
            }
            // The following code will find all the previously made differential saves according to folder naming conventions and sort them according to their save number
            ExistingSaves = new List<Existingsave>();
            Regex SaveFolderType = new Regex("^(\\d+)_(?:0[1-9]|[12][0-9]|3[01])-(?:0[1-9]|1[1,2])-(?:19|20)\\d{2}$");
            foreach (string Directory in Directory.GetDirectories(this.TargetPath))
            {
                MatchCollection Match = SaveFolderType.Matches(Path.GetRelativePath(this.TargetPath, Directory));
                if (Match.Count == 1)
                {
                    ExistingSaves.Add(new Existingsave(Directory, Convert.ToInt32(Match[0].Groups[1].Value)));
                }
            }
            ExistingSaves = ExistingSaves.OrderByDescending(x => x.SaveNumber).ToList();
            this.Logger = Logger;
        }
        public void SaveFiles()
        //For all the paths within RelativeFilePaths, a File object will be created, the object will have the responsibility of saving the file or not. The time it took to treat the file is counted, and the logger is called.
        {
            double TotalSize = RelativeFilePaths.Sum(x => new FileInfo(SourcePath + @"\\" + x).Length);
            double Counter = 1;
            double WereCopied = 0;
            string FolderName = (ExistingSaves != null && ExistingSaves.Count > 0 ? Convert.ToString(ExistingSaves[0].SaveNumber + 1) : "1") + "_" + DateTime.Now.ToString(Constants.DateFormat);
            Directory.CreateDirectory(TargetPath + @"\\" + FolderName);
            foreach (string RelativePath in RelativeFilePaths)
            {
                var Time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                File File = new File(ref FolderName, ref SourcePath, ref TargetPath, ref SaveType, RelativePath, ref ExistingSaves);
                double Size = File.Save();
                if (Size > -1)
                {
                    Logger.Log(Name, SourcePath + @"\\" + RelativePath, TargetPath + @"\\" + FolderName + @"\\" + RelativePath, Workstate.ACTIVE, RelativeFilePaths.Length, TotalSize, Size, Counter, DateTimeOffset.Now.ToUnixTimeMilliseconds() - Time);
                    WereCopied++;
                }
                Counter++;
            }
            Logger.EndState(Name, (WereCopied > 0) ? RelativeFilePaths.Length:0);
        }
    }
}
