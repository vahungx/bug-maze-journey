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

     public class MapScreenModel : IUIModel
     {
          public int StageIndex;
     }

     public class MapScreen : BaseScreen
     {
          [SerializeField] Button          _resetButton;
          [SerializeField] Button          _currentLevelButton;
          [SerializeField] StageMapLayout  _stageMapLayout;
          [SerializeField] TextMeshProUGUI _currentLevelText;
          MapLocalDataModel                _mapLocalDataModel;

          void Awake()
          {
               _resetButton.onClick.AddListener(OnResetButtonClicked);

               _currentLevelButton.onClick.AddListener(() =>
               {
                    _stageMapLayout.ScrollToStage(_mapLocalDataModel.CurrentStageIndex);
               });

               ServiceLocator.TryResolve(out ILocalDataService localDataService);
               _mapLocalDataModel = localDataService.Get<MapLocalData>().Model;
               _stageMapLayout.ScrollToStage(_mapLocalDataModel.CurrentStageIndex);
          }

          protected override void OnOpen()
          {
               base.OnOpen();
               _currentLevelText.text = $"Level {_mapLocalDataModel.CurrentStageIndex + 1}";
          }

          protected override void OnOpen(IUIModel model)
          {
               base.OnOpen(model);

               if (Model is MapScreenModel mapScreenModal)
                    _stageMapLayout.ScrollToStage(mapScreenModal.StageIndex);
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
               _mapLocalDataModel.CurrentStageIndex = Random.Range(Constant.STAGE_INDEX_MIN, Constant.STAGE_COUNT);
               _mapLocalDataModel.RandomizeUnlockedStageStars();
               _currentLevelText.text = $"Level {_mapLocalDataModel.CurrentStageIndex + 1}";

               _stageMapLayout.ScrollToStage(_mapLocalDataModel.CurrentStageIndex);
          }
     }
}
