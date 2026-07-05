namespace _Project.Scripts.Shared.Addressable
{
     using System.Threading;
     using Cysharp.Threading.Tasks;

     public interface IAddressableSceneHandle
     {
          float Progress { get; }

          UniTask ActivateAsync(CancellationToken ct = default);
     }
}
