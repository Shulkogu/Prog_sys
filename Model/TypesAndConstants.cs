using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public static class Constants
    {
        public static readonly string DateFormat = "dd-MM-yyyy";
        public static readonly string DateTimeFormat = "dd-MM-yyyy HH:mm:ss";
    }
    public struct Existingsave
    {
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
        public override string ToString()
        {
            return $"\"Name\": \"{Name}\",\n" +
                $"\"SourceFilePath\": \"{SourceFilePath}\",\n" +
                $"\"TargetFilePath\": \"{TargetFilePath}\",\n" +
                $"\"State\": \"{State}\",\n" +
                $"\"TotalFilesToCopy\": \"{TotalFilesToCopy}\",\n" +
                $"\"TotalFilesSize\": \"{TotalFilesSize}\",\n" +
                $"\"NbFilesLeftToDo\": \"{TotalFilesToCopy - CurrentFile}\",\n" +
                $"\"Progression\": \"{CurrentFile / TotalFilesToCopy * 100}%\"";
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
        END
    }
}
