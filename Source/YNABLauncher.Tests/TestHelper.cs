namespace YNABLauncher.Tests
{
    using System.Security;

    public static class TestHelper
    {
        public static SecureString CreateSecureString(string input)
        {
            var result = new SecureString();

            foreach (var c in input)
                result.AppendChar(c);

            return result;
        }
    }
}