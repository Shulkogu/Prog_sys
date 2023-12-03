using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Model;

class Program
{
    static void Main(string[] args)
    {
        Logger Logger = new Logger(@"C:\\PathToLogFile.json", @"C:\\PathToStateFile.json");
        Logger.FinalizeLogs();
        Saver saver = new Saver("Test", @"C:\\SourcePath", @"C:\\TargetPath", Savetype.Differential, ref Logger);
        saver.SaveFiles();
        Logger.FinalizeLogs();
    }
}