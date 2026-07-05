namespace _Project.Scripts.Maze
{
     using Sirenix.OdinInspector;
     using UnityEngine;

     public sealed class MazeController : MonoBehaviour
     {
          [Header("Maze Settings")]
          [SerializeField] private int _width = 10;

          [SerializeField] private int _height = 13;

          [Header("References")]
          [SerializeField] private MazeRenderer _mazeRenderer;

          private const int BaseSeed            = 1818;
          private const int StageSeedMultiplier = 36;

          private readonly MazeGenerator _mazeGenerator = new();

          private MazeData _currentMazeData;
          private int      _currentStageId;

          public MazeData CurrentMazeData => _currentMazeData;
          public int      CurrentStageId  => _currentStageId;

          // private void Start()
          // {
          //      // Test tạm trong scene.
          //      GenerateStageMaze(1);
          // }

          [Button("Generate Maze")]
          public void GenerateStageMaze(int stageId)
          {
               _currentStageId = stageId;

               int seed = GetSeedFromStage(stageId);

               _currentMazeData = _mazeGenerator.Generate(_width, _height, seed);
               _mazeRenderer.Render(_currentMazeData);
          }

          private static int GetSeedFromStage(int stageId) { return BaseSeed + stageId * StageSeedMultiplier; }
     }
}
