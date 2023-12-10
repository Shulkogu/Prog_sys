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
        private Job Job;
        private string[] RelativeFilePaths;
        private List<Existingsave>? ExistingSaves;
        private Logger Logger;
        public Saver(Job Job, ref Logger Logger)
        {
            this.Job = Job;
            //The next 2 lines make sure that paths are sent with \\ instead of \ and fixes them if necessary, to comply with UNC
            this.Job.SourcePath = Regex.Replace(this.Job.SourcePath, Constants.SingleToDoubleBackslashRegex, @"\\"); //Uses a Regex that only captures backslashes in groups of 1 or 3+ and replaces them with 2 backslashes
            this.Job.TargetPath = Regex.Replace(this.Job.TargetPath, Constants.SingleToDoubleBackslashRegex, @"\\"); //Same as source path
            //The next 5 lines retrieve the paths of all the files in the source folder and its sub-folders
            //To do so, the native GetFiles method is used, but, as it retrieves full paths, the source paths are then individually trimmed
            //using the native GetRelativePath method.
            RelativeFilePaths = Directory.GetFiles(this.Job.SourcePath, "*", SearchOption.AllDirectories);
            for (int i = 0; i < RelativeFilePaths.Length; i++)
            {
                RelativeFilePaths[i] = Path.GetRelativePath(this.Job.SourcePath, RelativeFilePaths[i]).Replace(@"\", @"\\");
            }
            //The following code will find all the previously made differential saves according to folder naming conventions and sort them according to their save number
            ExistingSaves = new List<Existingsave>();
            Regex SaveFolderType = new Regex("^(\\d+)_(?:0[1-9]|[12][0-9]|3[01])-(?:0[1-9]|1[1,2])-(?:19|20)\\d{2}$");
            foreach (string Directory in Directory.GetDirectories(this.Job.TargetPath))
            {
                MatchCollection Match = SaveFolderType.Matches(Path.GetRelativePath(this.Job.TargetPath, Directory));
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
            double TotalSize = RelativeFilePaths.Sum(x => new FileInfo(this.Job.SourcePath + @"\\" + x).Length);
            double Counter = 1; //Keeps track of the current file N°
            double WereCopied = 0; //Keeps track of the amount of files that were actually copied (which means no error occured and in case of a differential save, it was copied because new or different than the last version)
            string FolderName = (ExistingSaves != null && ExistingSaves.Count > 0 ? Convert.ToString(ExistingSaves[0].SaveNumber + 1) : "1") + "_" + DateTime.Now.ToString(Constants.DateFormat);
            Directory.CreateDirectory(this.Job.TargetPath + @"\\" + FolderName);
            foreach (string RelativePath in RelativeFilePaths)
            {
                var Time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                File File = new File(ref FolderName, ref this.Job, RelativePath, ref ExistingSaves);
                double? Size = File.Save();
                //If File.Save() returned null, the file wasn't copied because it didn't need to. If it returns a negative value, an error occured
                if (Size.HasValue)
                {
                    Logger.Log(this.Job.Name, this.Job.SourcePath + @"\\" + RelativePath, this.Job.TargetPath + @"\\" + FolderName + @"\\" + RelativePath, Workstate.ACTIVE, RelativeFilePaths.Length, TotalSize, Size.Value, Counter, DateTimeOffset.Now.ToUnixTimeMilliseconds() - Time);
                    if (Size.Value >= 0)
                    {
                        WereCopied++;
                    }
                }
                Counter++;
            }
            Logger.EndState(this.Job.Name, (WereCopied > 0) ? RelativeFilePaths.Length:0);
        }
    }
}
