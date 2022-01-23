using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

// ERROR: does not work for non-serializable types like Vector3 and Quaternion (or any other Unity-built type)
// unless an ISurrogate is created for every...single...type...  Use Easy Save instead.
namespace Playcraft.Saving
{
    public class SerializeData
    {
        string persistentPath => Application.persistentDataPath;
        
        string path => "/saves/";
        string fileName => "Save";
        string extension => ".save";
        string fullPath => path + fileName + extension;
        //static object saveData => SaveData.current;

        public bool Save(SaveData saveData) { return Save(fileName, saveData); }
        public bool Save(string saveName, object saveData)
        {
            //Debug.Log("Saving " + saveName);
            var formatter = new BinaryFormatter();
            
            if (!Directory.Exists(persistentPath + "/saves"))
                Directory.CreateDirectory(persistentPath + "/saves");
            
            var path = persistentPath + "/saves/" + saveName + ".save";
            var file = File.Create(path);
            
            formatter.Serialize(file, saveData);
            file.Close();
            
            return true;
        }
        
        public object Load() { return Load(fullPath); }
        public object Load(string _path)
        {
            var path = persistentPath + _path;
            //Debug.Log("Loading " + path);
        
            if (!File.Exists(path))
                return null;
                
            var formatter = new BinaryFormatter();
            var file = File.Open(path, FileMode.Open);

            try
            {
                var save = formatter.Deserialize(file);
                file.Close();
                return save;
            }
            catch
            {
                Debug.LogErrorFormat("Failed to open save file at {0}", path);
                file.Close();
                return null;
            }
        }
    }
}