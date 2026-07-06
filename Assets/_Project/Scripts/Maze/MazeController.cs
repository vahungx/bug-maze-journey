namespace _Project.Scripts.Maze
{
     using System.Collections.Generic;
     using _Project.Scripts.LocalData;
     using _Project.Scripts.Shared;
     using _Project.Scripts.Shared.Services;
     using _Project.Scripts.Shared.Services.LocalData;
     using _Project.Scripts.Shared.Services.UI;
     using _Project.Scripts.UI.Popup;
     using Sirenix.OdinInspector;
     using UnityEngine;

     public sealed class MazeController : MonoBehaviour
     {
          private const int BaseSeed            = 1000;
          private const int StageSeedMultiplier = 37;
          private const int TargetSeedOffset    = 9999;
          private const int MinTargetDistance   = 20;

          [Header("Maze Settings")]
          [SerializeField] private int _width = 10;

          [SerializeField] private int _height = 13;

          [Header("References")]
          [SerializeField] private MazeRenderer _mazeRenderer;

          [SerializeField] private PathRenderer     _pathRenderer;
          [SerializeField] private TargetController _targetController;
          [SerializeField] private BugController    _bugController;

          private readonly Vector2Int _startCell = Vector2Int.zero;

          private readonly MazeGenerator  _mazeGenerator = new();
          private readonly MazePathfinder _pathfinder    = new();

          private MazeTargetSelector _targetSelector;

          private MazeData         _currentMazeData;
          private Vector2Int       _currentTargetCell;
          private List<Vector2Int> _currentPath;

          private int _currentStageId;

          private void Awake() { _targetSelector = new MazeTargetSelector(_pathfinder); }

          [Button]
          public void GenerateStageMaze(int stageId)
          {
               _currentStageId = stageId;

               _bugController.KillMove();
               _pathRenderer.Clear();
               _targetController.Hide();
               _currentPath = null;

               int mazeSeed   = GetMazeSeed(stageId);
               int targetSeed = GetTargetSeed(stageId);

               _currentMazeData           = _mazeGenerator.Generate(_width, _height, mazeSeed);
               _currentMazeData.StartCell = _startCell;

               _mazeRenderer.Render(_currentMazeData);

               // Bug start ở top-left.
               _bugController.SetToCell(_startCell, _mazeRenderer);

               // Chọn target reachable và đủ xa.
               _currentTargetCell = _targetSelector.SelectTarget(
                    _currentMazeData, _startCell, targetSeed, MinTargetDistance);

               _targetController.SetTarget(_currentTargetCell, _mazeRenderer);
          }

          [Button]
          public void ShowHint()
          {
               if (_bugController.IsMoving)
                    return;

               FindAndRenderPath();
          }

          [Button]
          public void BugAutoMove()
          {
               if (_bugController.IsMoving)
                    return;

               if (_currentPath == null || _currentPath.Count == 0)
               {
                    bool found = FindAndRenderPath();

                    if (!found)
                         return;
               }

               _bugController.MoveAlongPath(_currentPath, _mazeRenderer, async () =>
               {
                    _targetController.ChangeState(false);
                    ServiceLocator.TryResolve(out ILocalDataService localDataService);
                    
                    localDataService.Get<MapLocalData>().
                                     Model.
                                     AddOrUpdateStageStars(_currentStageId, Constant.STAGE_STARS_MAX);
                    localDataService.Get<MapLocalData>().Model.UnlockNewLevelByStage(_currentStageId);
                    ServiceLocator.TryResolve(out IUIService uiService);

                    await uiService.OpenPopupAsync<WinPopup>(nameof(WinPopup), new WinPopupModal()
                    {
                         StageIndex = _currentStageId,
                         Stars      = Constant.STAGE_STARS_MAX,
                    });
               });
          }

          private bool FindAndRenderPath()
          {
               bool found = _pathfinder.TryFindPath(_currentMazeData, _startCell, _currentTargetCell, out _currentPath);

               if (!found)
               {
                    _currentPath = null;
                    _pathRenderer.Clear();

                    Debug.LogWarning("[MazeController] Path not found.");

                    return false;
               }

               _pathRenderer.RenderPath(_currentPath, _mazeRenderer);

               return true;
          }

          private static int GetMazeSeed(int stageId) { return BaseSeed + stageId * StageSeedMultiplier; }

          private static int GetTargetSeed(int stageId) { return GetMazeSeed(stageId) + TargetSeedOffset; }
     }
}
