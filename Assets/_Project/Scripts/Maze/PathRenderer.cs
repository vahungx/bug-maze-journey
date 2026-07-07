namespace _Project.Scripts.Maze
{
     using System.Collections.Generic;
     using System.Threading;
     using _Project.Scripts.Shared.Addressable;
     using _Project.Scripts.Shared.Pooling;
     using _Project.Scripts.Shared.Services;
     using Cysharp.Threading.Tasks;
     using UnityEngine;
     using UnityEngine.Serialization;

     public sealed class PathRenderer : MonoBehaviour
     {
          [Header("References")]
          [SerializeField] private string _hintPrefabKey = "Square";

          [SerializeField] private Transform _hintRoot;

          [Header("Visual")]
          [SerializeField] private Color _hintColor = new(1f, 0.45f, 0.1f, 0.75f);

          [FormerlySerializedAs("_hintScale")]
          [SerializeField] private float _pathThickness = 0.32f;

          [SerializeField] private int _sortingOrder = 3;

          private readonly List<SpriteRenderer> _activeHints = new();

          private GameObject          _hintPrefab;
          private PoolManager         _poolManager;
          private IAddressableService _addressableService;
          private int                 _renderVersion;

          public void RenderPath(IReadOnlyList<Vector2Int> path, MazeRenderer mazeRenderer)
          {
               RenderPathAsync(path, mazeRenderer, this.GetCancellationTokenOnDestroy()).Forget();
          }

          public void Clear()
          {
               _renderVersion++;
               RecycleActiveHints();
          }

          private void OnDestroy()
          {
               Clear();

               if (_addressableService != null && _hintPrefab != null)
                    _addressableService.ReleaseAsset(_hintPrefabKey);

               _hintPrefab = null;
          }

          private async UniTask RenderPathAsync(IReadOnlyList<Vector2Int> path, MazeRenderer mazeRenderer,
               CancellationToken                                         ct)
          {
               int renderVersion = ++_renderVersion;
               RecycleActiveHints();

               if (path == null || path.Count <= 1)
                    return;

               await EnsurePrefabLoadedAsync(ct);

               if (renderVersion != _renderVersion || _hintPrefab == null)
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

          private async UniTask EnsurePrefabLoadedAsync(CancellationToken ct)
          {
               if (_hintPrefab != null)
                    return;

               _addressableService = ServiceLocator.Resolve<IAddressableService>();
               _poolManager        = ServiceLocator.Resolve<PoolManager>();
               _hintPrefab         = await _addressableService.LoadAssetAsync<GameObject>(_hintPrefabKey, ct);

               if (_hintPrefab == null)
                    Debug.LogError($"[PathRenderer] Failed to load addressable prefab '{_hintPrefabKey}'.");
          }

          private void RenderHorizontalPath(Vector3 start, Vector3 end)
          {
               var hint = GetHint("HorizontalPath");
               if (hint == null) return;

               hint.transform.position   = (start + end) * 0.5f;
               hint.transform.localScale = new Vector3(Mathf.Abs(end.x - start.x) + _pathThickness, _pathThickness, 1f);
          }

          private void RenderVerticalPath(Vector3 start, Vector3 end)
          {
               var hint = GetHint("VerticalPath");
               if (hint == null) return;

               hint.transform.position   = (start + end) * 0.5f;
               hint.transform.localScale = new Vector3(_pathThickness, Mathf.Abs(end.y - start.y) + _pathThickness, 1f);
          }

          private SpriteRenderer GetHint(string objectName)
          {
               var item = _poolManager.Spawn(_hintPrefab, _hintRoot);
               var hint = item.GetComponent<SpriteRenderer>();

               if (hint == null)
               {
                    Debug.LogError($"[PathRenderer] Addressable prefab '{_hintPrefabKey}' missing SpriteRenderer.");
                    _poolManager.Recycle(item);

                    return null;
               }

               hint.name         = objectName;
               hint.color        = _hintColor;
               hint.sortingOrder = _sortingOrder;
               _activeHints.Add(hint);

               return hint;
          }

          private void RecycleActiveHints()
          {
               if (_poolManager == null)
               {
                    for (int i = _hintRoot.childCount - 1; i >= 0; i--)
                         DestroyImmediate(_hintRoot.GetChild(i).gameObject);

                    _activeHints.Clear();

                    return;
               }

               for (int i = 0; i < _activeHints.Count; i++)
               {
                    var hint = _activeHints[i];

                    if (hint != null)
                         _poolManager.Recycle(hint.gameObject);
               }

               _activeHints.Clear();
          }
     }
}
