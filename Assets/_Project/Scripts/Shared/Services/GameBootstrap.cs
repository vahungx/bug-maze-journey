namespace _Project.Scripts.Core.Bootstrap
{
     using System;
     using _Project.Scripts.Shared.Addressable;
     using _Project.Scripts.Shared.Pooling;
     using _Project.Scripts.Shared.Services;
     using _Project.Scripts.Shared.Services.Audio;
     using _Project.Scripts.Shared.Services.LocalData;
     using UnityEngine;
     using UnityEngine.UI;

     public sealed class GameServicesBootstrap : MonoBehaviour
     {
          private static bool _created;

          // ── Reset static khi domain reload (Editor Stop→Play) ──────────────
     #if UNITY_EDITOR
          [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
          private static void ResetStaticState() => _created = false;
     #endif

          // ── Tự tạo 1 lần duy nhất trước khi scene nào load ─────────────────
          [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
          private static void AutoCreate()
          {
               if (_created) return;

               var go = new GameObject("[GameServicesBootstrap]");
               DontDestroyOnLoad(go);
               go.AddComponent<GameServicesBootstrap>();

               _created = true;
          }

          private void Awake()
          {
               // Guard: nếu scene đã có sẵn 1 instance từ AutoCreate → tự hủy
               var existing = FindObjectsByType<GameServicesBootstrap>(FindObjectsSortMode.None);

               if (existing.Length > 1)
               {
                    Destroy(gameObject);

                    return;
               }

               RegisterAllServices();
               Initialize();
          }

          private void OnApplicationPause(bool onPause)
          {
               ServiceLocator.TryResolve<ILocalDataService>(out var localDataService);
               localDataService?.Save();
          }

          private void RegisterAllServices()
          {
               ServiceLocator.Register<ILocalDataService>(new LocalDataService(), overwrite: true, disposeOnReset: true);

               ServiceLocator.Register<IAddressableService>(new AddressableService(), overwrite: true,
                    disposeOnReset: true);

               ServiceLocator.Register(new PoolManager(), overwrite: true, disposeOnReset: true);

               ServiceLocator.Register<IAudioService>(
                    new AudioService("AudioSource", addressable: ServiceLocator.Resolve<IAddressableService>()),
                    overwrite: true);
          }

          private void Initialize() { ServiceLocator.Resolve<ILocalDataService>().Initialize(); }

          private void OnDestroy()
          {
               ServiceLocator.Reset(dispose: true);
               _created = false;
          }
     }
}
