namespace _Project.Scripts.Maze
{
     using UnityEngine;

     public class TargetController : MonoBehaviour
     {
          [Header("Visual")]
          [SerializeField] private GameObject _redStateObject;

          [SerializeField] private GameObject _blueStateObject;

          public void SetTarget(Vector2Int targetCell, MazeRenderer mazeRenderer)
          {
               ChangeState(true);
               transform.position = mazeRenderer.GetCellCenter(targetCell);
          }

          public void ChangeState(bool isRed)
          {
               _redStateObject.gameObject.SetActive(isRed);
               _blueStateObject.gameObject.SetActive(!isRed);
          }

          public void Hide()
          {
               _redStateObject.gameObject.SetActive(false);
               _blueStateObject.gameObject.SetActive(false);
          }
     }
}
