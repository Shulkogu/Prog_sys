using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Model
{
    internal class File
    {
        private string SaveSubfolder;
        private Job Job;
        private string RelativeFilePath;
        private List<Existingsave>? ExistingSaves;
        public File(ref string SaveSubfolder, ref Job Job, string RelativeFilePath, [Optional] ref List<Existingsave> ExistingSaves)
        {
            this.SaveSubfolder = SaveSubfolder;
            this.Job = Job;
            this.RelativeFilePath = RelativeFilePath;
            if (ExistingSaves != null && ExistingSaves.Count > 0)
            {
                this.ExistingSaves = ExistingSaves;
            }
        }
        public (double?, double) Save(bool Encrypted)
        //This method will save the file if it is necessary (differential save) or if the savetype is complete
        //If a file was created, it returns its size, if it deliberatly wasn't, it returns null, if an error occured creating it, it returns -1
        {
            if (this.Job.SaveType == Savetype.Differential)
            {
                //If the savetype is differential, the newest saved version of the file will be searched for
                string? NewestVersion = CheckForExistingNewest();
                //If no version of the file is found (i.e, the returned value is null), the file will be copied, but if a version was found, it will be checked for differences with the current version
                if (NewestVersion == null || CheckForDifferences(NewestVersion))
                {
                    return Copy(Encrypted);
                }
                else
                {
                    return (null,0);
                }
            }
            else
            {
                return Copy(Encrypted);
            }
        }
        private (long,long) Copy(bool Encrypted)
        //This method copies a file from source to target (save folder). If the path for it doesn't exist, it will create it.
        //The new file's size will be returned. -1 Will be returned if an error occured
        {
            try
            {
                string TargetFilePath = this.Job.TargetPath + @"\\" + this.SaveSubfolder + @"\\" + RelativeFilePath;
                if (!Directory.Exists(Path.GetDirectoryName(TargetFilePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(TargetFilePath));
                }
                long Time = 0;
                if (Encrypted)
                {
                    try
                    {
                        Process CryptoSoftProcess = new Process();
                        CryptoSoftProcess.StartInfo.FileName = Constants.CryptoSoft;
                        CryptoSoftProcess.StartInfo.Arguments = $"\"{this.Job.SourcePath + @"\\" + RelativeFilePath}\" \"{TargetFilePath}\" \"SUPERSECRETKEY\"";
                        Time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        CryptoSoftProcess.Start();
                        while (!CryptoSoftProcess.HasExited)
                        {
                        }
                        Time = DateTimeOffset.Now.ToUnixTimeMilliseconds() - Time;
                    }
                    catch
                    {
                        Time = -1;
                    }
                }
                else
                {
                    System.IO.File.Copy(this.Job.SourcePath + @"\\" + RelativeFilePath, TargetFilePath, true);
                }
                long Size = new FileInfo(TargetFilePath).Length;
                return (Size,Time);
            }
            catch
            {
                return (-1,0);
            }
        }
        private string? CheckForExistingNewest()
        //This method is used for differential saves, it searches for the existence of a file from the newest to the oldest revision of the save.
        //The path of the first version that is found (i.e: the newest revision) will be returned. If no file is found, null will be returned
        {
            if (ExistingSaves != null && ExistingSaves.Count > 0)
            {
                foreach (Existingsave Save in ExistingSaves)
                {
                    if (System.IO.File.Exists(Save.Path + @"\\" + RelativeFilePath))
                    {
                        return Save.Path + @"\\" + RelativeFilePath;
                    }
                }
            }
            return null;
        }
        private bool CheckForDifferences(string ComparedFilePath)
        //This method is used for differential saves, it compares the object's file to a file whose path is given in parameter
        //If the files are different, true is returned, false if they're not
        {
            FileStream ObjectFile = new(this.Job.SourcePath + @"\\" + RelativeFilePath, FileMode.Open);
            FileStream ComparedFile = new(ComparedFilePath, FileMode.Open);
            //First, file lengths are compared because it's the least time costly comparison method
            if (ObjectFile.Length != ComparedFile.Length)
            {
                //If lengths are different, we know files aren't equal, therefore files are closed and true is returned
                ObjectFile.Close();
                ComparedFile.Close();
                return true;
            }
            //If lengths are equal, byte per byte comparison proceeds
            int ObjectFileByte;
            int ComparedFileByte;
            //Byte are read while they are equal and the object's file still has bytes to test
            do
            {
                ObjectFileByte = ObjectFile.ReadByte();
                ComparedFileByte = ComparedFile.ReadByte();
            }
            while (ObjectFileByte == ComparedFileByte && ObjectFileByte != -1);
            //Once the loop closes, files are closed and the result of the last comparison is returned
            ObjectFile.Close();
            ComparedFile.Close();
            return ObjectFileByte - ComparedFileByte != 0;
        }
    }
}
