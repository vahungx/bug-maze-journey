namespace _Project.Scripts.Scenes
{
     using System;
     using Cysharp.Threading.Tasks;
     using DG.Tweening;
     using UnityEngine;
     using UnityEngine.SceneManagement;
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

               var cancellationToken = this.GetCancellationTokenOnDestroy();

               AsyncOperation sceneLoad;

               try
               {
                    sceneLoad = SceneManager.LoadSceneAsync(_homeSceneName, LoadSceneMode.Single);
               } catch (Exception exception)
               {
               #if UNITY_EDITOR
                    Debug.LogError($"[Loading] Cannot load scene '{_homeSceneName}'. Exception: {exception.Message}");
               #endif
                    return;
               }

               if (sceneLoad == null)
               {
               #if UNITY_EDITOR
                    Debug.LogError(
                         $"[Loading] Cannot load scene '{_homeSceneName}'. Make sure it is added to Build Settings.");
               #endif
                    return;
               }

               sceneLoad.allowSceneActivation = false;

               _progressBar.fillAmount = 0.3f;

               _progressBar.DOFillAmount(1f, _loadingDuration).SetEase(Ease.Linear).SetTarget(_progressBar);

               try
               {
                    var preloadTask = UniTask.WaitUntil(() => sceneLoad.progress >= 0.9f,
                         cancellationToken: cancellationToken);

                    var progressTask =
                         UniTask.WaitUntil(() => _progressBar.fillAmount >= Mathf.Max(1f, _progressBar.fillAmount),
                              cancellationToken: cancellationToken);

                    await UniTask.WhenAll(preloadTask, progressTask);
               } catch (OperationCanceledException)
               {
                    // Object destroyed, ignore safely.
               } finally
               {
                    if (_progressBar != null)
                    {
                         _progressBar.DOKill();
                    }

                    _progressBar.fillAmount        = 1f;
                    sceneLoad.allowSceneActivation = true;
               }
          }
     }
}
