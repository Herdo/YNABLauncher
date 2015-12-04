namespace YNABLauncher.DAL
{
    using System.Security;

    public interface ISystemAccessor
    {
        /// <summary>
        /// Launches the YNAB process and waits for it to exit.
        /// </summary>
        void LaunchProcessAndWait();

        /// <summary>
        /// Decrypts the encrypted ZIP file.
        /// </summary>
        /// <param name="password">The password used to decrypt the ZIP file.</param>
        void Decrypt(SecureString password);

        /// <summary>
        /// Encrypts the unencrypted ZIP file.
        /// </summary>
        /// <param name="password">The password used to encrypt the ZIP file.</param>
        void Encrypt(SecureString password);

        /// <summary>
        /// Unpacks the decrypted ZIP.
        /// </summary>
        void UnpackZip();

        /// <summary>
        /// Packs the ZIP file.
        /// </summary>
        void PackZip();
    }
}