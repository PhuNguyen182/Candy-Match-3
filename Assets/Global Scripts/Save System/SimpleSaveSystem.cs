using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.Encryption;
using Newtonsoft.Json;

namespace GlobalScripts.SaveSystem
{
    public static class SimpleSaveSystem<T>
    {
        public static T LoadData(string name)
        {
            string dataPath = Path.Combine(Application.persistentDataPath, $"{name}.dat");

            if (File.Exists(dataPath))
            {
                T data;
                using (StreamReader streamReader = new(dataPath))
                {
                    string json = streamReader.ReadToEnd();
                    using (StringReader stringReader = new(json))
                    {
                        using (JsonReader jsonReader = new JsonTextReader(stringReader))
                        {
                            JsonSerializer jsonSerializer = new();
                            data = jsonSerializer.Deserialize<T>(jsonReader);
                        }

                        stringReader.Close();
                    }

                    streamReader.Close();
                }

                return data;
            }

            return default;
        }

        public static T LoadDataWithEncryption(string name)
        {
            string dataPath = Path.Combine(Application.persistentDataPath, $"{name}.dat");

            if (File.Exists(dataPath))
            {
                T data;
                using (StreamReader streamReader = new(dataPath))
                {
                    string cipheredJson = streamReader.ReadToEnd();
                    double cipheredValue = double.Parse(cipheredJson);
                    byte[] cipheredArray = BitConverter.GetBytes(cipheredValue);
                    string decryptedJson = AesEncryptor.Decrypt(cipheredArray);

                    using (StringReader stringReader = new(decryptedJson))
                    {
                        using (JsonReader jsonReader = new JsonTextReader(stringReader))
                        {
                            JsonSerializer jsonSerializer = new();
                            data = jsonSerializer.Deserialize<T>(jsonReader);
                        }

                        stringReader.Close();
                    }

                    streamReader.Close();
                }

                return data;
            }

            return default;
        }

        public static void SaveData(string name, T data)
        {
            string dataPath = Path.Combine(Application.persistentDataPath, $"{name}.dat");

            using (StreamWriter writer = new(dataPath))
            {
                JsonSerializerSettings settings = new()
                {
                    Formatting = Formatting.Indented,
                };

                string json = JsonConvert.SerializeObject(data, settings);
                writer.Write(json);
                writer.Close();
            }
        }

        public static void SaveDataWithEncryption(string name, T data)
        {
            string dataPath = Path.Combine(Application.persistentDataPath, $"{name}.dat");

            using (StreamWriter writer = new(dataPath))
            {
                JsonSerializerSettings settings = new()
                {
                    Formatting = Formatting.Indented,
                };

                string json = JsonConvert.SerializeObject(data, settings);
                byte[] cipheredJson = AesEncryptor.Encrypt(json);
                string encryptedJson = $"{BitConverter.ToDouble(cipheredJson)}";

                writer.Write(encryptedJson);
                writer.Close();
            }
        }

        public static void DeleteData(string name)
        {
            string dataPath = Path.Combine(Application.persistentDataPath, $"{name}.dat");

            if (File.Exists(dataPath))
            {
                File.Delete(dataPath);
            }
        }
    }
}
