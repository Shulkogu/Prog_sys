using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Model
{
    internal class Logger
    {
        private string LogPath;
        private bool LogInitiated = false;
        private string StatePath;
        private Dictionary<string, Savestate> SaveStates;
        public object Locker = new object();
        public Logger(string LogPath, string StatePath)
        //Method that initializes the logger and create the JSON file that will be used to log the copied files
        {
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
            int Counter = 0;
            do
            {
                Counter++;
                this.LogPath = LogPath + @"\\" + DateTime.Now.ToString(Constants.DateFormat) + "_" + Counter + "." + Constants.Settings.LogFileType.Value;
            } while (System.IO.File.Exists(this.LogPath));
            System.IO.File.WriteAllText(this.LogPath, Constants.GetLoggerHeader());
            this.StatePath = StatePath + "state." + Constants.Settings.LogFileType.Value;
            if (!Directory.Exists(StatePath))
            {
                Directory.CreateDirectory(StatePath);
            }
            SaveStates = new Dictionary<string, Savestate>();
        }
        public void Log(string Name, string SourceFilePath, string TargetFilePath, Workstate State, double FileSize, double CurrentFile, float TransferTime, double EncryptionTime)
        //Method that adds logs the copy of a file. It also calls the method that updates states
        {
            lock (Locker)
            {
                string Log = "";
                if (Constants.Settings.LogFileType.Value == LogFileType.XML)
                {
                    Log = $"\n\t<row>\n" +
                        $"\t\t<Name>{Name}</Name>\n" +
                         $"\t\t<FileSource>{SourceFilePath}</FileSource>\n" +
                         $"\t\t<FileTarget>{TargetFilePath}</FileTarget>\n" +
                         $"\t\t<FileSize>{FileSize}</FileSize>\n" +
                         $"\t\t<TotalFileTransfterTime>{TransferTime}</TotalFileTransfterTime>\n" +
                         $"\t\t<EncryptionTime>{EncryptionTime}</EncryptionTime>\n" +
                         $"\t\t<Time>{DateTime.Now.ToString(Constants.DateTimeFormat)}</Time>\n" +
                         $"\t</row>";
                }
                else
                {
                    Log = (LogInitiated ? "," : "") + $"\n\t{{\n" +
                        $"\t\"Name\": \"{Name}\",\n" +
                        $"\t\"FileSource\": \"{SourceFilePath}\",\n" +
                        $"\t\"FileTarget\": \"{TargetFilePath}\",\n" +
                        $"\t\"FileSize\": {FileSize},\n" +
                        $"\t\"TotalFileTransfterTime\": {TransferTime},\n" +
                        $"\t\"EncryptionTime\": {EncryptionTime},\n" +
                        $"\t\"Time\": \"{DateTime.Now.ToString(Constants.DateTimeFormat)}\"\n\t}}";
                }
                System.IO.File.AppendAllText(LogPath, Log);
                LogInitiated = true;
                UpdateState(Name, SourceFilePath, TargetFilePath, State, CurrentFile);
            }
        }
        public void UpdateState(string Name, string SourceFilePath, string TargetFilePath, Workstate State, double? CurrentFile)
        //Method that updates the states JSON file
        {
            lock (Locker)
            {
                if (SaveStates.ContainsKey(Name))
                {
                    if(CurrentFile != null)
                    {
                        SaveStates[Name].AssignValues(SourceFilePath, TargetFilePath, State, CurrentFile.Value);
                    }
                    else
                    {
                        SaveStates[Name].AssignValues(SourceFilePath, TargetFilePath, State);
                    }
                    string StateContent = Constants.GetLoggerHeader();
                    foreach (var SaveState in SaveStates)
                    {
                        StateContent += SaveState.Value;
                    }
                    if (Constants.Settings.LogFileType.Value == LogFileType.XML)
                    {
                        StateContent += Constants.GetLoggerFooter();
                    }
                    else
                    {
                        StateContent = StateContent.Remove(StateContent.Length - 1, 1) + Constants.GetLoggerFooter();
                    }
                    System.IO.File.WriteAllText(StatePath, StateContent);
                }
            }
        }
        public void FinalizeLogs()
        //Method that adds the final ] to the JSON log file
        {
            lock (Locker)
            {
                System.IO.File.AppendAllText(LogPath, Constants.GetLoggerFooter());
            }
        }
        public void EndState(string Name, double Current)
        //Method that records a job as ended
        {
            UpdateState(Name, "", "", Workstate.END, Current);
        }
        public void CreateState(string Name, double TotalFiles, double TotalSize)
        {
            lock (Locker)
            {
                if (!SaveStates.ContainsKey(Name))
                {
                    SaveStates.Add(Name, new Savestate(Name, TotalFiles, TotalSize));
                }
            }
        }
        public void PauseState(string Name, Workstate workstate)
        {
            if (Workstate.PAUSED_UNPRIORITIZED == workstate || Workstate.PAUSED_USER == workstate || Workstate.PAUSED_FORBIDDENSOFTWARE == workstate)
            {
                UpdateState(Name, "", "", workstate, null);
            }
        }
        public void ResumeState(string Name, Workstate workstate)
        {
            if (Workstate.ACTIVE == workstate)
            {
                UpdateState(Name, "", "", workstate, null);
            }
        }
    }
}