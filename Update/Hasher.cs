using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Update
{
    class Hasher
    {
        public static string HashFile(string path)
        {
            if (File.Exists(path))
            {
                using (FileStream file = File.OpenRead(path))
                {
                    using (SHA256 hasher = SHA256.Create())
                    {
                        byte[] bytes = hasher.ComputeHash(file);

                        StringBuilder hashBuilder = new StringBuilder();
                        foreach (byte ch in bytes)
                        {
                            hashBuilder.Append(ch.ToString("x2"));
                        }
                        return hashBuilder.ToString();
                    }
                }
            } else
            {
                return "";
            }
        }
    }
}
