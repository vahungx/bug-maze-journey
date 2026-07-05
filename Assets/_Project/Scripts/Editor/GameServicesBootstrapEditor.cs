#if UNITY_EDITOR
namespace _Project.Scripts.Editor
{
     using System.Collections.Generic;
     using _Project.Scripts.Core.Bootstrap;
     using _Project.Scripts.LocalData;
     using _Project.Scripts.Shared.Services;
     using _Project.Scripts.Shared.Services.LocalData;
     using UnityEditor;
     using UnityEngine;

     [CustomEditor(typeof(GameServicesBootstrap))]
     public sealed class GameServicesBootstrapEditor : UnityEditor.Editor
     {
          private bool _showStageStars = true;

          public override void OnInspectorGUI()
          {
               DrawDefaultInspector();
               EditorGUILayout.Space();
               EditorGUILayout.LabelField("Map Local Data", EditorStyles.boldLabel);

               if (!EditorApplication.isPlaying)
               {
                    EditorGUILayout.HelpBox(
                         "MapLocalData is initialized at runtime. Enter Play Mode to inspect it.",
                         MessageType.Info);
                    return;
               }

               if (!ServiceLocator.TryResolve<ILocalDataService>(out var localDataService))
               {
                    EditorGUILayout.HelpBox("ILocalDataService is not registered.", MessageType.Warning);
                    return;
               }

               var mapLocalData = localDataService.Get<MapLocalData>();
               var model        = mapLocalData?.Model;

               if (model == null)
               {
                    EditorGUILayout.HelpBox("MapLocalDataModel is not available.", MessageType.Warning);
                    return;
               }

               using (new EditorGUI.DisabledScope(true))
               {
                    EditorGUILayout.IntField("Current Level", model.CurrentLevel);
                    EditorGUILayout.IntField("Total Stars", model.GetTotalStars());
                    DrawStageStars(model.StageStars);
               }
          }

          public override bool RequiresConstantRepaint() { return EditorApplication.isPlaying; }

          private void DrawStageStars(Dictionary<int, int> stageStars)
          {
               int stageCount = stageStars?.Count ?? 0;
               _showStageStars = EditorGUILayout.Foldout(
                    _showStageStars,
                    $"Stage Stars ({stageCount})",
                    true);

               if (!_showStageStars) return;

               if (stageCount == 0)
               {
                    EditorGUILayout.LabelField("No stage data.");
                    return;
               }

               var stageNumbers = new List<int>(stageStars.Keys);
               stageNumbers.Sort();

               EditorGUI.indentLevel++;

               foreach (int stageNumber in stageNumbers)
               {
                    EditorGUILayout.IntField($"Stage {stageNumber}", stageStars[stageNumber]);
               }

               EditorGUI.indentLevel--;
          }
     }
}
#endif
