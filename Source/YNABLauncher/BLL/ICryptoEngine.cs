namespace YNABLauncher.BLL
{
    using System.Security;

    public interface ICryptoEngine
    {
        void Decrypt(SecureString password, string inputFile, string outputFile);
        void Encrypt(SecureString password, string inputFile, string outputFile);
    }
}