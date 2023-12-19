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
        public event EventHandler PrioritizedCopyStarted;
        public event EventHandler PrioritizedCopyEnded;
        public Job Job;
        public FilePriority CurrentPriority;
        public Workstate Workstate;
        public ManualResetEvent ManualResetEvent;
        private List<(string, FilePriority)> RelativeFilePaths = new List<(string, FilePriority)>();
        private List<Existingsave>? ExistingSaves;
        private Logger Logger;
        private object Lock;
        public Saver(Job Job, ref Logger Logger, ref object LockPriorityEvent)
        {
            this.Lock = LockPriorityEvent;
            this.ManualResetEvent= new ManualResetEvent(true);
            this.Job = Job;
            //The next 2 lines make sure that paths are sent with \\ instead of \ and fixes them if necessary, to comply with UNC
            this.Job.SourcePath = Regex.Replace(this.Job.SourcePath, Constants.SingleToDoubleBackslashRegex, @"\\"); //Uses a Regex that only captures backslashes in groups of 1 or 3+ and replaces them with 2 backslashes
            this.Job.TargetPath = Regex.Replace(this.Job.TargetPath, Constants.SingleToDoubleBackslashRegex, @"\\"); //Same as source path
            //The next 5 lines retrieve the paths of all the files in the source folder and its sub-folders
            //To do so, the native GetFiles method is used, but, as it retrieves full paths, the source paths are then individually trimmed
            //using the native GetRelativePath method.
            string[] FilePaths = Directory.GetFiles(this.Job.SourcePath, "*", SearchOption.AllDirectories);
            FilePaths = FilePaths.OrderBy(filePath => new FileInfo(filePath).Length).ToArray();
            for (int i = 0; i < FilePaths.Length; i++)
            {
                RelativeFilePaths.Add((Path.GetRelativePath(this.Job.SourcePath, FilePaths[i]).Replace(@"\", @"\\"), Constants.Settings.PrioritizedExtensions.Contains(Regex.Match(FilePaths[i], Constants.GetExtensionRegex).Value, StringComparer.OrdinalIgnoreCase) ? FilePriority.HIGH : FilePriority.NORMAL));
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
            Workstate = Workstate.ACTIVE;
            double TotalSize = RelativeFilePaths.Select(x => x.Item1).Sum(x => new FileInfo(this.Job.SourcePath + @"\\" + x).Length);
            double Counter = 1; //Keeps track of the current file N°
            string FolderName = (ExistingSaves != null && ExistingSaves.Count > 0 ? Convert.ToString(ExistingSaves[0].SaveNumber + 1) : "1") + "_" + DateTime.Now.ToString(Constants.DateFormat);
            Directory.CreateDirectory(this.Job.TargetPath + @"\\" + FolderName);
            Logger.CreateState(this.Job.Name, RelativeFilePaths.Count, TotalSize);
            foreach (FilePriority FilePriority in Enum.GetValues(typeof(FilePriority)))
            {
                if (RelativeFilePaths.Any(x => x.Item2 == FilePriority))
                {
                    CurrentPriority = FilePriority;
                    if(FilePriority == FilePriority.HIGH) //If the file priority is one that has priority, tell the orchestrator
                    {
                        lock (Lock) //Prevents multiple savers from informing the orchestrator at once, which would put all the savers in a paused state
                        {
                            ManualResetEvent.WaitOne(); //If the saver is paused, it will wait until sending its current FilePriority
                            PrioritizedCopyStarted?.Invoke(this, EventArgs.Empty); //Tell the orchestrator that the copy of prioritized files started
                        }
                    }
                    //Do something to say it's currently working on files with priority
                    foreach (string RelativePath in RelativeFilePaths.Where(x => x.Item2 == FilePriority).Select(x => x.Item1))
                    {
                        ManualResetEvent.WaitOne(); //If the saver is paused, it will wait until resumed before copying the next file
                        var Time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        File File = new File(ref FolderName, ref this.Job, RelativePath, Constants.Settings.EncryptedExtensions.Contains(Regex.Match(RelativePath, Constants.GetExtensionRegex).Value, StringComparer.OrdinalIgnoreCase), ref ExistingSaves);
                        (double?, double) FileInfo = File.Save();
                        //If File.Save() returned null, the file wasn't copied because it didn't need to. If it returns a negative value, an error occured
                        if (FileInfo.Item1.HasValue)
                        {
                            Logger.Log(this.Job.Name, this.Job.SourcePath + @"\\" + RelativePath, this.Job.TargetPath + @"\\" + FolderName + @"\\" + RelativePath, this.Workstate, FileInfo.Item1.Value, Counter, DateTimeOffset.Now.ToUnixTimeMilliseconds() - Time, FileInfo.Item2);
                        }
                        else
                        {
                            Logger.UpdateState(this.Job.Name, this.Job.SourcePath + @"\\" + RelativePath, this.Job.TargetPath + @"\\" + FolderName + @"\\" + RelativePath, this.Workstate, Counter);
                        }
                        Counter++;
                    }
                    PrioritizedCopyEnded?.Invoke(this, EventArgs.Empty);
                }
            }
            Logger.EndState(this.Job.Name, RelativeFilePaths.Count);
        }
    }
}
