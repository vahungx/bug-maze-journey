namespace _Project.Scripts.Shared.Addressable
{
     using System;
     using System.Collections.Generic;
     using System.Linq;
     using System.Threading;
     using Cysharp.Threading.Tasks;
     using UnityEngine;
     using UnityEngine.AddressableAssets;
     using UnityEngine.ResourceManagement.AsyncOperations;
     using UnityEngine.ResourceManagement.ResourceProviders;
     using UnityEngine.SceneManagement;
     using Object = UnityEngine.Object;

     public class AddressableService : IAddressableService,
          IDisposable
     {
          private readonly Dictionary<string, AsyncOperationHandle> _handleCache = new();
          private readonly Dictionary<string, int>                  _refCount    = new();

          // Track riêng các handle của Instantiate để release đúng cách
          private readonly Dictionary<GameObject, AsyncOperationHandle<GameObject>> _instanceHandles = new();

          // Track scene handles
          private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _sceneHandles = new();

          // ── Load single ────────────────────────────────────────────────────────
          public async UniTask<T> LoadAssetAsync<T>(string key, CancellationToken ct = default)
               where T : Object
          {
               if (_handleCache.TryGetValue(key, out var cached))
               {
                    _refCount[key]++;

                    return (T)cached.Result;
               }

               var handle = Addressables.LoadAssetAsync<T>(key);

               try
               {
                    await handle.ToUniTask(cancellationToken: ct);
               } catch (OperationCanceledException)
               {
                    Addressables.Release(handle);

                    throw;
               }

               if (handle.Status != AsyncOperationStatus.Succeeded)
               {
                    Debug.LogError($"[Addressable] Failed to load: {key}");
                    Addressables.Release(handle);

                    return null;
               }

               _handleCache[key] = handle;
               _refCount[key]    = 1;

               return handle.Result;
          }

          // ── Load nhiều key song song ───────────────────────────────────────────
          public async UniTask<IList<T>> LoadAssetsAsync<T>(IList<string> keys, CancellationToken ct = default)
               where T : Object
          {
               var tasks = keys.Select(k => LoadAssetAsync<T>(k, ct));

               return await UniTask.WhenAll(tasks);
          }

          // ── Load theo Label (e.g: "UI", "HeroIcons") ──────────────────────────
          public async UniTask<IList<T>> LoadAssetsByLabelAsync<T>(string label, CancellationToken ct = default)
               where T : Object
          {
               var handle = Addressables.LoadAssetsAsync<T>(label, null);

               try
               {
                    await handle.ToUniTask(cancellationToken: ct);
               } catch (OperationCanceledException)
               {
                    Addressables.Release(handle);

                    throw;
               }

               if (handle.Status != AsyncOperationStatus.Succeeded)
               {
                    Debug.LogError($"[Addressable] Failed to load label: {label}");
                    Addressables.Release(handle);

                    return null;
               }

               // Cache bằng label key
               _handleCache[label] = handle;
               _refCount[label]    = 1;

               return handle.Result;
          }

          // ── Instantiate (tự track handle để release đúng) ─────────────────────
          public async UniTask<GameObject> InstantiateAsync(string key, Transform parent = null,
               CancellationToken                                   ct = default)
          {
               var handle = Addressables.InstantiateAsync(key, parent);

               try
               {
                    await handle.ToUniTask(cancellationToken: ct);
               } catch (OperationCanceledException)
               {
                    Addressables.Release(handle);

                    throw;
               }

               if (handle.Status != AsyncOperationStatus.Succeeded)
               {
                    Debug.LogError($"[Addressable] Failed to instantiate: {key}");

                    return null;
               }

               _instanceHandles[handle.Result] = handle;

               return handle.Result;
          }

          // ── Spawn nhiều instance cùng lúc ─────────────────────────────────────
          public async UniTask<IList<GameObject>> InstantiateMultipleAsync(string key, int count, Transform parent = null,
               CancellationToken                                                  ct = default)
          {
               var tasks = Enumerable.Range(0, count).Select(_ => InstantiateAsync(key, parent, ct));

               return await UniTask.WhenAll(tasks);
          }

          // ── Load Scene ────────────────────────────────────────────────────────
          public async UniTask LoadSceneAsync(string key, LoadSceneMode mode = LoadSceneMode.Single,
               CancellationToken                     ct = default)
          {
               var handle = Addressables.LoadSceneAsync(key, mode);
               await handle.ToUniTask(cancellationToken: ct);

               if (handle.Status == AsyncOperationStatus.Succeeded)
                    _sceneHandles[key] = handle;
               else
                    Debug.LogError($"[Addressable] Failed to load scene: {key}");
          }

          // ── Unload Scene ──────────────────────────────────────────────────────
          public async UniTask UnloadSceneAsync(string key)
          {
               if (!_sceneHandles.TryGetValue(key, out var handle)) return;

               await Addressables.UnloadSceneAsync(handle).ToUniTask();
               _sceneHandles.Remove(key);
          }

          // ── Kiểm tra download size trước khi tải (Remote assets) ─────────────
          public async UniTask<long> GetDownloadSizeAsync(string key)
          {
               var handle = Addressables.GetDownloadSizeAsync(key);
               await handle.ToUniTask();
               var size = handle.Result;
               Addressables.Release(handle);

               return size;
          }

          // ── Download dependencies với progress callback ───────────────────────
          public async UniTask DownloadDependenciesAsync(string key, IProgress<float> progress = null,
               CancellationToken                                ct = default)
          {
               var handle = Addressables.DownloadDependenciesAsync(key);

               try
               {
                    await handle.ToUniTask(progress, cancellationToken: ct);
               } finally
               {
                    Addressables.Release(handle);
               }
          }

          // ── Cache state helpers ───────────────────────────────────────────────
          public bool IsLoaded(string key) => _handleCache.ContainsKey(key);

          public T GetCached<T>(string key)
               where T : Object
          {
               if (_handleCache.TryGetValue(key, out var handle))
                    return (T)handle.Result;

               Debug.LogWarning($"[Addressable] Asset not cached: {key}");

               return null;
          }

          /// <summary>
          /// Kiểm tra xem key có tồn tại trong Addressables không
          /// </summary>
          public async UniTask<bool> ValidateKeyAsync(string key, CancellationToken ct = default)
          {
               if (string.IsNullOrWhiteSpace(key))
               {
                    Debug.LogWarning($"[Addressable] Key is empty or null");
                    return false;
               }

               var handle = Addressables.LoadResourceLocationsAsync(key);

               try
               {
                    await handle.ToUniTask(cancellationToken: ct);
               }
               catch (OperationCanceledException)
               {
                    Addressables.Release(handle);
                    throw;
               }

               bool exists = handle.Result != null && handle.Result.Count > 0;

               if (!exists)
                    Debug.LogWarning($"[Addressable] Key not found: {key}");

               Addressables.Release(handle);

               return exists;
          }

          // ── Release ───────────────────────────────────────────────────────────
          public void ReleaseAsset(string key)
          {
               if (!_handleCache.TryGetValue(key, out var handle)) return;

               _refCount[key]--;

               if (_refCount[key] > 0) return;

               Addressables.Release(handle);
               _handleCache.Remove(key);
               _refCount.Remove(key);
          }

          // ── Release 1 instance đã Instantiate ────────────────────────────────
          public void ReleaseInstance(GameObject instance)
          {
               if (!_instanceHandles.TryGetValue(instance, out _)) return;

               Addressables.ReleaseInstance(instance); // tự destroy + release handle
               _instanceHandles.Remove(instance);
          }

          public void ReleaseAll()
          {
               foreach (var h in _handleCache.Values) Addressables.Release(h);
               foreach (var h in _instanceHandles.Values) Addressables.Release(h);
               foreach (var h in _sceneHandles.Values) Addressables.UnloadSceneAsync(h);

               _handleCache.Clear();
               _refCount.Clear();
               _instanceHandles.Clear();
               _sceneHandles.Clear();
          }

          public void Dispose() => ReleaseAll();
     }
}
