namespace YNABLauncher.DAL
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Security;
    using System.Text;
    using System.Threading;
    using BLL;
    using Data;

    public class SystemAccessor : ISystemAccessor
    {
        //////////////////////////////////////////////////////////////////
        #region Fields

        private readonly Encoding _encoding;
        private readonly LaunchArguments _launchArguments;
        private readonly ICryptoEngine _cryptoEngine;
        private readonly string _encryptedZipPath;
        private readonly string _decryptedZipPath;

        #endregion

        //////////////////////////////////////////////////////////////////
        #region Constructors
            
        public SystemAccessor(LaunchArguments launchArguments,
                              ICryptoEngine cryptoEngine)
        {
            _encoding = Encoding.GetEncoding(1252);
            _launchArguments = launchArguments;
            _cryptoEngine = cryptoEngine;
            _encryptedZipPath = Path.Combine(_launchArguments.EncryptedZipPath, "ynab.encrypted");
            _decryptedZipPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".decrypted");
            Directory.SetCurrentDirectory(_launchArguments.YnabInstallDirectory);
        }

        #endregion

        //////////////////////////////////////////////////////////////////
        #region Private Methods

        private void CleanWorkingDirectoryRecursive(DirectoryInfo directoryToDelete)
        {
            if (!directoryToDelete.Exists) return;
            
            foreach (var subDir in directoryToDelete.EnumerateDirectories())
            {
                CleanWorkingDirectoryRecursive(subDir);
            }

            foreach (var fileInfo in directoryToDelete.EnumerateFiles())
            {
                // If the file info is flagged as read only, remove the flag
                RemoveReadOnlyFlag(fileInfo);
                fileInfo.Delete();
            }

            // If the working dir is flagged as read only, remove the flag
            RemoveReadOnlyFlag(directoryToDelete);
            directoryToDelete.Delete(true);
        }

        private static void RemoveReadOnlyFlag(FileSystemInfo info)
        {
            if (info.Attributes.HasFlag(FileAttributes.ReadOnly))
                info.Attributes &= ~FileAttributes.ReadOnly;
        }

        private byte[] ReadInputBytes(string inputFilename)
        {
            var input = File.ReadAllText(inputFilename, _encoding);
            return _encoding.GetBytes(input);
        }

        private void WriteOuputBytes(string outputFilename, byte[] bytes)
        {
            var output = _encoding.GetString(bytes);
            File.WriteAllText(outputFilename, output, _encoding);
        }

        #endregion

        //////////////////////////////////////////////////////////////////
        #region ISystemAccessor Members

        void ISystemAccessor.LaunchProcessAndWait()
        {
            var startInfo = new ProcessStartInfo("YNAB 4.exe");
            var process = new Process
            {
                StartInfo = startInfo
            };
            Console.WriteLine("Starting YNAB...");
            process.Start();
            process.WaitForExit();
        }

        void ISystemAccessor.Decrypt(SecureString password)
        {
            if (!File.Exists(_encryptedZipPath)) return;

            Console.WriteLine("Reading encrypted archive...");
            var input = ReadInputBytes(_encryptedZipPath);

            Console.WriteLine("Decrypting archive...");
            var decrypted = _cryptoEngine.Decrypt(password, input, _encoding);

            Console.WriteLine("Writing decrypted archive...");
            WriteOuputBytes(_decryptedZipPath, decrypted);
        }

        void ISystemAccessor.Encrypt(SecureString password)
        {
            Console.WriteLine("Reading decrypted archive...");
            var input = ReadInputBytes(_decryptedZipPath);

            Console.WriteLine("Encrypting archive...");
            var encrypted = _cryptoEngine.Encrypt(password, input, _encoding);

            Console.WriteLine("Writing encrypted archive...");
            WriteOuputBytes(_encryptedZipPath, encrypted);

            File.Delete(_decryptedZipPath);
        }

        void ISystemAccessor.UnpackZip()
        {
            if (!File.Exists(_decryptedZipPath)) return;

            Console.WriteLine("Unpacking archive...");
            if (Directory.Exists(_launchArguments.DecryptedWorkingFolderPath))
                Directory.Delete(_launchArguments.DecryptedWorkingFolderPath, true);
            ZipFile.ExtractToDirectory(_decryptedZipPath, _launchArguments.DecryptedWorkingFolderPath);

            File.Delete(_decryptedZipPath);
        }

        void ISystemAccessor.PackZip()
        {
            Console.WriteLine("Packing archive...");
            ZipFile.CreateFromDirectory(_launchArguments.DecryptedWorkingFolderPath, _decryptedZipPath, CompressionLevel.Fastest, false);

            Console.WriteLine("Cleaning working directory...");
            
            while (Directory.Exists(_launchArguments.DecryptedWorkingFolderPath))
            {
                try
                {
                    var workingDir = new DirectoryInfo(_launchArguments.DecryptedWorkingFolderPath);
                    CleanWorkingDirectoryRecursive(workingDir);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Error removing working directory. Retrying soon...");
                    Thread.Sleep(500);
                }
                catch (IOException)
                {
                    Console.WriteLine("Error removing working directory. Retrying soon...");
                    Thread.Sleep(500);
                }
            }

            Console.WriteLine("Successfully deleted working directory.");
        }

        #endregion
    }
}