using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DucMinh
{
    public class EncryptionStorage : IDataStorage
    {
        private readonly IDataStorage _baseStorage;
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionStorage(IDataStorage baseStorage, string encryptionKey)
        {
            _baseStorage = baseStorage ?? throw new ArgumentNullException(nameof(baseStorage));

            // Tạo key và IV từ encryptionKey
            using var sha256 = SHA256.Create();
            byte[] keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
            _key = new byte[32];
            _iv = new byte[16];
            Array.Copy(keyBytes, _key, 32);
            Array.Copy(keyBytes, 0, _iv, 0, 16);
        }

        private string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            using Aes aes = Aes.Create();

            aes.Key = _key;
            aes.IV = _iv;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        private string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                using Aes aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using var ms = new MemoryStream(cipherBytes);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }

        #region Set

        public void SetInt(string key, int value)
        {
            _baseStorage.SetString(key, Encrypt(value.ToString()));
        }

        public void SetFloat(string key, float value)
        {
            _baseStorage.SetString(key, Encrypt(value.ToString(CultureInfo.InvariantCulture)));
        }

        public void SetString(string key, string value)
        {
            _baseStorage.SetString(key, Encrypt(value));
        }

        public void SetBool(string key, bool value)
        {
            _baseStorage.SetString(key, Encrypt(value.ToString()));
        }

        public void SetDateTime(string key, DateTime value)
        {
            // var timestamp = TimeHelper.ToUnixTimestamp(value).ToString();
            // _baseStorage.SetString(key, Encrypt(timestamp));
        }

        #endregion

        #region Get

        public int GetInt(string key, int defaultValue = 0)
        {
            var encrypted = _baseStorage.GetString(key);
            var decrypted = Decrypt(encrypted);
            return int.TryParse(decrypted, out var result) ? result : defaultValue;
        }

        public float GetFloat(string key, float defaultValue = 0)
        {
            var encrypted = _baseStorage.GetString(key);
            var decrypted = Decrypt(encrypted);
            return float.TryParse(decrypted, NumberStyles.Any, CultureInfo.CurrentCulture, out var result) ? result : defaultValue;
        }

        public string GetString(string key, string defaultValue = null)
        {
            var encrypted = _baseStorage.GetString(key);
            var decrypted = Decrypt(encrypted);
            return decrypted ?? defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            var encrypted = _baseStorage.GetString(key);
            var decrypted = Decrypt(encrypted);
            if (bool.TryParse(decrypted, out var result))
            {
                return result;
            }
            if (int.TryParse(decrypted, out int resultInt))
            {
                return resultInt == 1;
            }
            return defaultValue;
        }

        public DateTime GetDateTime(string key, DateTime defaultValue = default)
        {
            var encrypted = _baseStorage.GetString(key);
            var decrypted = Decrypt(encrypted);
            return default;
            // return long.TryParse(decrypted, out var result) ? TimeHelper.FromUnixTimestamp(result) : defaultValue;
            
        }

        #endregion

        public bool HasKey(string key)
        {
            return _baseStorage.HasKey(key);
        }

        public void Delete(string key)
        {
            _baseStorage.Delete(key);
        }

        public void DeleteAll()
        {
            _baseStorage.DeleteAll();
        }

        public List<string> GetAllKeys()
        {
            return _baseStorage.GetAllKeys();
        }
    }
}