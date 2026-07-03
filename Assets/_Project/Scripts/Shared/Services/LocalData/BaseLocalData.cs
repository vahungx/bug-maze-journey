using UnityEngine;
using System.IO;

public abstract class BaseLocalData
{
     public abstract LocalDataEnum GetEnumType();

     public abstract void Load();

     public abstract void Save();

#region Save Load Json

     public void SaveJson(string fileName, string json)
     {
          string path = Path.Combine(Application.persistentDataPath, "s");

          if (!Directory.Exists(path))
          {
               Directory.CreateDirectory(path);
          }

          path = Path.Combine(path, fileName);

          File.WriteAllText(path, json);
     }

     public string LoadJson(string fileName)
     {
          string path = Path.Combine(Application.persistentDataPath, "s", fileName);

          // Debug.Log("LoadJson " + path);
          if (!File.Exists(path))
          {
               return string.Empty;
          }

          string json = File.ReadAllText(path);

          return json;
     }

     private void DeleteJson(string fileName)
     {
          string path = Path.Combine(Application.persistentDataPath, "s", fileName);

          if (File.Exists(path))
          {
               File.Delete(path);
          }
     }

#endregion
}

public enum LocalDataEnum
{
     
}
