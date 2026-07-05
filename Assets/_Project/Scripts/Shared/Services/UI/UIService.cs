namespace _Project.Scripts.Shared.Services.UI
{
     using System;
     using System.Collections.Generic;
     using System.Threading;
     using _Project.Scripts.Shared.Addressable;
     using _Project.Scripts.UI;
     using Cysharp.Threading.Tasks;
     using UnityEngine;
     using UnityEngine.UI;

     public sealed class UIService : IUIService,
          IDisposable
     {
          private readonly IAddressableService        _addressable;
          private readonly Dictionary<string, BaseUI> _cache      = new();
          private readonly List<BasePopup>            _popupStack = new();
          private readonly GameObject                 _root;
          private readonly Transform                  _screenRoot;
          private readonly Transform                  _popupRoot;
          private readonly Transform                  _loadingRoot;
          private          BaseLoading                _loading;
          private          string                     _loadingKey;
          private          int                        _loadingCount;
          private          bool                       _disposed;

          public BaseScreen CurrentScreen { get; private set; }

          public UIService(IAddressableService addressable)
          {
               _addressable = addressable ?? throw new ArgumentNullException(nameof(addressable));
               _root        = new GameObject("[UIRoot]");
               UnityEngine.Object.DontDestroyOnLoad(_root);
               _screenRoot  = CreateCanvas("Screens", 0);
               _popupRoot   = CreateCanvas("Popups", 100);
               _loadingRoot = CreateCanvas("Loading", 200);
          }

          public async UniTask<T> OpenScreenAsync<T>(string key, CancellationToken ct = default)
               where T : BaseScreen
          {
               var screen = await GetOrCreateAsync<T>(key, _screenRoot, ct);

               if (screen == null || screen == CurrentScreen) return screen;

               CurrentScreen?.Close();
               CurrentScreen = screen;
               screen.transform.SetAsLastSibling();
               screen.Open();

               return screen;
          }

          public void CloseScreen()
          {
               CurrentScreen?.Close();
               CurrentScreen = null;
          }

          public async UniTask<T> OpenPopupAsync<T>(string key, CancellationToken ct = default)
               where T : BasePopup
          {
               var popup = await GetOrCreateAsync<T>(key, _popupRoot, ct);

               if (popup == null) return null;

               _popupStack.Remove(popup);
               _popupStack.Add(popup);
               popup.transform.SetAsLastSibling();
               popup.Open();

               return popup;
          }

          public void ClosePopup()
          {
               if (_popupStack.Count == 0) return;

               ClosePopup(_popupStack[^1]);
          }

          public void ClosePopup(BasePopup popup)
          {
               if (popup == null || !_popupStack.Remove(popup)) return;

               popup.Close();
          }

          public void CloseAllPopups()
          {
               for (var i = _popupStack.Count - 1; i >= 0; i--)
                    _popupStack[i].Close();

               _popupStack.Clear();
          }

          public async UniTask PreloadLoadingAsync<T>(string key, CancellationToken ct = default)
               where T : BaseLoading
          {
               await GetOrCreateAsync<T>(key, _loadingRoot, ct);
          }

          public async UniTask ShowLoadingAsync<T>(string key, Action onFadeInCompleted = null,
               CancellationToken                          ct = default)
               where T : BaseLoading
          {
               if (_loading != null && _loadingKey != key)
                    throw new InvalidOperationException($"[UIService] Loading '{_loadingKey}' is already configured.");

               var loading = await GetOrCreateAsync<T>(key, _loadingRoot, ct);

               if (loading == null) return;

               _loading    = loading;
               _loadingKey = key;

               if (_loadingCount++ == 0)
               {
                    loading.transform.SetAsLastSibling();
                    loading.Open();
                    await loading.FadeInAsync(onFadeInCompleted, ct);
               }
          }

          public async UniTask HideLoadingAsync(Action onFadeOutCompleted = null, CancellationToken ct = default)
          {
               if (_disposed || _loadingCount == 0) return;

               _loadingCount--;

               if (_loadingCount == 0)
               {
                    await _loading.FadeOutAsync(onFadeOutCompleted, ct);
                    _loading?.Close();
               }
          }

          public void Dispose()
          {
               _disposed     = true;
               _loadingCount = 0;
               _loading?.Close();
               CloseAllPopups();
               CloseScreen();

               foreach (var view in _cache.Values)
                    if (view != null)
                         _addressable.ReleaseInstance(view.gameObject);

               _cache.Clear();
               _loading    = null;
               _loadingKey = null;

               if (_root != null)
                    UnityEngine.Object.Destroy(_root);
          }

          private async UniTask<T> GetOrCreateAsync<T>(string key, Transform parent, CancellationToken ct)
               where T : BaseUI
          {
               if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Addressable key cannot be empty.", nameof(key));

               if (_cache.TryGetValue(key, out var cached))
               {
                    if (cached is T typed) return typed;

                    throw new InvalidOperationException(
                         $"[UIService] Key '{key}' is cached as {cached.GetType().Name}, not {typeof(T).Name}.");
               }

               // var prefab = await _addressable.LoadAssetAsync<GameObject>(key, ct);

               var instance = await _addressable.InstantiateAsync(key, parent, ct);

               if (instance == null) return null;

               var view = instance.GetComponent<T>();

               if (view == null)
               {
                    _addressable.ReleaseInstance(instance);

                    throw new InvalidOperationException($"[UIService] Prefab '{key}' does not contain {typeof(T).Name}.");
               }

               Stretch(instance.transform);
               instance.SetActive(false);
               _cache.Add(key, view);

               return view;
          }

          private Transform CreateCanvas(string name, int sortingOrder)
          {
               var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                    typeof(GraphicRaycaster));

               var canvas = go.GetComponent<Canvas>();
               canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
               canvas.sortingOrder = sortingOrder;
               var canvasScaler = go.GetComponent<CanvasScaler>();
               canvasScaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
               canvasScaler.referenceResolution = new Vector2(1080, 1920);
               canvasScaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.Expand;
               go.transform.SetParent(_root.transform, false);

               return go.transform;
          }

          private static void Stretch(Transform target)
          {
               if (target is not RectTransform rectTransform) return;

               rectTransform.anchorMin = Vector2.zero;
               rectTransform.anchorMax = Vector2.one;
               rectTransform.offsetMin = Vector2.zero;
               rectTransform.offsetMax = Vector2.zero;
          }
     }
}
