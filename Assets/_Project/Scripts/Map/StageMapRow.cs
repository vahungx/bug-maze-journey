namespace _Project.Scripts.Map
{
     using UnityEngine;

     public class StageMapRow : MonoBehaviour
     {
          [SerializeField] GameObject      _rightLine;
          [SerializeField] GameObject      _leftLine;
          [SerializeField] StageItemView[] _stageItems;

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

               if (_stageRowIndex % 2 == 0) //index stage left to right
               {
                    for (int i = 0; i < _stageItems.Length; i++)
                    {
                         if (_stageItems[i] == null)
                         {
                         #if UNITY_EDITOR
                              Debug.Log("<color=red>StageMapRow: _stageItems[i] == null</color>");
                         #endif
                              continue;
                         }

                         _stageItems[i].SetView(_stageRowIndex * _stageItems.Length + i, Random.Range(1, 4));
                    }
               }
               else //index stage right to left 
               {
                    for (int i = _stageItems.Length - 1; i >= 0; i--)
                    {
                         if (_stageItems[i] == null)
                         {
                         #if UNITY_EDITOR
                              Debug.Log("<color=red>StageMapRow: _stageItems[i] == null</color>");
                         #endif
                              continue;
                         }

                         int stageIndex = _stageRowIndex * _stageItems.Length + (_stageItems.Length - 1 - i);

                         _stageItems[i].SetView(stageIndex, Random.Range(1, 4));
                    }
               }
          }
     }
}
