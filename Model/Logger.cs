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
        public Logger(string LogPath, string StatePath)
        //Method that initializes the logger and create the JSON file that will be used to log the copied files
        {
            int Counter = 0;
            do
            {
                Counter++;
                this.LogPath = LogPath + @"\\" + DateTime.Now.ToString(Constants.DateFormat) + "_" + Counter + ".json";
            } while (System.IO.File.Exists(this.LogPath));
            System.IO.File.WriteAllText(this.LogPath, "[");
            this.StatePath = StatePath + @"\\state.json";
            SaveStates = new Dictionary<string, Savestate>();
        }
        public void Log(string Name, string SourceFilePath, string TargetFilePath, Workstate State, double TotalFiles, double TotalSize, double FileSize, double CurrentFile, float TransferTime)
        //Method that adds logs the copy of a file. It also calls the method that updates states
        {
            string Log = (LogInitiated ? "," : "") + $"\n{{\n" +
                $"\"Name\": \"{Name}\",\n" +
                $"\"FileSource\": \"{SourceFilePath}\",\n" +
                $"\"FileTarget\": \"{TargetFilePath}\",\n" +
                $"\"FileSize\": {FileSize},\n" +
                $"\"FileTransfterTime\": {TransferTime},\n" +
                $"\"Time\": \"{DateTime.Now.ToString(Constants.DateTimeFormat)}\"\n}}";
            System.IO.File.AppendAllText(LogPath, Log);
            LogInitiated = true;
            UpdateState(Name, TotalFiles, TotalSize, SourceFilePath, TargetFilePath, State, CurrentFile);
        }
        private void UpdateState(string Name, double TotalFiles, double TotalSize, string SourceFilePath, string TargetFilePath, Workstate State, double CurrentFile)
        //Method that updates the states JSON file
        {
            if (!SaveStates.ContainsKey(Name))
            {
                SaveStates.Add(Name, new Savestate(Name, TotalFiles, TotalSize));
            }
            SaveStates[Name].AssignValues(SourceFilePath, TargetFilePath, State, CurrentFile);
            string StateContent = "[";
            foreach (var SaveState in SaveStates)
            {
                StateContent += $"\n{{\n{SaveState.Value}}},";
            }
            StateContent = StateContent.Remove(StateContent.Length - 1, 1) + "\n]";
            System.IO.File.WriteAllText(StatePath, StateContent);
        }
        public void FinalizeLogs()
        //Method that adds the final ] to the JSON log file
        {
            System.IO.File.AppendAllText(LogPath, "\n]");
        }
        public void EndState(string Name, double Current)
        //Method that records a job as ended
        {
            UpdateState(Name, 0, 0, "", "", Workstate.END, Current);
        }
    }
}