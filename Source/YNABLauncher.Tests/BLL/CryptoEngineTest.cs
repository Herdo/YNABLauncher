// ReSharper disable AssignNullToNotNullAttribute
namespace YNABLauncher.Tests.BLL
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Security;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Telerik.JustMock;
    using YNABLauncher.BLL;

    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CryptoEngineTest
    {
        //////////////////////////////////////////////////////////////////
        #region Test Methods

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_SaltNull()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            new CryptoEngine(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_SaltToShort()
        {
            // Act
            // ReSharper disable once ObjectCreationAsStatement
            new CryptoEngine(new byte[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Encrypt_PasswordNull()
        {
            // Arrange
            ICryptoEngine cryptoEngine = new CryptoEngine(new byte[8]);

            // Act
            cryptoEngine.Encrypt(null, new byte[0], Mock.Create<Encoding>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Encrypt_InputNull()
        {
            // Arrange
            ICryptoEngine cryptoEngine = new CryptoEngine(new byte[8]);

            // Act
            cryptoEngine.Encrypt(new SecureString(), null, Mock.Create<Encoding>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Encrypt_EncodingNull()
        {
            // Arrange
            ICryptoEngine cryptoEngine = new CryptoEngine(new byte[8]);

            // Act
            cryptoEngine.Encrypt(new SecureString(), new byte[0], null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Decrypt_PasswordNull()
        {
            // Arrange
            ICryptoEngine cryptoEngine = new CryptoEngine(new byte[8]);

            // Act
            cryptoEngine.Decrypt(null, new byte[0], Mock.Create<Encoding>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Decrypt_InputNull()
        {
            // Arrange
            ICryptoEngine cryptoEngine = new CryptoEngine(new byte[8]);

            // Act
            cryptoEngine.Decrypt(new SecureString(), null, Mock.Create<Encoding>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Decrypt_EncodingNull()
        {
            // Arrange
            ICryptoEngine cryptoEngine = new CryptoEngine(new byte[8]);

            // Act
            cryptoEngine.Decrypt(new SecureString(), new byte[0], null);
        }

        [TestMethod]
        public void EncryptionDecryptionCorrectness()
        {
            // Arrange
            var encoding = Encoding.GetEncoding(1252);
            var salt = new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 30, 74, 12, 234, 165, 140, 253};
            var input = encoding.GetBytes("Input string to encrypt");
            var password = TestHelper.CreateSecureString("very secure password 1234567890?!\"§$%&()=?");
            ICryptoEngine cryptoEngine = new CryptoEngine(salt);

            // Act
            var encrypted = cryptoEngine.Encrypt(password, input, encoding);
            var decrypted = cryptoEngine.Decrypt(password, encrypted, encoding);

            // Assert
            CollectionAssert.AreNotEqual(input, encrypted);
            CollectionAssert.AreEqual(input, decrypted);
        }

        #endregion
    }
}
