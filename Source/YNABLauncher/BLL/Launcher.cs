namespace YNABLauncher.BLL
{
    using System;
    using System.Security;
    using System.Security.Cryptography;
    using DAL;

    public class Launcher : ILauncher
    {
        //////////////////////////////////////////////////////////////////
        #region Fields

        private readonly ISystemAccessor _systemAccessor;
        private readonly SecureString _password;

        #endregion

        //////////////////////////////////////////////////////////////////
        #region Constructors

        public Launcher(ISystemAccessor systemAccessor, SecureString password)
        {
            _systemAccessor = systemAccessor;
            _password = password;
        }

        #endregion
        
        //////////////////////////////////////////////////////////////////
        #region ILauncher Members

        void ILauncher.LaunchAndWait()
        {
            try
            {
                _systemAccessor.Decrypt(_password);
            }
            catch (CryptographicException)
            {
                Console.WriteLine("Invalid password. Press any key to exit...");
                Console.ReadKey();
                return;
            }
            _systemAccessor.UnpackZip();
            _systemAccessor.LaunchProcessAndWait();
            _systemAccessor.PackZip();
            _systemAccessor.Encrypt(_password);
        }

        #endregion
    }
}