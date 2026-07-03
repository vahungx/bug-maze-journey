namespace _Project.Scripts.Shared.Pooling
{
     using _Project.Scripts.Shared.Services;
     using UnityEngine;

     public static class PoolExtensions
     {
          private static PoolManager GetPool()
          {
               if (ServiceLocator.TryResolve<PoolManager>(out var pool)) return pool;

               // Fallback: tự tạo nếu quên register trong bootstrap
               pool = new PoolManager();
               ServiceLocator.Register<PoolManager>(pool, overwrite: true, disposeOnReset: true);

               return pool;
          }

          public static GameObject Spawn(this GameObject prefab,          Transform parent = null, Vector3? position = null,
               Quaternion?                               rotation = null, int       initialSize = 1)
          {
               return GetPool().Spawn(prefab, parent, position, rotation, initialSize).gameObject;
          }

          public static T Spawn<T>(this GameObject prefab,          Transform parent      = null, Vector3? position = null,
               Quaternion?                         rotation = null, int       initialSize = 1)
               where T : Component
          {
               return GetPool().Spawn<T>(prefab, parent, position, rotation, initialSize);
          }

          public static void Recycle(this GameObject go) => GetPool().Recycle(go);

          public static void WarmUp(this GameObject prefab, int count) => GetPool().WarmUp(prefab, count);

          public static void RecycleAll(this GameObject prefab) => GetPool().RecycleAll(prefab);
     }
}
