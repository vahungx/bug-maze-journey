namespace _Project.Scripts.UI.Screen
{
     using System;
     using _Project.Scripts.Shared.Addressable;
     using _Project.Scripts.Shared.Services;
     using _Project.Scripts.Shared.Services.UI;
     using TMPro;
     using UnityEngine;
     using UnityEngine.UI;

     public class MazeScreen : BaseScreen
     {
          [SerializeField] Button _backButton;
          [SerializeField] Button _hintButton;
          [SerializeField] Button _moveButton;
          [SerializeField] TextMeshProUGUI _currentLevelText;
          void Awake() { _backButton.onClick.AddListener(OnBackButtonClicked); }

          async void OnBackButtonClicked()
          {
               try
               {
                    var uiService = ServiceLocator.Resolve<IUIService>();
                    await uiService.ShowLoadingAsync<LoadingView>(nameof(LoadingView));
                    var addressableService = ServiceLocator.Resolve<IAddressableService>();
                    await addressableService.LoadSceneAsync("1.Home", UnityEngine.SceneManagement.LoadSceneMode.Single);
                    await uiService.OpenScreenAsync<BaseScreen>(nameof(MapScreen));
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
