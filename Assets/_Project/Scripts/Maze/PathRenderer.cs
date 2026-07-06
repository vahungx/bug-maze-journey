namespace _Project.Scripts.Maze
{
     using System.Collections.Generic;
     using UnityEngine;
     using UnityEngine.Serialization;

     public sealed class PathRenderer : MonoBehaviour
     {
          [Header("References")]
          [SerializeField] private SpriteRenderer _hintPrefab;

          [SerializeField] private Transform _hintRoot;

          [Header("Visual")]
          [SerializeField] private Color _hintColor = new(1f, 0.45f, 0.1f, 0.75f);

          [FormerlySerializedAs("_hintScale")]
          [SerializeField] private float _pathThickness = 0.32f;

          [SerializeField] private int   _sortingOrder = 3;

          private readonly List<SpriteRenderer>  _activeHints = new();
          private readonly Queue<SpriteRenderer> _hintPool    = new();

          public void RenderPath(IReadOnlyList<Vector2Int> path, MazeRenderer mazeRenderer)
          {
               Clear();

               if (path == null || path.Count <= 1)
                    return;

               for (int i = 1; i < path.Count; i++)
               {
                    Vector3 start = mazeRenderer.GetCellCenter(path[i - 1]);
                    Vector3 end   = mazeRenderer.GetCellCenter(path[i]);

                    if (Mathf.Approximately(start.y, end.y))
                         RenderHorizontalPath(start, end);
                    else
                         RenderVerticalPath(start, end);
               }
          }

          private void RenderHorizontalPath(Vector3 start, Vector3 end)
          {
               var hint = GetHint("HorizontalPath");

               hint.transform.position   = (start + end) * 0.5f;
               hint.transform.localScale = new Vector3(Mathf.Abs(end.x - start.x) + _pathThickness, _pathThickness, 1f);
          }

          private void RenderVerticalPath(Vector3 start, Vector3 end)
          {
               var hint = GetHint("VerticalPath");

               hint.transform.position   = (start + end) * 0.5f;
               hint.transform.localScale = new Vector3(_pathThickness, Mathf.Abs(end.y - start.y) + _pathThickness, 1f);
          }

          public void Clear()
          {
               for (int i = 0; i < _activeHints.Count; i++)
               {
                    var hint = _activeHints[i];
                    hint.gameObject.SetActive(false);
                    _hintPool.Enqueue(hint);
               }

               _activeHints.Clear();
          }

          private SpriteRenderer GetHint(string objectName)
          {
               SpriteRenderer hint;

               if (_hintPool.Count > 0)
                    hint = _hintPool.Dequeue();
               else
                    hint = Instantiate(_hintPrefab, _hintRoot);

               hint.name         = objectName;
               hint.color        = _hintColor;
               hint.sortingOrder = _sortingOrder;
               hint.gameObject.SetActive(true);
               _activeHints.Add(hint);

               return hint;
          }
     }
}
