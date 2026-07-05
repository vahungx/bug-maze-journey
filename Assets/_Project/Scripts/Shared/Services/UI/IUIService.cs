namespace _Project.Scripts.Shared.Services.UI
{
     using System;
     using System.Threading;
     using _Project.Scripts.UI;
     using Cysharp.Threading.Tasks;

     public interface IUIService
     {
          BaseScreen CurrentScreen { get; }

          UniTask<T> OpenScreenAsync<T>(string key, CancellationToken ct = default)
               where T : BaseScreen;

          void CloseScreen();

          UniTask<T> OpenPopupAsync<T>(string key, CancellationToken ct = default)
               where T : BasePopup;

          void ClosePopup();

          void ClosePopup(BasePopup popup);

          void CloseAllPopups();

          UniTask PreloadLoadingAsync<T>(string key, CancellationToken ct = default)
               where T : BaseLoading;

          UniTask ShowLoadingAsync<T>(string key, Action onFadeInCompleted = null, CancellationToken ct = default)
               where T : BaseLoading;

          UniTask HideLoadingAsync(Action onFadeOutCompleted = null, CancellationToken ct = default);
     }
}
