namespace _Project.Scripts.Scenes
{
     using System;
     using _Project.Scripts.Maze;
     using UnityEngine;

     public class Maze : MonoBehaviour
     {
          public static Maze Instance;

          void Awake()
          {
               if (Instance != null && Instance != this)
               {
                    Destroy(this.gameObject);
               }
               else
               {
                    Instance = this;
               }
          }

          [SerializeField] private MazeController _mazeController;

          public void Init(int stageId)
          {
               _mazeController.GenerateStageMaze(stageId);
          }
     }
}
