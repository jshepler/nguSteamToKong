using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace nguSteamToKong
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //args = new[] { @"NGUSave-Build-1260-April-08-05-41.txt" };

            string path;
            FileInfo file = null;

            if (args.Length == 0)
            {
                Console.WriteLine("file path not passed");
            }
            else if (!(file = new FileInfo(path = args[0])).Exists)
            {
                Console.WriteLine($"file not found: {path}");
            }
            else
            {
                Console.WriteLine(convert(file));
            }

            Console.WriteLine("\nPress ENTER to exit");
            Console.ReadLine();
        }

        static string convert(FileInfo file)
        {
            try
            {
                var formatter = new BinaryFormatter();
                formatter.Binder = new nguSerializationBinder();
                formatter.SurrogateSelector = new NonSerializableSurrogateSelector();

                var steam_base64Data = File.ReadAllText(file.FullName);
                var steam_saveData = DeserializeSaveData(formatter, steam_base64Data);
                var steam_playerData = DeserializePlayerData(formatter, steam_saveData.playerData);
                steam_playerData.version = 1220;

                var kong_playerData = SerializePlayerData(formatter, steam_playerData);
                var kong_base64PlayerData = Convert.ToBase64String(kong_playerData);

                var md5 = new MD5CryptoServiceProvider();
                var kong_base64Checksum = Convert.ToBase64String(md5.ComputeHash(kong_playerData));

                var kong_saveData = new SaveData(kong_base64PlayerData, kong_base64Checksum);
                var kong_base64SaveData = SerializeSaveData(formatter, kong_saveData);

                var newPath = file.FullName.Substring(0, file.FullName.Length - 5) + " (kong).txt";
                File.WriteAllText(newPath, kong_base64SaveData);

                return "downgrade successful";
            }

            catch (Exception ex)
            {
                return "downgrade failed";
            }
        }

        static SaveData DeserializeSaveData(BinaryFormatter formatter, string data)
        {
            using (MemoryStream serializationStream = new MemoryStream(Convert.FromBase64String(data)))
            {
                return (SaveData)formatter.Deserialize(serializationStream);
            }
        }

        static PlayerData DeserializePlayerData(BinaryFormatter formatter, string data)
        {
            PlayerData pd;

            using (MemoryStream serializationStream = new MemoryStream(Convert.FromBase64String(data)))
            {
                pd = (PlayerData)formatter.Deserialize(serializationStream);
            }

            return pd;
        }

        static byte[] SerializePlayerData(BinaryFormatter formatter, PlayerData value)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, value);
                memoryStream.Flush();
                return memoryStream.ToArray();
            }
        }

        static string SerializeSaveData(BinaryFormatter formatter, SaveData value)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, value);
                memoryStream.Flush();
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }
}
