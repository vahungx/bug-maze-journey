namespace _Project.Scripts.UI.Screen
{
     using _Project.Scripts.LocalData;
     using _Project.Scripts.Map;
     using _Project.Scripts.Shared;
     using _Project.Scripts.Shared.Services;
     using _Project.Scripts.Shared.Services.LocalData;
     using UnityEngine;
     using UnityEngine.UI;

     public class MapScreen : MonoBehaviour
     {
          [SerializeField] Button         _resetButton;
          [SerializeField] StageMapLayout _stageMapLayout;

          MapLocalDataModel _mapLocalDataModel;

          void Awake()
          {
               ServiceLocator.TryResolve(out ILocalDataService localDataService);
               _mapLocalDataModel = localDataService.Get<MapLocalData>().Model;
               _resetButton.onClick.AddListener(OnResetButtonClicked);
          }

          void OnResetButtonClicked()
          {
               if (_mapLocalDataModel == null)
               {
               #if UNITY_EDITOR
                    Debug.Log("<color=yellow>LocalDataService could be not initial</color>");
               #endif
                    return;
               }

               _mapLocalDataModel.Reset();
               _mapLocalDataModel.currentLevel = Random.Range(Constant.STAGE_MAP_MIN, Constant.STAGE_MAP_MAX);

               _stageMapLayout.RefreshRows(0, true);
          }
     }
}
