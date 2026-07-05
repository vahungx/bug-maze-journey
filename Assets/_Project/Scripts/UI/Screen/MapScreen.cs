namespace _Project.Scripts.UI.Screen
{
     using _Project.Scripts.LocalData;
     using _Project.Scripts.Map;
     using _Project.Scripts.Shared;
     using _Project.Scripts.Shared.Services;
     using _Project.Scripts.Shared.Services.LocalData;
     using _Project.Scripts.UI;
     using TMPro;
     using UnityEngine;
     using UnityEngine.UI;

     public class MapScreen : BaseScreen
     {
          [SerializeField] Button          _resetButton;
          [SerializeField] StageMapLayout  _stageMapLayout;
          [SerializeField] TextMeshProUGUI _currentLevelText;
          MapLocalDataModel                _mapLocalDataModel;

          void Awake()
          {
               ServiceLocator.TryResolve(out ILocalDataService localDataService);
               _mapLocalDataModel = localDataService.Get<MapLocalData>().Model;
               _resetButton.onClick.AddListener(OnResetButtonClicked);
               _currentLevelText.text = $"Level {_mapLocalDataModel.CurrentLevel}";
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
               _mapLocalDataModel.CurrentLevel = Random.Range(Constant.STAGE_MAP_MIN, Constant.STAGE_MAP_MAX);
               _currentLevelText.text          = $"Level {_mapLocalDataModel.CurrentLevel}";
               _stageMapLayout.RefreshRows(0, true);
          }
     }
}
