using System.Diagnostics.Metrics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace CryptoSoft
{
    class CryptoSoft
    {
        private static uint DefaultChunkSize = 20000000;
        [STAThread]
        static int Main(string[] args)
        {
            //There should at least be 3 given arguments : a target path, a destination path (which have to be different), and a key (passed as a string). The key should be at least 4 characters, as 1 character is 16 bits and the minimum is 64 bits
            //Order:
            //Optional: -d, which stands for differential, and an encrypted file to compare against
            //Required arguments
            //Optional: a chunk size can be given, the default one is DefaultChunkSize
            //Exit codes: 0, execution ended | 1, encrypted file was identical to compared file | 2, wrong arguments
            if ((args.Length == 5 || args.Length == 6) && args[0] == "-d" && args[2] != args[3] && File.Exists(args[2]) && File.Exists(args[1]) && args[4].Length >= 4)
            {
                byte[] KeyBytes = Encoding.Default.GetBytes(args[4]);
                return ApplyXOR(args[2], args[3], args[1], KeyBytes, ((args.Length == 6) ? Convert.ToUInt32(args[5]) : DefaultChunkSize));
            }
            else if ((args.Length == 3 || args.Length == 4) && args[0] != args[1] && File.Exists(args[0]) && args[2].Length >= 4)
            {
                byte[] KeyBytes = Encoding.Default.GetBytes(args[2]);
                ApplyXOR(args[0], args[1], KeyBytes, ((args.Length == 4) ? Convert.ToUInt32(args[3]) : DefaultChunkSize));
                return 0;
            }
            else if (args.Length == 1 && args[0].ToUpper() == "HELP")
            {
                Console.WriteLine($"Three arguments are necessary, others are optional:\n" +
                    $"1.[optional] -d (the resulting encrypted file will be compared against another already encrypted file and only kept if different)\n" +
                    $"2.[required if -d] Existing copy path (path of the already encrypted file to compare against)\n" +
                    $"3.[necessary] Source file path (should exist)\n" +
                    $"4.[necessary] Target file path (should be different than source file path. If the directory doesn't exist, it will be created)\n" +
                    $"5.[necessary] Key (passed as a string, one character being 16bits) of at least 64bits\n" +
                    $"6.[optional] Chunk size (when encrypted, the file is divided in chunks of X bytes), default: {DefaultChunkSize}\n" +
                    $"Example: CryptoSoft.exe \"C:\\Users\\Leo\\Desktop\\Tests\\image.png\" \"C:\\Users\\Leo\\Desktop\\Tests\\Encrypted Files\\image.png\" \"sUp34S4ECR@Tk4y\" 20000");
                return 0;
            }
            else
            {
                Console.WriteLine("Wrong or no arguments given. For help: CryptoSoft.exe help");
                return 1;
            }
        }
        private static void ApplyXOR(string SourceFilePath, string DestinationFilePath, byte[] Key, uint ChunkSize)
        {
            //This function copies a file from source to target, and encrypts it along the way, so that the destination path never sees the unencrypted file
            //If the destination path doesn't exist, it's created
            if (!Directory.Exists(Path.GetDirectoryName(DestinationFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(DestinationFilePath));
            }
            //Files are opened
            FileStream SourceFile = new(SourceFilePath, FileMode.Open);
            FileStream DestinationFile = new(DestinationFilePath, FileMode.Create);
            long Remaining = SourceFile.Length;
            long i = 0;
            //While there are still bytes to copy from source to target
            while (Remaining > 0)
            {
                //The size of the next chunk of bytes to copy is ChunkSize, unless less bytes than ChunkSize are remaining
                if (Remaining - ChunkSize < 0)
                {
                    ChunkSize = Convert.ToUInt32(Remaining);
                }
                byte[] Chunk = new byte[ChunkSize];
                //The byte of chunk is read from the source file
                SourceFile.ReadExactly(Chunk, 0, Chunk.Length);
                for (int Byte = 0; Byte < ChunkSize; Byte++)
                {
                    //Each byte gets XOR applied according to the Key
                    Chunk[Byte] = Convert.ToByte((Chunk[Byte] ^ Key[i % (Key.Length)]));
                    i++;
                }
                //The encrypted chunk is written to the destination file
                DestinationFile.Write(Chunk);
                DestinationFile.Flush();
                Remaining -= ChunkSize;
            }
            //When finished, files are closed
            SourceFile.Close();
            DestinationFile.Flush();
            DestinationFile.Close(); DestinationFile.Dispose();
        }
        private static int ApplyXOR(string SourceFilePath, string DestinationFilePath, string ExistingCopyPath, byte[] Key, uint ChunkSize)
        {
            //This function also copies a file from source to target, but this time it compares the file that it's creating to an already created encrypted file. If the files are identical, the newly made file isn't kept.
            //If the source path doesn't exist, it's once again created, unless this time, the function keeps track of the top folder it had to create.
            //For example : if the directory target directory is "C:\Users\Leo\Desktop\Tests\2\3\4\encrypted.file" and only "C:\Users\Leo\Desktop\Tests" exists, "\2\3\4" will be created, and each one folder can be deleted if the file isn't different
            string TopDirectoryToCreate = "";
            if (!Directory.Exists(Path.GetDirectoryName(DestinationFilePath)))
            {
                TopDirectoryToCreate = Path.GetDirectoryName(DestinationFilePath);
                while (!Directory.Exists(Path.GetDirectoryName(TopDirectoryToCreate)) && TopDirectoryToCreate != null)
                {
                    TopDirectoryToCreate = Path.GetDirectoryName(TopDirectoryToCreate);
                }
            }
            //If TopDirectoryToCreate is null, it means that the drive letter/network location doesn't exist
            if (TopDirectoryToCreate == null)
            {
                return 1;
            }
            //Else, if the top directory to create is different than an empty string, it means the directory has to be created
            else if(TopDirectoryToCreate != "")
            {
                Directory.CreateDirectory(Path.GetDirectoryName(DestinationFilePath));
            }
            //Files are opened
            FileStream SourceFile = new(SourceFilePath, FileMode.Open);
            FileStream ExistingCopy = new(ExistingCopyPath, FileMode.Open);
            FileStream DestinationFile = new(DestinationFilePath, FileMode.Create);
            long Remaining = SourceFile.Length;
            long i = 0;
            bool FoundDifferences = false; //This bool keeps track of whether any differences were found or not
            //While there are still bytes to copy from source to target
            while (Remaining > 0)
            {
                //The size of the next chunk of bytes to copy is ChunkSize, unless less bytes than ChunkSize are remaining
                if (Remaining - ChunkSize < 0)
                {
                    ChunkSize = Convert.ToUInt32(Remaining);
                }
                byte[] Chunk = new byte[ChunkSize];
                //The byte of chunk is read from the source file
                SourceFile.ReadExactly(Chunk, 0, Chunk.Length);
                for (int Byte = 0; Byte < ChunkSize; Byte++)
                {
                    //Each byte gets XOR applied according to the Key
                    Chunk[Byte] = Convert.ToByte((Chunk[Byte] ^ Key[i % (Key.Length)]));
                    i++;
                }
                if (!FoundDifferences) //While there were no differences found, the bytes from the existing copy are read and compared to the newly created bytes
                {
                    byte[] ExistingChunk = new byte[ChunkSize];
                    ExistingCopy.ReadExactly(ExistingChunk, 0, Chunk.Length);
                    FoundDifferences = !ExistingChunk.AsSpan().SequenceEqual(Chunk);
                }
                //The encrypted chunk is written to the destination file
                DestinationFile.Write(Chunk);
                DestinationFile.Flush();
                Remaining -= ChunkSize;
            }
            //When finished, files are closed
            SourceFile.Close();
            ExistingCopy.Close();
            DestinationFile.Flush();
            DestinationFile.Close(); DestinationFile.Dispose();
            if (!FoundDifferences) //If no differences were found during the process of copying and encrypting the file,
            {
                File.Delete(DestinationFilePath); //The copied file is deleted
                //Then, if a folder/some folders were created, they are deleted
                if (TopDirectoryToCreate != "")
                {
                    try
                    {
                        string DeletedDirectory = Path.GetDirectoryName(DestinationFilePath);
                        Directory.Delete(DeletedDirectory);
                        while (DeletedDirectory != TopDirectoryToCreate)
                        {
                            DeletedDirectory = Path.GetDirectoryName(DeletedDirectory);
                            Directory.Delete(DeletedDirectory);
                        }
                        //Note: this loop inside a try block is used instead of a simple Delete(TopDirectoryToCreate,true) with a "true" argument that would state that the folder should be recursively deleted, just in case any other file/folder was created within those folders during the process of copying/encrypting the file
                    }
                    catch
                    {
                    }
                }
                return 1;
            }
            return 0;
        }
    }
}