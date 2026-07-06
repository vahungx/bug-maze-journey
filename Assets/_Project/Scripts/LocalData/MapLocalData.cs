namespace _Project.Scripts.LocalData
{
     using System;
     using System.Collections.Generic;
     using System.Linq;
     using _Project.Scripts.Shared;
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

                    if (Model.StageStars == null)
                    {
                         Model = new MapLocalDataModel
                         {
                              CurrentStageIndex = Model.CurrentStageIndex,
                              StageStars        = new Dictionary<int, int>()
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
                    CurrentStageIndex = 1,
                    StageStars        = new Dictionary<int, int>()
               };
          }
     }

     [Serializable]
     public class MapLocalDataModel
     {
          public int                  CurrentStageIndex = 0;
          public Dictionary<int, int> StageStars; // Key: Stage number, Value: Star count

          public void AddOrUpdateStageStars(int stageNumber, int starCount)
          {
               StageStars              ??= new Dictionary<int, int>();
               StageStars[stageNumber] =   starCount;
          }

          public void Reset()
          {
               CurrentStageIndex = 0;
               StageStars        = new Dictionary<int, int>();
          }

          public void ResetStageStars(int stageNumber)
          {
               StageStars              ??= new Dictionary<int, int>();
               StageStars[stageNumber] =   0;
          }

          public int GetTotalStars()
          {
               int totalStars = 0;

               if (StageStars == null) return totalStars;

               return StageStars.Values.Sum();
          }

          public int GetStarsForStage(int stageNumber)
          {
               if (StageStars == null || !StageStars.TryGetValue(stageNumber, out int star)) return 0;

               return star;
          }

          public bool ContainsStage(int stageNumber) { return StageStars != null && StageStars.ContainsKey(stageNumber); }

          public void RandomizeUnlockedStageStars()
          {
               StageStars ??= new Dictionary<int, int>();
               StageStars.Clear();

               var random = new System.Random();

               for (int stageIndex = Constant.STAGE_INDEX_MIN; stageIndex < CurrentStageIndex; stageIndex++)
               {
                    StageStars[stageIndex] = random.Next(1, Constant.STAGE_STARS_MAX + 1);
               }

               StageStars[CurrentStageIndex] = 0;
          }

          public void UnlockNewLevelByStage(int stageIndex)
          {
               if (stageIndex < CurrentStageIndex || stageIndex >= Constant.STAGE_INDEX_MAX) return;

               CurrentStageIndex = stageIndex + 1;
               AddOrUpdateStageStars(CurrentStageIndex, 0);
          }
     }
}
