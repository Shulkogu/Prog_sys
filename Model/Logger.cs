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
        private Dictionary<string, Savestate> SavedStates;
        public Logger(string LogPath, string StatePath)
        {
            int Counter = 0;
            do
            {
                Counter++;
                this.LogPath = LogPath + @"\\" + DateTime.Now.ToString(Constants.DateFormat) + "_" + Counter + ".json";
            } while (System.IO.File.Exists(this.LogPath));
            System.IO.File.WriteAllText(this.LogPath, "[");
            this.StatePath = StatePath + @"\\state.json";
            SavedStates = new Dictionary<string, Savestate>();
        }
        public void Log(string Name, string SourceFilePath, string TargetFilePath, Workstate State, double TotalFiles, double TotalSize, double FileSize, double CurrentFile, float TransferTime)
        {
            string Log = (LogInitiated ? "," : "") + $"\n{{" +
                $"\n\"Name\": \"{Name}\",\n" +
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
        {
            if (!SavedStates.ContainsKey(Name))
            {
                SavedStates.Add(Name, new Savestate(Name, TotalFiles, TotalSize));
            }
            SavedStates[Name].AssignValues(SourceFilePath, TargetFilePath, State, CurrentFile);
            string StateContent = "[";
            foreach (var SavedState in SavedStates)
            {
                StateContent += $"\n{{\n{SavedState.Value}}},";
            }
            StateContent = StateContent.Remove(StateContent.Length - 1, 1) + "\n]";
            System.IO.File.WriteAllText(StatePath, StateContent);
        }
        public void FinalizeLogs()
        {
            System.IO.File.AppendAllText(LogPath, "\n]");
        }
        public void EndState(string Name, double Current)
        {
            UpdateState(Name, 0, 0, "", "", Workstate.END, Current);
        }
    }
}