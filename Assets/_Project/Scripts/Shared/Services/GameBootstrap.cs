namespace _Project.Scripts.Core.Bootstrap
{
     using _Project.Scripts.Shared.Addressable;
     using _Project.Scripts.Shared.Pooling;
     using _Project.Scripts.Shared.Services;
     using _Project.Scripts.Shared.Services.Audio;
     using _Project.Scripts.Shared.Services.LocalData;
     using _Project.Scripts.Shared.Services.UI;
     using UnityEngine;
     using UnityEngine.EventSystems;
     using UnityEngine.SceneManagement;

     public sealed class GameServicesBootstrap : MonoBehaviour
     {
          private static bool        _created;
          private        Camera      _mainCamera;
          private        EventSystem _eventSystem;

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
               Application.targetFrameRate = 60;
               QualitySettings.vSyncCount  = 0;

               // Guard: nếu scene đã có sẵn 1 instance từ AutoCreate → tự hủy
               var existing = FindObjectsByType<GameServicesBootstrap>(FindObjectsSortMode.None);

               if (existing.Length > 1)
               {
                    Destroy(gameObject);

                    return;
               }

               SceneManager.sceneLoaded += OnSceneLoaded;
               BindSceneInfrastructure(SceneManager.GetActiveScene());
               RegisterAllServices();
               Initialize();
          }

          private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => BindSceneInfrastructure(scene);

          private void BindSceneInfrastructure(Scene scene)
          {
               foreach (var camera in FindObjectsByType<Camera>(FindObjectsSortMode.None))
               {
                    if (!camera.CompareTag("MainCamera")) continue;

                    if (_mainCamera == null)
                    {
                         _mainCamera = camera;
                         camera.transform.SetParent(transform, true);
                    }
                    else if (camera != _mainCamera && camera.gameObject.scene == scene)
                    {
                         Destroy(_mainCamera.gameObject);
                         _mainCamera = camera;
                         camera.transform.SetParent(transform, true);
                    }
               }

               foreach (var eventSystem in FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
               {
                    if (_eventSystem == null)
                    {
                         _eventSystem = eventSystem;
                         eventSystem.transform.SetParent(transform, true);
                    }
                    else if (eventSystem != _eventSystem && eventSystem.gameObject.scene == scene)
                         Destroy(eventSystem.gameObject);
               }
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

               ServiceLocator.Register<IUIService>(new UIService(ServiceLocator.Resolve<IAddressableService>()),
                    overwrite: true, disposeOnReset: true);

               ServiceLocator.Register(new PoolManager(), overwrite: true, disposeOnReset: true);

               ServiceLocator.Register<IAudioService>(
                    new AudioService("AudioSource", addressable: ServiceLocator.Resolve<IAddressableService>()),
                    overwrite: true);
          }

          private void Initialize() { ServiceLocator.Resolve<ILocalDataService>().Initialize(); }

          private void OnDestroy()
          {
               SceneManager.sceneLoaded -= OnSceneLoaded;
               ServiceLocator.Reset(dispose: true);
               _created = false;
          }
     }
}
