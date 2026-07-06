namespace _Project.Scripts.Map
{
     using _Project.Scripts.Shared;
     using UnityEngine;
     using UnityEngine.UI;

     public class StageMapRow : MonoBehaviour
     {
          [SerializeField] GameObject      _rightLine;
          [SerializeField] GameObject      _leftLine;
          [SerializeField] StageItemView[] _stageItems;
          [SerializeField] Image           _lineHorizontal;

          int _stageRowIndex = -1;

          public void SetStageRow(int stageRowIndex)
          {
               _stageRowIndex = stageRowIndex;

               transform.name = $"Row_{_stageRowIndex}";

               if (_stageRowIndex == 0)
               {
                    _leftLine.SetActive(false);
                    _rightLine.SetActive(false);
                    SetStateViews();

                    return;
               }

               if (_stageRowIndex % 2 == 1)
               {
                    _leftLine.SetActive(false);
                    _rightLine.SetActive(true);
               }
               else
               {
                    _leftLine.SetActive(true);
                    _rightLine.SetActive(false);
               }

               SetStateViews();
          }

          private void SetStateViews()
          {
               if (_stageRowIndex == -1)
               {
               #if UNITY_EDITOR
                    Debug.Log("<color=red>StageMapRow: _stageRowIndex was wrong</color>");
               #endif
                    return;
               }

               bool fillsFromRight     = _stageRowIndex % 2 == 1;
               int  unlockedStageCount = 0;

               for (int stageOffset = 0; stageOffset < _stageItems.Length; stageOffset++)
               {
                    int itemIndex = fillsFromRight ? _stageItems.Length - 1 - stageOffset : stageOffset;
                    var stageItem = _stageItems[itemIndex];

                    if (stageItem == null)
                    {
                    #if UNITY_EDITOR
                         Debug.Log("<color=red>StageMapRow: stageItem == null</color>");
                    #endif
                         continue;
                    }

                    int stageIndex = _stageRowIndex * _stageItems.Length + stageOffset;

                    if (SetStageItem(stageItem, stageIndex))
                         unlockedStageCount++;
               }

               SetLineHorizontalState(_lineHorizontal, fillsFromRight, unlockedStageCount);
          }

          private static bool SetStageItem(StageItemView stageItem, int stageIndex)
          {
               bool isValid = stageIndex >= Constant.STAGE_INDEX_MIN && stageIndex <= Constant.STAGE_INDEX_MAX;

               stageItem.gameObject.SetActive(isValid);

               if (!isValid)
                    return false;

               stageItem.SetView(stageIndex);

               return true;
          }

          private static void SetLineHorizontalState(Image lineHorizontal, bool isRight, int stageActiveCount = 4,
               bool                                        isActive = true)
          {
               if (lineHorizontal == null)
               {
               #if UNITY_EDITOR
                    Debug.Log("<color=red>StageMapRow: lineHorizontal == null</color>");
               #endif
                    return;
               }

               int maxStageCount      = Mathf.Max(1, Constant.STAGE_MAP_MAX_PER_ROW);
               int clampedActiveCount = Mathf.Clamp(stageActiveCount, 0, maxStageCount);
               int connectionCount    = Mathf.Max(0, clampedActiveCount - 1);
               int maxConnectionCount = Mathf.Max(1, maxStageCount      - 1);

               lineHorizontal.gameObject.SetActive(isActive && connectionCount > 0);
               lineHorizontal.type       = Image.Type.Filled;
               lineHorizontal.fillMethod = Image.FillMethod.Horizontal;
               lineHorizontal.fillOrigin = isRight ? (int)Image.OriginHorizontal.Right : (int)Image.OriginHorizontal.Left;
               lineHorizontal.fillAmount = connectionCount / (float)maxConnectionCount;
          }
     }
}
