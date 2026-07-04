namespace _Project.Scripts.Map
{
     using System;
     using _Project.Scripts.LocalData;
     using _Project.Scripts.Shared.Services;
     using _Project.Scripts.Shared.Services.LocalData;
     using TMPro;
     using UnityEngine;

     /// <summary>
     /// Là view item của 1 stage trong map, có thể là button hoặc image, tùy vào thiết kế UI
     /// </summary>
     public class StageItemView : MonoBehaviour
     {
          [SerializeField] GameObject      _tutorialTitleObject;
          [SerializeField] TextMeshProUGUI _levelText;
          [SerializeField] GameObject      _lockObject;
          [SerializeField] StageStars      _stageStars;

          int               _index;
          MapLocalDataModel _mapLocalDataModel;

          void Awake() { _mapLocalDataModel = ServiceLocator.Resolve<ILocalDataService>().Get<MapLocalData>().Model; }

          public void SetView(int index, int stars)
          {
               if (_mapLocalDataModel == null)
               {
               #if UNITY_EDITOR
                    Debug.Log("<color=yellow>MapLocalDataModel could be not initial</color>");

               #endif
                    return;
               }

               _index = index;

               // Nếu stage chưa có trong dữ liệu local và currentLevel < index, thì thêm stage vào dữ liệu local
               if (!_mapLocalDataModel.ContainsStage(_index) && _mapLocalDataModel.currentLevel >= _index)
               {
                    _mapLocalDataModel.AddOrUpdateStageStars(_index, stars);
               }
               else
               {
                    stars = _mapLocalDataModel.GetStarsForStage(_index);
               }

               _stageStars.SetStageStar(_mapLocalDataModel.currentLevel < _index ? 0 : stars);

               if (_index == 0)
               {
                    _tutorialTitleObject.SetActive(true);
                    _levelText.gameObject.SetActive(false);
               }
               else
               {
                    _tutorialTitleObject.SetActive(false);
                    _levelText.gameObject.SetActive(true);
               }

               _levelText.text = (_index + 1).ToString();
               _lockObject.SetActive(_mapLocalDataModel.currentLevel < _index);
          }
     }
}
