namespace _Project.Scripts.UI.Screen
{
     using System;
     using _Project.Scripts.LocalData;
     using _Project.Scripts.Maze;
     using _Project.Scripts.Shared;
     using _Project.Scripts.Shared.Addressable;
     using _Project.Scripts.Shared.Services;
     using _Project.Scripts.Shared.Services.UI;
     using TMPro;
     using UnityEngine;
     using UnityEngine.UI;

     public class MazeScreenModel : IUIModel
     {
          public MazeController MazeController;
          public int            StageIndex;
     }

     public class MazeScreen : BaseScreen
     {
          [SerializeField] Button          _backButton;
          [SerializeField] Button          _hintButton;
          [SerializeField] Button          _moveButton;
          [SerializeField] TextMeshProUGUI _currentLevelText;

          MazeScreenModel _model;

          protected override void OnOpen(IUIModel model)
          {
               base.OnOpen(model);
               _model = model as MazeScreenModel;

               if (_model != null)
                    _currentLevelText.text = $"Level {_model.StageIndex + 1}";
          }

          void Awake()
          {
               _backButton.onClick.AddListener(OnBackButtonClicked);
               _hintButton.onClick.AddListener(OnHintButtonClicked);
               _moveButton.onClick.AddListener(OnMoveButtonClicked);
          }

          void OnMoveButtonClicked()
          {
               if (Model is null)
               {
                    return;
               }

               _model.MazeController.BugAutoMove();
          }

          void OnHintButtonClicked()
          {
               if (Model is null)
               {
                    return;
               }

               _model.MazeController.ShowHint();
          }

          async void OnBackButtonClicked()
          {
               try
               {
                    var uiService = ServiceLocator.Resolve<IUIService>();
                    await uiService.ShowLoadingAsync<LoadingView>(nameof(LoadingView));
                    var addressableService = ServiceLocator.Resolve<IAddressableService>();
                    await addressableService.LoadSceneAsync(Constant.SCENE_HOME);

                    await uiService.OpenScreenAsync<BaseScreen>(nameof(MapScreen), new MapScreenModel()
                    {
                         StageIndex = _model.StageIndex,
                    });

                    await uiService.HideLoadingAsync();
               } catch (Exception e)
               {
               #if UNITY_EDITOR
                    Debug.LogError($"[MazeScreen] Failed to load Home scene: {e.Message}");
               #endif
               }
          }
     }
}
