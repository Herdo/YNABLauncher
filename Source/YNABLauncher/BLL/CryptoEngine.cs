namespace YNABLauncher.BLL
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;

    public class CryptoEngine : ICryptoEngine
    {
        //////////////////////////////////////////////////////////////////
        #region Fields
            
        private readonly byte[] _salt;

        #endregion

        //////////////////////////////////////////////////////////////////
        #region Constructors

        public CryptoEngine(byte[] salt)
        {
            _salt = salt;
        }

        #endregion

        //////////////////////////////////////////////////////////////////
        #region Private Methods

        private void EncryptFile(string inputFilename,
                                 string outputFilename,
                                 string password)
        {
            var fsInput = new FileStream(inputFilename,
                                         FileMode.Open,
                                         FileAccess.Read);

            var fsEncrypted = new FileStream(outputFilename,
                                             FileMode.Create,
                                             FileAccess.Write);

            var aes = new AesCryptoServiceProvider();
            byte[] key;
            byte[] iv;
            GetKeyAndIvFromPasswordAndSalt(password, _salt, aes, out key, out iv);
            aes.Key = key;
            aes.IV = iv;

            var aesEncrypt = aes.CreateEncryptor();
            var cryptostream = new CryptoStream(fsEncrypted,
               aesEncrypt,
               CryptoStreamMode.Write);

            var bytearrayinput = new byte[fsInput.Length];
            fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);
            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);

            // Cleanup
            cryptostream.Close();
            fsInput.Close();
            fsEncrypted.Close();
        }

        private void DecryptFile(string inputFilename,
                                 string outputFilename,
                                 string password)
        {
            var aes = new AesCryptoServiceProvider();
            byte[] key;
            byte[] iv;
            GetKeyAndIvFromPasswordAndSalt(password, _salt, aes, out key, out iv);
            aes.Key = key;
            aes.IV = iv;

            var fsread = new FileStream(inputFilename,
               FileMode.Open,
               FileAccess.Read);
            
            var aesDecrypt = aes.CreateDecryptor();
            var cryptostreamDecr = new CryptoStream(fsread,
                                                    aesDecrypt,
                                                    CryptoStreamMode.Read);
            
            var fsDecrypted = new StreamWriter(outputFilename);
            fsDecrypted.Write(new StreamReader(cryptostreamDecr).ReadToEnd());

            // Cleanup
            fsDecrypted.Flush();
            fsDecrypted.Close();
        }

        private static void GetKeyAndIvFromPasswordAndSalt(string password,
                                                           byte[] salt,
                                                           SymmetricAlgorithm symmetricAlgorithm,
                                                           out byte[] key,
                                                           out byte[] iv)
        {
            var db = new Rfc2898DeriveBytes(password, salt);
            key = db.GetBytes(symmetricAlgorithm.KeySize / 8);
            iv = db.GetBytes(symmetricAlgorithm.BlockSize / 8);
        }

        private static void ExecuteSecuredAction(SecureString password, Action<string> action)
        {
            unsafe
            {
                var length = password.Length;
                var insecurePassword = new string('\0', length);

                var gch = new GCHandle();
                RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(
                    delegate
                    {
                        RuntimeHelpers.PrepareConstrainedRegions();
                        try { }
                        finally
                        {
                            gch = GCHandle.Alloc(insecurePassword, GCHandleType.Pinned);
                        }

                        var passwordPtr = IntPtr.Zero;
                        RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(
                            delegate
                            {
                                RuntimeHelpers.PrepareConstrainedRegions();
                                try { }
                                finally
                                {
                                    passwordPtr = Marshal.SecureStringToBSTR(password);
                                }

                                var pPassword = (char*)passwordPtr;
                                var pInsecurePassword = (char*)gch.AddrOfPinnedObject();
                                for (int index = 0; index < length; index++)
                                {
                                    pInsecurePassword[index] = pPassword[index];
                                }
                            },
                            delegate
                            {
                                if (passwordPtr != IntPtr.Zero)
                                {
                                    Marshal.ZeroFreeBSTR(passwordPtr);
                                }
                            },
                            null);

                        // Use the password.
                        action(insecurePassword);
                    },
                    delegate
                    {
                        if (gch.IsAllocated)
                        {
                            // Zero the string.
                            var pInsecurePassword = (char*)gch.AddrOfPinnedObject();
                            for (var index = 0; index < length; index++)
                            {
                                pInsecurePassword[index] = '\0';
                            }

                            gch.Free();
                        }
                    },
                    null);
            }
        }

        #endregion

        //////////////////////////////////////////////////////////////////
        #region ICryptoEngine Members

        void ICryptoEngine.Decrypt(SecureString password, string inputFile, string outputFile)
        {
            if (!File.Exists(inputFile)) return;
            ExecuteSecuredAction(password, pw => DecryptFile(inputFile, outputFile, pw));
        }

        void ICryptoEngine.Encrypt(SecureString password, string inputFile, string outputFile)
        {
            if (!File.Exists(inputFile)) return;
            ExecuteSecuredAction(password, pw => EncryptFile(inputFile, outputFile, pw));
        }

        #endregion
    }
}