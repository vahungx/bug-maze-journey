namespace _Project.Scripts.Shared.Pooling
{
     using System;
     using System.Collections.Generic;
     using UnityEngine;
     using Object = UnityEngine.Object;

     /// <summary>
     /// Central pool service (non-MonoBehaviour).
     /// Key = prefab instance.
     /// </summary>
     public sealed class PoolManager : IDisposable
     {
          private readonly Dictionary<GameObject, PrefabPool> _pools = new();
          private readonly Transform                          _root;

          public PoolManager(string rootName = "[PoolManagerRoot]")
          {
               var go = new GameObject(rootName);
               Object.DontDestroyOnLoad(go);
               _root = go.transform;
          }

          public PoolItem Spawn(GameObject prefab,          Transform parent      = null, Vector3? position = null,
               Quaternion?                 rotation = null, int       initialSize = 1)
          {
               return GetOrCreatePool(prefab, initialSize).
                    Spawn(parent, position ?? Vector3.zero, rotation ?? Quaternion.identity);
          }

          public T Spawn<T>(GameObject prefab,          Transform parent      = null, Vector3? position = null,
               Quaternion?             rotation = null, int       initialSize = 1)
               where T : Component
          {
               return Spawn(prefab, parent, position, rotation, initialSize).GetComponent<T>();
          }

          public void Recycle(PoolItem item)
          {
               if (item == null) return;

               item.Recycle();
          }

          public void Recycle(GameObject go)
          {
               if (go == null) return;

               var item = go.GetComponent<PoolItem>();

               if (item != null) item.Recycle();
               else Debug.LogWarning($"[PoolManager] '{go.name}' has no PoolItem.", go);
          }

          public void WarmUp(GameObject prefab, int count, int initialSize = 0)
          {
               GetOrCreatePool(prefab, Math.Max(initialSize, 0)).WarmUp(count);
          }

          public void RecycleAll(GameObject prefab)
          {
               if (_pools.TryGetValue(prefab, out var pool)) pool.RecycleAll();
          }

          public void RecycleAll()
          {
               foreach (var pool in _pools.Values) pool.RecycleAll();
          }

          public void DestroyPool(GameObject prefab)
          {
               if (!_pools.TryGetValue(prefab, out var pool)) return;

               pool.Destroy();
               _pools.Remove(prefab);
          }

          public (int idle, int active) GetStats(GameObject prefab)
          {
               return _pools.TryGetValue(prefab, out var p) ? (p.IdleCount, p.ActiveCount) : (0, 0);
          }

          public void Clear()
          {
               foreach (var pool in _pools.Values) pool.Destroy();
               _pools.Clear();
          }

          public void Dispose()
          {
               Clear();
               if (_root != null) Object.Destroy(_root.gameObject);
          }

          private PrefabPool GetOrCreatePool(GameObject prefab, int initialSize)
          {
               if (_pools.TryGetValue(prefab, out var pool)) return pool;

               var poolRoot = new GameObject($"[Pool] {prefab.name}").transform;
               poolRoot.SetParent(_root, false);

               pool           = new PrefabPool(prefab, poolRoot, initialSize <= 0 ? 1 : initialSize);
               _pools[prefab] = pool;

               return pool;
          }
     }
}
