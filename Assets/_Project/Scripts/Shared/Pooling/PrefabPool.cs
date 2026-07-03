namespace _Project.Scripts.Shared.Pooling
{
     using System.Collections.Generic;
     using UnityEngine;
     using Object = UnityEngine.Object;

     /// <summary>
     /// Per-prefab pool nội bộ.
     /// Stack → idle objects (inactive).
     /// HashSet → active / spawned objects.
     /// </summary>
     internal sealed class PrefabPool
     {
          // ── Config ─────────────────────────────────────────────────────────
          private readonly GameObject _prefab;
          private readonly Transform  _root; // container ẩn trong hierarchy

          // ── Collections ────────────────────────────────────────────────────
          private readonly Stack<PoolItem>   _idle   = new();
          private readonly HashSet<PoolItem> _active = new();

          // ── Stats ──────────────────────────────────────────────────────────
          internal int IdleCount   => _idle.Count;
          internal int ActiveCount => _active.Count;

          // ── Ctor ───────────────────────────────────────────────────────────
          internal PrefabPool(GameObject prefab, Transform root, int initialSize = 1)
          {
               _prefab = prefab;
               _root   = root;

               if (initialSize > 0) WarmUp(initialSize);
          }

          // ── Spawn ──────────────────────────────────────────────────────────
          internal PoolItem Spawn(Transform parent, Vector3 position, Quaternion rotation)
          {
               var item = _idle.Count > 0 ? _idle.Pop() : CreateItem();

               var t = item.transform;
               t.SetParent(parent);
               t.SetPositionAndRotation(position, rotation);
               t.localScale = _prefab.transform.localScale; // reset scale

               item.State = PoolState.Spawned;
               item.gameObject.SetActive(true);

               _active.Add(item);

               // BroadcastCallbacks(item, spawn: true);

               return item;
          }

          // ── Recycle ────────────────────────────────────────────────────────
          internal void Recycle(PoolItem item)
          {
               if (!_active.Remove(item)) return; // idempotent

               // BroadcastCallbacks(item, spawn: false);

               item.gameObject.SetActive(false);
               item.transform.SetParent(_root, worldPositionStays: false);
               item.State = PoolState.Pooled;

               _idle.Push(item);
          }

          // ── RecycleAll ─────────────────────────────────────────────────────
          internal void RecycleAll()
          {
               // snapshot tránh modify trong lặp
               var snapshot = new List<PoolItem>(_active);
               foreach (var item in snapshot) Recycle(item);
          }

          // ── WarmUp ─────────────────────────────────────────────────────────
          internal void WarmUp(int count)
          {
               for (int i = 0; i < count; i++)
                    _idle.Push(CreateItem());
          }

          // ── Destroy ────────────────────────────────────────────────────────
          internal void Destroy()
          {
               RecycleAll();

               while (_idle.Count > 0)
               {
                    var item = _idle.Pop();
                    if (item != null) Object.Destroy(item.gameObject);
               }

               if (_root != null) Object.Destroy(_root.gameObject);
          }

          // ── Factory ────────────────────────────────────────────────────────
          private PoolItem CreateItem()
          {
               var go = Object.Instantiate(_prefab, _root);
               go.SetActive(false);
               go.name = _prefab.name; // bỏ suffix "(Clone)"

               var item = go.GetComponent<PoolItem>() ?? go.AddComponent<PoolItem>();
               item.SourcePrefab = _prefab;
               item.Pool         = this;
               item.State        = PoolState.Pooled;

               return item;
          }

          // ── Callbacks ──────────────────────────────────────────────────────
          // private static void BroadcastCallbacks(PoolItem item, bool spawn)
          // {
          //      var callbacks = item.GetComponentsInChildren<IPoolCallbacks>(includeInactive: true);
          //
          //      foreach (var cb in callbacks)
          //      {
          //           if (spawn) cb.OnSpawn();
          //           else cb.OnRecycle();
          //      }
          // }
     }
}
