namespace _Project.Scripts.Scenes
{
     using System;
     using _Project.Scripts.LocalData;
     using _Project.Scripts.Maze;
     using _Project.Scripts.Shared.Services;
     using _Project.Scripts.Shared.Services.LocalData;
     using UnityEngine;

     public class Maze : MonoBehaviour
     {
          public static Maze Instance;

          [SerializeField] private MazeController _mazeController;

          public  MazeController    MazeController    => _mazeController;

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

          public void Init(int stageId)
          {
               _mazeController.GenerateStageMaze(stageId);
          }
     }
}
