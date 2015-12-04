namespace YNABLauncher.Data
{
    public struct LaunchArguments
    {
        //////////////////////////////////////////////////////////////////
        #region Properties

        public string YnabInstallDirectory { get; }

        public string EncryptedZipPath { get; }

        public string DecryptedWorkingFolderPath { get; }

        #endregion

        //////////////////////////////////////////////////////////////////
        #region Constructors

        public LaunchArguments(string ynabInstallDirectory,
                               string encryptedZipPath,
                               string decryptedWorkingFolderPath)
        {
            YnabInstallDirectory = ynabInstallDirectory;
            EncryptedZipPath = encryptedZipPath;
            DecryptedWorkingFolderPath = decryptedWorkingFolderPath;
        }

        #endregion
    }
}