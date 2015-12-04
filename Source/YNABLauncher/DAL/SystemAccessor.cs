namespace YNABLauncher.DAL
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Security;
    using System.Threading;
    using BLL;
    using Data;

    public class SystemAccessor : ISystemAccessor
    {
        //////////////////////////////////////////////////////////////////
        #region Fields

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
            _launchArguments = launchArguments;
            _cryptoEngine = cryptoEngine;
            _encryptedZipPath = Path.Combine(_launchArguments.EncryptedZipPath, "ynab.encrypted");
            _decryptedZipPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".zip");
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

            Console.WriteLine("Decrypting archive...");
            _cryptoEngine.Decrypt(password, _encryptedZipPath, _decryptedZipPath);
        }

        void ISystemAccessor.Encrypt(SecureString password)
        {
            Console.WriteLine("Encrypting archive...");
            _cryptoEngine.Encrypt(password, _decryptedZipPath, _encryptedZipPath);

            File.Delete(_decryptedZipPath);
        }

        void ISystemAccessor.UnpackZip()
        {
            if (!File.Exists(_decryptedZipPath)) return;

            Console.WriteLine("Unpacking archive...");
            if (!Directory.Exists(_launchArguments.DecryptedWorkingFolderPath))
                Directory.CreateDirectory(_launchArguments.DecryptedWorkingFolderPath);
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