namespace _Project.Scripts.LocalData
{
     using System;
     using System.Collections.Generic;
     using System.Linq;
     using Newtonsoft.Json;
#if UNITY_EDITOR
     using UnityEngine;
#endif

     public class MapLocalData : BaseLocalData
     {
          private const string FileName = "map.json";

          public MapLocalDataModel Model { get; set; } = CreateDefaultModel();

          public override LocalDataEnum GetEnumType() { return LocalDataEnum.Map; }

          public override void Load()
          {
               var json = LoadJson(FileName);

               if (string.IsNullOrWhiteSpace(json))
               {
                    Model = CreateDefaultModel();

                    return;
               }

               try
               {
                    Model = JsonConvert.DeserializeObject<MapLocalDataModel>(json);

                    if (Model.stageStars == null)
                    {
                         Model = new MapLocalDataModel
                         {
                              currentLevel = Model.currentLevel,
                              stageStars   = new Dictionary<int, int>()
                         };
                    }
               } catch (Exception exception)
               {
               #if UNITY_EDITOR
                    Debug.LogWarning($"Could not load {FileName}. Default map data will be used.\n{exception.Message}");
               #endif
                    Model = CreateDefaultModel();
               }
          }

          public override void Save()
          {
               var json = JsonConvert.SerializeObject(Model, Formatting.Indented);
               SaveJson(FileName, json);
          }

          private static MapLocalDataModel CreateDefaultModel()
          {
               return new MapLocalDataModel
               {
                    currentLevel = 1,
                    stageStars   = new Dictionary<int, int>()
               };
          }
     }

     [Serializable]
     public class MapLocalDataModel
     {
          public int                  currentLevel = 0;
          public Dictionary<int, int> stageStars; // Key: Stage number, Value: Star count

          public void AddOrUpdateStageStars(int stageNumber, int starCount)
          {
               stageStars              ??= new Dictionary<int, int>();
               stageStars[stageNumber] =   starCount;
          }

          public void Reset()
          {
               currentLevel = 0;
               stageStars   = new Dictionary<int, int>();
          }
          
          public void ResetStageStars(int stageNumber)
          {
               stageStars              ??= new Dictionary<int, int>();
               stageStars[stageNumber] =   0;
          }

          public int GetTotalStars()
          {
               int totalStars = 0;

               if (stageStars == null) return totalStars;

               return stageStars.Values.Sum();
          }

          public int GetStarsForStage(int stageNumber)
          {
               if (stageStars == null || !stageStars.TryGetValue(stageNumber, out int star)) return 0;

               return star;
          }

          public bool ContainsStage(int stageNumber) { return stageStars != null && stageStars.ContainsKey(stageNumber); }
     }
}
