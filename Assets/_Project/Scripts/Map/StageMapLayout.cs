namespace _Project.Scripts.Map
{
     using System.Collections.Generic;
     using System.Threading;
     using _Project.Scripts.Shared;
     using _Project.Scripts.Shared.Addressable;
     using _Project.Scripts.Shared.Pooling;
     using _Project.Scripts.Shared.Services;
     using Cysharp.Threading.Tasks;
     using UnityEngine;
     using UnityEngine.UI;

     public class StageMapLayout : MonoBehaviour
     {
          [SerializeField] private ScrollRect    _scrollRect;
          [SerializeField] private RectTransform _content;
          [SerializeField] private string        _rowPrefabKey = "Row";

          [Header("Stage Row Layout")]
          [SerializeField] private int stagePerRow = 4;

          [SerializeField]         private float rowHeight  = 250f;
          [SerializeField, Min(1)] private int   maxRowShow = 6;

          private readonly List<StageMapRow> _rowPool = new();

          private int                 _totalRow;
          private int                 _currentPoolStart  = -1;
          private int                 _pendingStageIndex = Constant.STAGE_INDEX_MIN;
          private bool                _isBuilt;
          private GameObject          _rowPrefab;
          private PoolManager         _poolManager;
          private IAddressableService _addressableService;

          private void Start()
          {
               BuildAsync(this.GetCancellationTokenOnDestroy()).Forget();
          }

          private void OnDestroy()
          {
               if (_scrollRect != null)
                    _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);

               if (_poolManager != null && _rowPrefab != null)
                    _poolManager.DestroyPool(_rowPrefab);

               _rowPool.Clear();

               if (_addressableService != null && _rowPrefab != null)
                    _addressableService.ReleaseAsset(_rowPrefabKey);

               _rowPrefab = null;
          }

          private async UniTask BuildAsync(CancellationToken ct)
          {
               _totalRow = Mathf.CeilToInt(Constant.STAGE_COUNT / (float)stagePerRow);
               float contentHeight = _totalRow * rowHeight;

               // Content bắt đầu từ bottom và kéo dài lên top
               _content.anchorMin        = new Vector2(0f, 0f);
               _content.anchorMax        = new Vector2(1f, 0f);
               _content.pivot            = new Vector2(0.5f, 0f);
               _content.anchoredPosition = Vector2.zero;
               _content.sizeDelta        = new Vector2(0f, contentHeight);

               int poolSize = Mathf.Min(_totalRow, maxRowShow + 2);
               _addressableService = ServiceLocator.Resolve<IAddressableService>();
               _poolManager        = ServiceLocator.Resolve<PoolManager>();
               _rowPrefab          = await _addressableService.LoadAssetAsync<GameObject>(_rowPrefabKey, ct);

               if (_rowPrefab == null)
               {
                    Debug.LogError($"[StageMapLayout] Failed to load addressable prefab '{_rowPrefabKey}'.");

                    return;
               }

               for (int poolIndex = 0; poolIndex < poolSize; poolIndex++)
               {
                    var item = _poolManager.Spawn(_rowPrefab, _content, initialSize: poolSize);
                    var row  = item.GetComponent<StageMapRow>();

                    if (row == null)
                    {
                         Debug.LogError($"[StageMapLayout] Addressable prefab '{_rowPrefabKey}' missing StageMapRow.");
                         _poolManager.Recycle(item);

                         continue;
                    }

                    RectTransform rowRect = (RectTransform)row.transform;

                    rowRect.SetParent(_content, false);
                    rowRect.anchorMin = new Vector2(0f, 0f);
                    rowRect.anchorMax = new Vector2(1f, 0f);
                    rowRect.pivot     = new Vector2(0.5f, 0f);
                    rowRect.sizeDelta = new Vector2(0f, rowHeight);
                    _rowPool.Add(row);
               }

               Canvas.ForceUpdateCanvases();

               _isBuilt = true;
               _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
               ScrollToStage(_pendingStageIndex);
          }

          private void OnScrollValueChanged(Vector2 _)
          {
               if (_totalRow <= maxRowShow) return;

               int maxFirstVisibleRow = Mathf.Max(0, _totalRow - maxRowShow);

               int firstVisibleRow = Mathf.FloorToInt(
                    Mathf.Clamp01(_scrollRect.verticalNormalizedPosition) * maxFirstVisibleRow);

               int poolStart = Mathf.Clamp(firstVisibleRow - 1, 0, Mathf.Max(0, _totalRow - _rowPool.Count));

               RefreshRows(poolStart);
          }

          void RefreshRows(int poolStart, bool forceRefresh = false)
          {
               switch (forceRefresh)
               {
                    case true:
                         _currentPoolStart = -1;
                         _scrollRect.StopMovement();

                         break;

                    case false when poolStart == _currentPoolStart:
                         return;
               }

               _currentPoolStart = poolStart;

               for (int poolIndex = 0; poolIndex < _rowPool.Count; poolIndex++)
               {
                    int         rowIndex = poolStart + poolIndex;
                    StageMapRow row      = _rowPool[poolIndex];

                    row.SetStageRow(rowIndex);

                    var rowRect = (RectTransform)row.transform;
                    rowRect.anchoredPosition = new Vector2(0f, rowIndex * rowHeight);
                    row.transform.SetAsFirstSibling();
               }
          }

          public void ScrollToStage(int stageIndex)
          {
               _pendingStageIndex = Mathf.Clamp(stageIndex, Constant.STAGE_INDEX_MIN, Constant.STAGE_INDEX_MAX);

               if (!_isBuilt)
                    return;

               int targetRow          = _pendingStageIndex / stagePerRow;
               int maxFirstVisibleRow = Mathf.Max(0, _totalRow      - maxRowShow);
               int firstVisibleRow    = Mathf.Clamp(targetRow       - maxRowShow / 2, 0, maxFirstVisibleRow);
               int poolStart          = Mathf.Clamp(firstVisibleRow - 1, 0, Mathf.Max(0, _totalRow - _rowPool.Count));

               _scrollRect.StopMovement();

               _scrollRect.verticalNormalizedPosition = maxFirstVisibleRow == 0
                                                            ? 0f
                                                            : firstVisibleRow / (float)maxFirstVisibleRow;

               RefreshRows(poolStart, true);
          }
     }
}
