namespace _Project.Scripts.UI
{
     using System;
     using System.Threading;
     using Cysharp.Threading.Tasks;

     public abstract class BaseLoading : BaseUI
     {
          internal abstract UniTask FadeInAsync(Action onCompleted, CancellationToken ct);

          internal abstract UniTask FadeOutAsync(Action onCompleted, CancellationToken ct);
     }
}
