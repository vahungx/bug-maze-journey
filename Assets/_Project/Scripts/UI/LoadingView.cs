namespace _Project.Scripts.UI
{
     using System;
     using System.Threading;
     using Cysharp.Threading.Tasks;
     using DG.Tweening;
     using UnityEngine;

     [RequireComponent(typeof(CanvasGroup))]
     public sealed class LoadingView : BaseLoading
     {
          [SerializeField, Min(0f)] float _fadeDuration = 0.25f;
          [SerializeField]          Ease  _fadeEase     = Ease.InOutSine;

          private CanvasGroup _canvasGroup;

          private void Awake() => _canvasGroup = GetComponent<CanvasGroup>();

          internal override async UniTask FadeInAsync(Action onCompleted, CancellationToken ct)
          {
               _canvasGroup.DOKill();
               _canvasGroup.alpha          = 0f;
               _canvasGroup.blocksRaycasts = true;

               await _canvasGroup.DOFade(1f, _fadeDuration)
                    .SetEase(_fadeEase)
                    .SetUpdate(true)
                    .SetTarget(_canvasGroup)
                    .AsyncWaitForCompletion();

               ct.ThrowIfCancellationRequested();
               onCompleted?.Invoke();
          }

          internal override async UniTask FadeOutAsync(Action onCompleted, CancellationToken ct)
          {
               _canvasGroup.DOKill();

               await _canvasGroup.DOFade(0f, _fadeDuration)
                    .SetEase(_fadeEase)
                    .SetUpdate(true)
                    .SetTarget(_canvasGroup)
                    .AsyncWaitForCompletion();

               ct.ThrowIfCancellationRequested();
               _canvasGroup.blocksRaycasts = false;
               onCompleted?.Invoke();
          }

          private void OnDestroy() => _canvasGroup?.DOKill();
     }
}
