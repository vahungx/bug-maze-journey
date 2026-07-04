namespace _Project.Scripts.Map
{
     using System.Collections.Generic;
     using UnityEngine;
     using UnityEngine.UI;

     public class StageMapLayout : MonoBehaviour
     {
          [SerializeField] private ScrollRect    scrollRect;
          [SerializeField] private RectTransform content;
          [SerializeField] private StageMapRow   rowPrefab;

          [Header("Stage Row Layout")]
          [SerializeField] private int totalStage = 999;

          [SerializeField]         private int   stagePerRow = 4;
          [SerializeField]         private float rowHeight   = 250f;
          [SerializeField, Min(1)] private int   maxRowShow  = 6;

          private readonly List<StageMapRow> _rowPool = new();

          private int _totalRow;
          private int _currentPoolStart = -1;

          private void Start()
          {
               Build();
               scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
          }

          private void OnDestroy() { scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged); }

          private void Build()
          {
               _totalRow = Mathf.CeilToInt(totalStage / (float)stagePerRow);
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

               scrollRect.verticalNormalizedPosition = 0f;
               RefreshRows(0);
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

          public void RefreshRows(int poolStart, bool forceRefresh = false)
          {
               if (forceRefresh)
               {
                    _currentPoolStart        = poolStart;
                    scrollRect.StopMovement();
                    content.anchoredPosition = Vector2.zero;
               }
               else
               {
                    if (poolStart == _currentPoolStart) return;

                    _currentPoolStart = poolStart;
               }

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
     }
}
