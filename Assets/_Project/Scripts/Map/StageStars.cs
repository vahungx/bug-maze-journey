namespace _Project.Scripts.Map
{
     using UnityEngine;

     public class StageStars : MonoBehaviour
     {
          [SerializeField] GameObject[] _stageStars;

          public void SetStageStar(int starCount)
          {
               for (int i = 0; i < _stageStars.Length; i++)
               {
                    _stageStars[i].SetActive(i < starCount);
               }
          }
     }
}
