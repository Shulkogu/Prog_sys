using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace Model
{
    internal class Logger
    {
        public event EventHandler StatesUpdated;
        private string LogPath;
        private bool LogInitiated = false;
        private string StatePath;
        public Dictionary<string, Savestate> SaveStates;
        public object Locker = new object();
        public Logger(string LogPath, string StatePath)
        //Method that initializes the logger and create the JSON file that will be used to log the copied files
        {
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
            this.LogPath = LogPath + @"\\" + DateTime.Now.ToString(Constants.DateFormat) + "." + Constants.Settings.LogFileType.Value;
            bool ProperFormat = false;
            //If a daily log file already exists
            if (System.IO.File.Exists(this.LogPath))
            {
                //It is opened
                FileStream File = new(this.LogPath, FileMode.Open);
                byte[] Beginning = Encoding.UTF8.GetBytes(Constants.GetLoggerHeader() + "\n");
                byte[] Ending = Encoding.UTF8.GetBytes(Constants.GetLoggerFooter());
                //Its size is checked to see if it is big enough to contain at least a header, footer and some data
                if (File.Length > Beginning.Length + Ending.Length)
                {
                    byte[] BeginningChunk = new byte[Beginning.Length];
                    File.ReadExactly(BeginningChunk, 0, Beginning.Length);
                    //We check if the file starts with the typical header
                    if (BeginningChunk.AsSpan().SequenceEqual(Beginning))
                    {
                        byte[] EndingChunk = new byte[Ending.Length];
                        File.Seek(-Ending.Length, SeekOrigin.End);
                        File.ReadExactly(EndingChunk, 0, Ending.Length);
                        //Then, if it ends with the typical footer
                        if(EndingChunk.AsSpan().SequenceEqual(Ending))
                        {
                            //If all the tests passed, the original footer is removed and the logging can start on that file
                            ProperFormat = true;
                            File.SetLength(File.Length - Ending.Length);
                            LogInitiated = true;
                        }
                    }
                }
                File.Close();
            }
            //If there exist no daily log with the right format, a new file is created with the proper header
            if(!ProperFormat)
            {
                System.IO.File.WriteAllText(this.LogPath, Constants.GetLoggerHeader());
            }
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
                //If the current log file isn't named according to the current date, a new file is created with the current date.
                //Example: Log process is started at 23:59 and a file is copied at 00:00 (the next day)
                if (!this.LogPath.EndsWith(@"\\" + DateTime.Now.ToString(Constants.DateFormat) + "." + Constants.Settings.LogFileType.Value))
                {
                    this.LogPath = LogPath + @"\\" + DateTime.Now.ToString(Constants.DateFormat) + "." + Constants.Settings.LogFileType.Value;
                    System.IO.File.WriteAllText(this.LogPath, Constants.GetLoggerHeader());
                    LogInitiated = false;
                }
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
                    StatesUpdated?.Invoke(this, EventArgs.Empty);
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
        public void CreateState(string Name, double TotalFiles, double TotalSize, Workstate Workstate)
        {
            lock (Locker)
            {
                if (!SaveStates.ContainsKey(Name))
                {
                    SaveStates.Add(Name, new Savestate(Name, TotalFiles, TotalSize));
                    UpdateState(Name, "", "", Workstate,null);
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