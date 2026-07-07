namespace _Project.Scripts.Scenes
{
     using System;
     using _Project.Scripts.Shared.Addressable;
     using _Project.Scripts.Shared.Services;
     using _Project.Scripts.Shared.Services.UI;
     using _Project.Scripts.UI;
     using _Project.Scripts.UI.Screen;
     using Cysharp.Threading.Tasks;
     using DG.Tweening;
     using UnityEngine;
     using UnityEngine.UI;

     public class Loading : MonoBehaviour
     {
          [SerializeField]                Image  _progressBar;
          [SerializeField, Range(3f, 5f)] float  _loadingDuration = 4f;
          [SerializeField]                string _homeSceneName   = "1.Home";

          private void Start() { LoadHomeAsync().Forget(); }

          private async UniTaskVoid LoadHomeAsync()
          {
               if (_progressBar == null)
               {
               #if UNITY_EDITOR
                    Debug.LogError("[Loading] Progress bar is missing.");
               #endif
                    return;
               }

               if (!ServiceLocator.TryResolve<IUIService>(out var uiService))
               {
               #if UNITY_EDITOR
                    Debug.LogError("[Loading] UIService is not registered.");
               #endif
                    return;
               }

               var cancellationToken = this.GetCancellationTokenOnDestroy();

               try
               {
                    var addressableService = ServiceLocator.Resolve<IAddressableService>();
                    await uiService.PreloadLoadingAsync<LoadingView>(nameof(LoadingView), cancellationToken);
                    var sceneLoad = await addressableService.PreloadSceneAsync(_homeSceneName, ct: cancellationToken);

                    _progressBar.fillAmount = 0.3f;

                    _progressBar.DOFillAmount(1f, _loadingDuration).SetEase(Ease.Linear).SetTarget(_progressBar);
                    await UniTask.WaitForSeconds(_loadingDuration, cancellationToken: cancellationToken);
                    _progressBar.DOKill();
                    _progressBar.fillAmount = 1f;
                    await uiService.ShowLoadingAsync<LoadingView>(nameof(LoadingView));

                    await sceneLoad.ActivateAsync();

                    await uiService.OpenScreenAsync<MapScreen>(nameof(MapScreen));

                    await uiService.HideLoadingAsync();
               } catch (Exception exception)
               {
               #if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogError($"[Loading] Cannot load scene '{_homeSceneName}'. Exception: {exception.Message}");
               #endif
               } finally
               {
                    if (_progressBar != null)
                         _progressBar.DOKill();
               }
          }
     }
}
