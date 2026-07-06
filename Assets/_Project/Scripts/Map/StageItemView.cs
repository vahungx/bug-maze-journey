namespace _Project.Scripts.Map
{
     using System;
     using _Project.Scripts.LocalData;
     using _Project.Scripts.Shared;
     using _Project.Scripts.Shared.Addressable;
     using _Project.Scripts.Shared.Services;
     using _Project.Scripts.Shared.Services.LocalData;
     using _Project.Scripts.Shared.Services.UI;
     using _Project.Scripts.UI;
     using _Project.Scripts.UI.Screen;
     using Cysharp.Threading.Tasks;
     using TMPro;
     using UnityEngine;
     using UnityEngine.UI;

     /// <summary>
     /// Là view item của 1 stage trong map, có thể là button hoặc image, tùy vào thiết kế UI
     /// </summary>
     public class StageItemView : MonoBehaviour
     {
          [SerializeField] GameObject      _tutorialTitleObject;
          [SerializeField] TextMeshProUGUI _levelText;
          [SerializeField] GameObject      _lockObject;
          [SerializeField] StageStars      _stageStars;
          [SerializeField] Button          _button;

          int               _index;
          MapLocalDataModel _mapLocalDataModel;

          public bool IsUnlocked { get; private set; }

          void Awake()
          {
               _mapLocalDataModel = ServiceLocator.Resolve<ILocalDataService>().Get<MapLocalData>().Model;
               _button.onClick.AddListener(OnButtonClicked);
          }

          async void OnButtonClicked()
          {
               if (_mapLocalDataModel == null)
               {
               #if UNITY_EDITOR
                    Debug.Log("<color=yellow>MapLocalDataModel could be not initial</color>");
               #endif
                    return;
               }

               if (_mapLocalDataModel.CurrentStageIndex < _index)
               {
                    // Stage chưa mở, không làm gì cả
                    return;
               }

               // Stage đã mở, thực hiện hành động khi click vào stage
          #if UNITY_EDITOR
               Debug.Log($"Stage {_index + 1} clicked!");

          #endif
               // Load vào MazeScene hoặc thực hiện hành động khác tùy vào thiết kế game
               var uiService = ServiceLocator.Resolve<IUIService>();
               await uiService.ShowLoadingAsync<LoadingView>(nameof(LoadingView));
               var addressableService = ServiceLocator.Resolve<IAddressableService>();
               await addressableService.LoadSceneAsync(Constant.SCENE_MAZE);
               Scenes.Maze.Instance.Init(_index);

               await uiService.OpenScreenAsync<BaseScreen>(nameof(MazeScreen), new MazeScreenModel()
               {
                    MazeController = Scenes.Maze.Instance.MazeController,
                    StageIndex     = _index
               });

               await uiService.HideLoadingAsync();
          }

          public void SetView(int index)
          {
               if (_mapLocalDataModel == null)
               {
               #if UNITY_EDITOR
                    Debug.Log("<color=yellow>MapLocalDataModel could be not initial</color>");

               #endif
                    return;
               }

               _index     = index;
               IsUnlocked = _mapLocalDataModel.CurrentStageIndex >= _index;
               int stars = _mapLocalDataModel.GetStarsForStage(_index);

               _stageStars.SetStageStar(IsUnlocked ? stars : 0);

               if (_index == 0)
               {
                    _tutorialTitleObject.SetActive(true);
                    _levelText.gameObject.SetActive(false);
               }
               else
               {
                    _tutorialTitleObject.SetActive(false);
                    _levelText.gameObject.SetActive(true);
               }

               _levelText.text = (_index + 1).ToString();
               _lockObject.SetActive(!IsUnlocked);
          }
     }
}
