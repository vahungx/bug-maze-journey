namespace _Project.Scripts.Maze
{
     using System;
     using System.Collections.Generic;
     using DG.Tweening;
     using UnityEngine;

     public class BugController : MonoBehaviour
     {
          private const float TopRotationZ    = 0f;
          private const float RightRotationZ  = -90f;
          private const float BottomRotationZ = 180f;
          private const float LeftRotationZ   = 90f;

          [Header("References")]
          [SerializeField] private Transform _bugTransform;

          [SerializeField] private SpriteRenderer _bugSpriteRenderer;

          [Header("Visual")]
          [SerializeField] private Sprite _redSprite;

          [SerializeField] private Sprite _blueSprite;

          [Header("Movement")]
          [SerializeField] private float _moveDurationPerCell = 0.12f;

          [SerializeField] private Ease _moveEase = Ease.Linear;

          private Sequence _moveSequence;

          public bool IsMoving => _moveSequence != null && _moveSequence.IsActive() && _moveSequence.IsPlaying();

          public void SetToCell(Vector2Int cell, MazeRenderer mazeRenderer)
          {
               KillMove();

               _bugTransform.position = mazeRenderer.GetCellCenter(cell);
               SetDirection(TopRotationZ);
               _bugSpriteRenderer.sprite = _redSprite;
          }

          public void MoveAlongPath(IReadOnlyList<Vector2Int> path, MazeRenderer mazeRenderer, Action onCompleted = null)
          {
               KillMove();

               if (path == null || path.Count <= 1)
               {
                    onCompleted?.Invoke();

                    return;
               }

               _moveSequence = DOTween.Sequence();

               // i = 1 vì path[0] là cell hiện tại của bug.
               for (int i = 1; i < path.Count; i++)
               {
                    float   directionZ     = GetDirectionZ(path[i - 1], path[i]);
                    Vector3 targetPosition = mazeRenderer.GetCellCenter(path[i]);

                    _moveSequence.AppendCallback(() => SetDirection(directionZ));
                    _moveSequence.Append(_bugTransform.DOMove(targetPosition, _moveDurationPerCell).SetEase(_moveEase));
               }

               _moveSequence.OnComplete(() =>
               {
                    _moveSequence             = null;
                    _bugSpriteRenderer.sprite = _blueSprite;
                    onCompleted?.Invoke();
               });
          }

          private static float GetDirectionZ(Vector2Int from, Vector2Int to)
          {
               Vector2Int direction = to - from;

               if (direction == Vector2Int.up)
                    return BottomRotationZ;

               if (direction == Vector2Int.right)
                    return RightRotationZ;

               if (direction == Vector2Int.left)
                    return LeftRotationZ;

               return TopRotationZ;
          }

          private void SetDirection(float rotationZ) { _bugTransform.localRotation = Quaternion.Euler(0f, 0f, rotationZ); }

          public void KillMove()
          {
               if (_moveSequence == null)
                    return;

               if (_moveSequence.IsActive())
                    _moveSequence.Kill();

               _moveSequence = null;
          }

          private void OnDestroy() { KillMove(); }
     }
}
