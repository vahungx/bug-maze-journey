namespace _Project.Scripts.Scenes
{
     using _Project.Scripts.Shared.Addressable;
     using _Project.Scripts.Shared.Services;
     using Cysharp.Threading.Tasks;
     using UnityEngine;

     /// <summary>
     /// Scene này để create các object cần thiết cho game như gameboostrap và register các service.
     /// </summary>
     public class Splash : MonoBehaviour
     {
          async void Start()
          {
               await UniTask.Delay(1000).SuppressCancellationThrow();
               ServiceLocator.TryResolve<IAddressableService>(out var addressableService);
               await addressableService.LoadSceneAsync("0.Loading");
          }
     }
}
