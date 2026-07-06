namespace _Project.Scripts.Map
{
     using System.Collections.Generic;
     using _Project.Scripts.Shared;
     using UnityEngine;
     using UnityEngine.UI;

     public class StageMapLayout : MonoBehaviour
     {
          [SerializeField] private ScrollRect    scrollRect;
          [SerializeField] private RectTransform content;
          [SerializeField] private StageMapRow   rowPrefab;

          [Header("Stage Row Layout")]
          [SerializeField] private int stagePerRow = 4;

          [SerializeField]         private float rowHeight  = 250f;
          [SerializeField, Min(1)] private int   maxRowShow = 6;

          private readonly List<StageMapRow> _rowPool = new();

          private int  _totalRow;
          private int  _currentPoolStart  = -1;
          private int  _pendingStageIndex = Constant.STAGE_INDEX_MIN;
          private bool _isBuilt;

          private void Start()
          {
               Build();
               scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
          }

          private void OnDestroy() { scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged); }

          private void Build()
          {
               _totalRow = Mathf.CeilToInt(Constant.STAGE_COUNT / (float)stagePerRow);
               float contentHeight = _totalRow * rowHeight;

               // Content bắt đầu từ bottom và kéo dài lên top
               content.anchorMin        = new Vector2(0f, 0f);
               content.anchorMax        = new Vector2(1f, 0f);
               content.pivot            = new Vector2(0.5f, 0f);
               content.anchoredPosition = Vector2.zero;
               content.sizeDelta        = new Vector2(0f, contentHeight);

               int poolSize = Mathf.Min(_totalRow, maxRowShow + 2);

               for (int poolIndex = 0; poolIndex < poolSize; poolIndex++)
               {
                    var           row     = Instantiate(rowPrefab, content);
                    RectTransform rowRect = (RectTransform)row.transform;

                    rowRect.anchorMin = new Vector2(0f, 0f);
                    rowRect.anchorMax = new Vector2(1f, 0f);
                    rowRect.pivot     = new Vector2(0.5f, 0f);
                    rowRect.sizeDelta = new Vector2(0f, rowHeight);
                    _rowPool.Add(row);
               }

               Canvas.ForceUpdateCanvases();

               _isBuilt = true;
               ScrollToStage(_pendingStageIndex);
          }

          private void OnScrollValueChanged(Vector2 _)
          {
               if (_totalRow <= maxRowShow) return;

               int maxFirstVisibleRow = Mathf.Max(0, _totalRow - maxRowShow);

               int firstVisibleRow = Mathf.FloorToInt(
                    Mathf.Clamp01(scrollRect.verticalNormalizedPosition) * maxFirstVisibleRow);

               int poolStart = Mathf.Clamp(firstVisibleRow - 1, 0, Mathf.Max(0, _totalRow - _rowPool.Count));

               RefreshRows(poolStart);
          }

          void RefreshRows(int poolStart, bool forceRefresh = false)
          {
               switch (forceRefresh)
               {
                    case true:
                         _currentPoolStart = -1;
                         scrollRect.StopMovement();

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

               scrollRect.StopMovement();

               scrollRect.verticalNormalizedPosition = maxFirstVisibleRow == 0
                                                            ? 0f
                                                            : firstVisibleRow / (float)maxFirstVisibleRow;

               RefreshRows(poolStart, true);
          }
     }
}
