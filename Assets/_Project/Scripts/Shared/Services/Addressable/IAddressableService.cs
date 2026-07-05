namespace _Project.Scripts.Shared.Addressable
{
     using System;
     using System.Collections.Generic;
     using System.Threading;
     using Cysharp.Threading.Tasks;
     using UnityEngine;
     using UnityEngine.SceneManagement;
     using Object = UnityEngine.Object;

     public interface IAddressableService
     {
     #region ── Basic Load ─────────────────────────────────────────

          UniTask<T> LoadAssetAsync<T>(string key, CancellationToken ct = default)
               where T : Object;

          UniTask<IList<T>> LoadAssetsAsync<T>(IList<string> keys, CancellationToken ct = default)
               where T : Object;

     #endregion

     #region ── Label-based (load theo nhóm) ───────────────────────

          UniTask<IList<T>> LoadAssetsByLabelAsync<T>(string label, CancellationToken ct = default)
               where T : Object;

          // ── Instantiate ────────────────────────────────────────
          UniTask<GameObject> InstantiateAsync(string key, Transform parent = null, CancellationToken ct = default);

          UniTask<IList<GameObject>> InstantiateMultipleAsync(string key, int count, Transform parent = null,
               CancellationToken                                     ct = default);

     #endregion

     #region ── Scene ──────────────────────────────────────────────

          public UniTask<IAddressableSceneHandle> PreloadSceneAsync(string key, LoadSceneMode mode = LoadSceneMode.Single,
               CancellationToken                                           ct = default);

          UniTask LoadSceneAsync(string key, LoadSceneMode mode = LoadSceneMode.Single, CancellationToken ct = default);

          UniTask UnloadSceneAsync(string key);

     #endregion

     #region ── Download / Size check ──────────────────────────────

          UniTask<long> GetDownloadSizeAsync(string key);

          UniTask DownloadDependenciesAsync(string key, IProgress<float> progress = null, CancellationToken ct = default);

     #endregion

     #region ── Cache state ────────────────────────────────────────

          bool IsLoaded(string key);

          T GetCached<T>(string key)
               where T : Object;

          /// <summary>
          /// Kiểm tra xem key có tồn tại trong Addressables không
          /// </summary>
          UniTask<bool> ValidateKeyAsync(string key, CancellationToken ct = default);

     #endregion

     #region ── Release ────────────────────────────────────────────

          void ReleaseAsset(string key);

          void ReleaseInstance(GameObject instance);

          void ReleaseAll();

     #endregion
     }
}
