namespace YNABLauncher.BLL
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;

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

        private byte[] EncryptFile(string password,
                                   byte[] input,
                                   Encoding encoding)
        {
            byte[] result;

            using (var aes = InitializeAes(password))
            {
                // Create a encryptor to perform the stream transform.
                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new BinaryWriter(csEncrypt, encoding))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(input);
                        }
                        result = msEncrypt.ToArray();
                    }
                }
            }

            return result;
        }

        private byte[] DecryptFile(string password,
                                   byte[] input,
                                   Encoding encoding)
        {
            byte[] result;

            using (var aes = InitializeAes(password))
            {
                // Create a decrytor to perform the stream transform.
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                // Create the streams used for decryption.
                using (var msDecrypt = new MemoryStream(input))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt, encoding))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in the result.
                            result = encoding.GetBytes(srDecrypt.ReadToEnd());
                        }
                    }
                }
            }
            
            return result;
        }

        private AesCryptoServiceProvider InitializeAes(string password)
        {
            var aes = new AesCryptoServiceProvider();
            byte[] key;
            byte[] iv;
            GetKeyAndIvFromPasswordAndSalt(password, _salt, aes, out key, out iv);
            aes.Key = key;
            aes.IV = iv;
            return aes;
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

        private static byte[] ExecuteSecuredFunction(SecureString password, Func<string, byte[]> func)
        {
            byte[] result = null;

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
                        result = func(insecurePassword);
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

            return result;
        }

        #endregion

        //////////////////////////////////////////////////////////////////
        #region ICryptoEngine Members

        byte[] ICryptoEngine.Encrypt(SecureString password, byte[] input, Encoding encoding)
        {
            return ExecuteSecuredFunction(password, pw => EncryptFile(pw, input, encoding));
        }

        byte[] ICryptoEngine.Decrypt(SecureString password, byte[] input, Encoding encoding)
        {
            return ExecuteSecuredFunction(password, pw => DecryptFile(pw, input, encoding));
        }

        #endregion
    }
}