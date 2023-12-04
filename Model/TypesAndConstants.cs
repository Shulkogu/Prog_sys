using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public static readonly string JobsFile = @"jobs.json";
        public static readonly string LogPath = @"logs\\files\\";
        public static readonly string StatePath = @"logs\\state\\";
        public static readonly string SingleToDoubleBackslashRegex = @"(?<!\\)\\{1}(?!\\)|\\{3,}";
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
