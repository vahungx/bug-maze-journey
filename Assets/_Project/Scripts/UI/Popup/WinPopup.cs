namespace _Project.Scripts.UI.Popup
{
     using System;
     using _Project.Scripts.LocalData;
     using _Project.Scripts.Map;
     using _Project.Scripts.Shared;
     using _Project.Scripts.Shared.Addressable;
     using _Project.Scripts.Shared.Services;
     using _Project.Scripts.Shared.Services.LocalData;
     using _Project.Scripts.Shared.Services.UI;
     using _Project.Scripts.UI.Screen;
     using UnityEngine;
     using UnityEngine.UI;

     public class WinPopupModal : IUIModel
     {
          public int Stars;
          public int StageIndex;
     }

     public class WinPopup : BasePopup
     {
          [SerializeField] Button     _nextButton;
          [SerializeField] Button     _menuButton;
          [SerializeField] StageStars _stageStars;

          void Awake()
          {
               _nextButton.onClick.AddListener(OnNextButtonClicked);
               _menuButton.onClick.AddListener(OnMenuButtonClicked);
          }

          WinPopupModal _modal;

          protected override void OnOpen(IUIModel model)
          {
               base.OnOpen(model);
               _modal = model as WinPopupModal;

               if (_modal == null) return;

               _nextButton.gameObject.SetActive(_modal.StageIndex < Constant.STAGE_INDEX_MAX);
               _stageStars.SetStageStar(_modal.Stars);
          }

          async void OnMenuButtonClicked()
          {
               try
               {
                    var uiService = ServiceLocator.Resolve<IUIService>();
                    await uiService.ShowLoadingAsync<LoadingView>(nameof(LoadingView));
                    uiService.ClosePopup();
                    var addressableService = ServiceLocator.Resolve<IAddressableService>();
                    await addressableService.LoadSceneAsync(Constant.SCENE_HOME);

                    var nextStageIndex = _modal.StageIndex + 1 <= Constant.STAGE_INDEX_MAX
                                              ? _modal.StageIndex + 1
                                              : Constant.STAGE_INDEX_MAX;

                    await uiService.OpenScreenAsync<BaseScreen>(nameof(MapScreen), new MapScreenModel()
                    {
                         StageIndex = nextStageIndex,
                    });

                    await uiService.HideLoadingAsync();
               } catch (Exception e)
               {
               #if UNITY_EDITOR
                    Debug.LogError($"[MazeScreen] Failed to load Home scene: {e.Message}");
               #endif
               }
          }

          async void OnNextButtonClicked()
          {
               try
               {
                    var nextStageIndex = _modal.StageIndex + 1;
                    var uiService      = ServiceLocator.Resolve<IUIService>();
                    await uiService.ShowLoadingAsync<LoadingView>(nameof(LoadingView));
                    uiService.ClosePopup();
                    Scenes.Maze.Instance.Init(_modal.StageIndex + 1);

                    await uiService.OpenScreenAsync<MazeScreen>(nameof(MazeScreen), new MazeScreenModel()
                    {
                         MazeController = Scenes.Maze.Instance.MazeController,
                         StageIndex     = nextStageIndex,
                    });

                    await uiService.HideLoadingAsync();
               } catch (Exception e)
               {
               #if UNITY_EDITOR
                    Debug.LogError($"[MazeScreen] Failed to load Next map  {e.Message}");
               #endif
               }
          }
     }
}
