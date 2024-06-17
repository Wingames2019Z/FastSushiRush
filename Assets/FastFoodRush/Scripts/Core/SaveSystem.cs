using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public static class SaveSystem
    {
        public static void SaveData<T>(T data, string fileName)
        {
            string filePath = Application.persistentDataPath + "/" + fileName + ".dat";

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(filePath, FileMode.Create);

            formatter.Serialize(fileStream, data);
            fileStream.Close();
        }

        public static T LoadData<T>(string fileName)
        {
            string filePath = Application.persistentDataPath + "/" + fileName + ".dat";

            if (File.Exists(filePath))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(filePath, FileMode.Open);

                T data = (T)formatter.Deserialize(fileStream);
                fileStream.Close();

                return data;
            }
            else
            {
                // Debug.LogError("Save file not found in " + filePath);
                return default(T);
            }
        }
    }
}
