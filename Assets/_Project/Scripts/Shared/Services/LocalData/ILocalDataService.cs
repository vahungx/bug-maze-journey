namespace _Project.Scripts.Shared.Services.LocalData
{
     public interface ILocalDataService
     {
          bool IsInitialized { get; }

          void Initialize(bool forceReload = false);

          void Save();

          T Get<T>()
               where T : BaseLocalData;
     }
}
