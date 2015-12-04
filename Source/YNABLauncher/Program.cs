namespace YNABLauncher
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security;
    using BLL;
    using Data;
    using DAL;

    class Program
    {
        static void Main(string[] args)
        {
            string saltString = null;

            // Set defaults
            var defaultYnabInstallDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "YNAB 4");
            var defaultEncryptedZipPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OneDrive\\YNAB");
            var defaultDecryptedWorkingFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "YNAB");

            // Accept overwrites
            string ynabInstallDir = null;
            string encryptedZipPath = null;
            string decryptedWorkingFolderPath = null;
            
            foreach (var arg in args)
            {
                saltString = GetParameterForArgument(arg, "salt");
                if (saltString != null)
                    continue;
                ynabInstallDir = GetParameterForArgument(arg, "installDir");
                if (ynabInstallDir != null)
                    continue;
                encryptedZipPath = GetParameterForArgument(arg, "zipPath");
                if (encryptedZipPath != null)
                    continue;
                decryptedWorkingFolderPath = GetParameterForArgument(arg, "workingFolder");
            }

            if (saltString == null)
            {
                Console.WriteLine("Required salt is not present. Please add an salt with at least 8 bytes like the following:");
                Console.WriteLine("/salt=0,0,0,0,0,0,0,0");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            var salt = ParseSalt(saltString);
            if (salt.Length < 8)
            {
                Console.WriteLine("Required salt doesn't contain at least 8 bytes!");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            // Create arguments
            var launchArguments = new LaunchArguments(ynabInstallDir ?? defaultYnabInstallDirectory,
                                                      encryptedZipPath ?? defaultEncryptedZipPath,
                                                      decryptedWorkingFolderPath ?? defaultDecryptedWorkingFolderPath);

            // Get password
            var password = GetPassword();

            ISystemAccessor systemAccessor = new SystemAccessor(launchArguments, new CryptoEngine(salt));
            ILauncher launcher = new Launcher(systemAccessor, password);

            launcher.LaunchAndWait();
        }

        private static byte[] ParseSalt(string saltString)
        {
            var result = new List<byte>();
            var split = saltString.Split(',');
            foreach (var s in split)
            {
                byte parsed;
                if (byte.TryParse(s, out parsed))
                    result.Add(parsed);
            }
            return result.ToArray();
        }

        private static string GetParameterForArgument(string arg, string parameterKey)
        {
            if (!arg.StartsWith($"/{parameterKey}=")) return null;
            var param = arg.Split('=');
            return param.Length >= 2 ? param[1] : null;
        }

        private static SecureString GetPassword()
        {
            ConsoleKeyInfo key;
            var result = new SecureString();
            Console.Write("Enter your password: ");

            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    result.AppendChar(key.KeyChar);
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && result.Length > 0)
                    {
                        result.RemoveAt(result.Length - 1);
                        Console.Write("\b \b");
                    }
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);

            result.MakeReadOnly();
            Console.WriteLine();

            return result;
        }
    }
}
