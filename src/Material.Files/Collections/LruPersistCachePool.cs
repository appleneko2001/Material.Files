using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Material.Files.Collections
{
    public class LruPersistCachePool
    {
        private ulong _capacity;
        public ulong Capacity => _capacity;

        private string _rootPath;

        public LruPersistCachePool(string root, ulong capacity)
        {
            _rootPath = root;
            _capacity = capacity;
        }

        public bool TryGetCache(string key, out Stream stream)
        {
            var hashedPath = GetHashKey(key);
            var final = Path.Combine(_rootPath, hashedPath);

            Stream outStream = null;
            
            if (File.Exists(final))
                outStream = File.OpenRead(final);

            stream = outStream;

            if (outStream != null)
            {
                if (!outStream.CanRead)
                {
                    outStream.Close();
                    return false;
                }

                return true;
            }

            return false;
        }

        public bool PushOrReplace(string key, Stream stream)
        {
            var result = false;
            Stream input = null;
            try
            {
                var hashedPath = GetHashKey(key);
                var final = Path.Combine(_rootPath, hashedPath);

                var finalDir = Path.GetDirectoryName(final);

                if (!Directory.Exists(finalDir))
                    Directory.CreateDirectory(finalDir);
                
                if (File.Exists(final))
                    input = File.OpenWrite(final);
                else
                    input = File.Create(final);

                stream.CopyTo(input);
                result = true;
            }
            catch(Exception e)
            {
                
            }
            finally
            {
                input.Close();
                input.Dispose();
            }

            return result;
        }

        public bool IsCacheExist(string key)
        {
            return IsCacheExistCore(GetHashKey(key));
        }

        private bool IsCacheExistCore(string hash)
        {
            var final = Path.Combine(_rootPath, hash);
            return File.Exists(final);
        }

        public void Clear()
        {
            
        }
        
        private string GetHashKey(string path)
        {
            string hash = null;
            using (var shaHasher = new SHA256Managed())
            {
                var keyData = Encoding.UTF8.GetBytes(path);
                shaHasher.TransformFinalBlock(keyData, 0, keyData.Length);

                var shaHashBytes = shaHasher.Hash;
                hash = BitConverter.ToString(shaHashBytes).Replace("-", string.Empty);
            }

            return GetPath(hash);
        }
        
        /// <summary>
        /// Retrieves a fully qualified path used to store the cached value.
        /// </summary>
        /// <param name="hash">A hash of the contents of the cache.</param>
        /// <returns>A fully qualified path for a cached value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="hash"/> is <c>null</c>, empty, or whitespace.</exception>
        /// <exception cref="ArgumentException"><paramref name="hash"/> is not 64 characters long and does not contain only hexadecimal characters.</exception>
        protected virtual string GetPath(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentNullException(nameof(hash));
            if (hash.Length != 64)
                throw new ArgumentException("The hash must be a 32 character long representation of a 256-bit hash.", nameof(hash));
            var allValidChars = hash.All(IsValidHexChar);
            if (!allValidChars)
                throw new ArgumentException("The hash must be string containing only hexadecimal characters that represent a 256-bit hash", nameof(hash));

            var firstDir = hash.Substring(0, 2);
            var secondDir = hash.Substring(2, 2);

            return Path.Combine(firstDir, secondDir, hash);
        }
        
        /// <summary>
        /// Convenience method used to determine whether a character is a valid hexadecimal character.
        /// </summary>
        /// <param name="c">A unicode character.</param>
        /// <returns><c>true</c> if the value is a hexadecimal character; otherwise <c>false</c>.</returns>
        protected static bool IsValidHexChar(char c) => byte.TryParse(c.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var tmp);

        public Stream GetValue(string key)
        {
            var result = false;
            Stream input = null;
            
            var hashedPath = GetHashKey(key);
            var final = Path.Combine(_rootPath, hashedPath);

            if (File.Exists(final))
                input = File.OpenRead(final);
            else
                return null;

            return input;
        }
    }
}