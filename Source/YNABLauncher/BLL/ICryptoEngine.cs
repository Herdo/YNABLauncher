namespace YNABLauncher.BLL
{
    using System;
    using System.Security;
    using System.Text;
    using Annotations;

    public interface ICryptoEngine
    {
        /// <summary>
        /// Encrypts the <paramref name="input"/> using the <paramref name="password"/> and returns the encrypted result.
        /// </summary>
        /// <param name="password">The password used for encryption.</param>
        /// <param name="input">The input to encrypt.</param>
        /// <param name="encoding"><see cref="Encoding"/> used for byte operations.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/>, <paramref name="input"/> or <paramref name="encoding"/> are null.</exception>
        /// <returns>The encrypted content.</returns>
        byte[] Encrypt([NotNull] SecureString password, [NotNull] byte[] input, [NotNull] Encoding encoding);

        /// <summary>
        /// Decrypts the <paramref name="input"/> using the <paramref name="password"/> and returns the decrypted result.
        /// </summary>
        /// <param name="password">The password used for decryption.</param>
        /// <param name="input">The input to decrypt.</param>
        /// <param name="encoding"><see cref="Encoding"/> used for byte operations.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/>, <paramref name="input"/> or <paramref name="encoding"/> are null.</exception>
        /// <returns>The decrypted content.</returns>
        byte[] Decrypt([NotNull] SecureString password, [NotNull] byte[] input, [NotNull] Encoding encoding);
    }
}