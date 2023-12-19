using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Model
{
    internal class Job
    {
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public Savetype SaveType { get; set; }
        public override string ToString()
        {
            return $"Name: {Name}\nSource: {SourcePath}\nTarget: {TargetPath}\nType: {SaveType}";
        }
    }
    public static class Constants
    {
        //Constants used in various places within the program
        public static readonly string DateFormat = @"dd-MM-yyyy";
        public static readonly string DateTimeFormat = @"dd-MM-yyyy HH:mm:ss";
        public static readonly string ExeLocation = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string JobsFile = ExeLocation+@"\\jobs.json";
        public static readonly string SettingsFile = ExeLocation + @"\\settings.json";
        public static readonly string LogPath = ExeLocation+@"\\logs\\files\\";
        public static readonly string StatePath = ExeLocation+@"\\logs\\state\\";
        public static readonly string CryptoSoft = ExeLocation + @"\\CryptoSoft.exe";
        public static readonly string SingleToDoubleBackslashRegex = @"(?<!\\)\\{1}(?!\\)|\\{3,}";
        public static readonly string GetExtensionRegex = @"\..+$";
        public static readonly Settings Settings = new Settings();
        public static ManualResetEvent SmallFilesAuthorized = new ManualResetEvent(true);
        private static object Lock = new object();
        public static void BlockBigFiles()
        {
            lock (Lock)
            {
                SmallFilesAuthorized.Reset();
            }
        }
        public static void AuthorizeBigFiles()
        {
            lock (Lock)
            {
                SmallFilesAuthorized.Set();
            }
        }
        public static string GetLoggerHeader()
        {
            switch (Constants.Settings.LogFileType.Value)
            {
                case LogFileType.XML:
                    return ("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n<root>");
                default:
                    return("[");
            }
        }
        public static string GetLoggerFooter()
        {
            switch (Constants.Settings.LogFileType.Value)
            {
                case LogFileType.XML:
                    return ("\n</root>");
                default:
                    return ("\n]");
            }
        }
    }
    public class MutableEnum<TEnum>
    {
        //Class that encapsulates the Settings' Enums to make them mutable.
        //This way, if any other Enum is added, the view's "adjust settings" tab automatically proposes them to the user without needing to modify its code
        public TEnum Value { get; set; }

        public MutableEnum(TEnum value)
        {
            Value = value;
        }
    }
    public class Settings
    {
        //Singleton class that contains the program's settings
        private static Lazy<Settings> _cache = new(() => new());
        public static Settings Instance => _cache.Value;
        [JsonInclude]
        public MutableEnum<View.Language> Language = new MutableEnum <View.Language>(View.Language.English);
        [JsonInclude]
        public MutableEnum<LogFileType> LogFileType = new MutableEnum <LogFileType>(Model.LogFileType.JSON);
        [JsonInclude]
        public List<string> EncryptedExtensions = new List<string>();
        [JsonInclude]
        public string ForbiddenSoftware = "notepad";
        [JsonInclude]
        public long MaxSimultaneousFileSize = 40000;
        [JsonInclude]
        public List<string> PrioritizedExtensions = [".jpg"];
        public bool LoadSettings()
        //Function used to load the settings stored in the settings file.
        //Returns true if a file existed and was loaded, otherwise, false.
        {
            if (System.IO.File.Exists(Constants.SettingsFile))
            //If the settings file exists, it gets loaded into the object
            {
                try
                {
                    Settings Loaded = JsonSerializer.Deserialize<Settings>(System.IO.File.ReadAllText(Constants.SettingsFile))!;
                    this.Language=Loaded.Language;this.LogFileType=Loaded.LogFileType; this.EncryptedExtensions=Loaded.EncryptedExtensions;
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }
        public void SaveSettings()
        {
            string json = System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(Constants.SettingsFile, json);
        }
    }
    public struct Existingsave
    {
        //Characteristics of a save that has already been done within a job
        public string Path;
        public int SaveNumber;
        public Existingsave(string Path, int SaveNumber)
        {
            this.Path = Path;
            this.SaveNumber = SaveNumber;
        }
    }
    public class Savestate
    {
        //Last state of a job that is being/has been executed
        public string? Name;
        public string? SourceFilePath;
        public string? TargetFilePath;
        public Workstate State;
        public double TotalFilesToCopy;
        public double TotalFilesSize;
        public double CurrentFile;
        public Savestate(string Name, double TotalFilesToCopy, double TotalFilesSize)
        {
            this.Name = Name; this.TotalFilesToCopy = TotalFilesToCopy; this.TotalFilesSize = TotalFilesSize;
        }
        public void AssignValues(string SourceFilePath, string TargetFilePath, Workstate State, double CurrentFile)
        {
            this.SourceFilePath = SourceFilePath; this.TargetFilePath = TargetFilePath; this.State = State; this.CurrentFile = CurrentFile;
        }
        public void AssignValues(string SourceFilePath, string TargetFilePath, Workstate State)
        {
            this.SourceFilePath = SourceFilePath; this.TargetFilePath = TargetFilePath; this.State = State;
        }
        public override string ToString()
        {
            if(Constants.Settings.LogFileType.Value == LogFileType.XML)
            {
                return $"\n\t<row>\n" +
                $"\t\t<Name>{Name}</Name>\n" +
                $"\t\t<SourceFilePath>{SourceFilePath}</SourceFilePath>\n" +
                $"\t\t<TargetFilePath>{TargetFilePath}</TargetFilePath>\n" +
                $"\t\t<State>{State}</State>\n" +
                $"\t\t<TotalFilesToCopy>{TotalFilesToCopy}</TotalFilesToCopy>\n" +
                $"\t\t<TotalFilesSize>{TotalFilesSize}</TotalFilesSize>\n" +
                $"\t\t<NbFilesLeftToDo>{TotalFilesToCopy - CurrentFile}</NbFilesLeftToDo>\n" +
                $"\t\t<Progression>{CurrentFile / TotalFilesToCopy * 100}%</Progression>\n" +
                $"\t</row>";
            }
            else
            {
                return $"\n{{\n" +
                $"\t\"Name\": \"{Name}\",\n" +
                $"\t\"SourceFilePath\": \"{SourceFilePath}\",\n" +
                $"\t\"TargetFilePath\": \"{TargetFilePath}\",\n" +
                $"\t\"State\": \"{State}\",\n" +
                $"\t\"TotalFilesToCopy\": \"{TotalFilesToCopy}\",\n" +
                $"\t\"TotalFilesSize\": \"{TotalFilesSize}\",\n" +
                $"\t\"NbFilesLeftToDo\": \"{TotalFilesToCopy - CurrentFile}\",\n" +
                $"\t\"Progression\": \"{CurrentFile / TotalFilesToCopy * 100}%\"\n" +
                $"}},";
            }
        }
    }
    public enum Savetype
    {
        Complete,
        Differential
    }
    public enum Workstate
    {
        ACTIVE,
        END,
        PAUSED_UNPRIORITIZED,
        PAUSED_FORBIDDENSOFTWARE,
        PAUSED_USER
    }
    public enum LogFileType
    {
        JSON,
        XML
    }
    public enum FilePriority
    {
        HIGH,
        NORMAL
    }
}
