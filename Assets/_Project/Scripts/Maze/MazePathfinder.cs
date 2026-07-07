namespace _Project.Scripts.Maze
{
     using System.Collections.Generic;
     using UnityEngine;

     public sealed class MazePathfinder
     {
          private static readonly Vector2Int InvalidCell = new(-1, -1);

          private readonly Queue<Vector2Int> _queue          = new();
          private readonly List<Vector2Int>  _pathBuffer     = new();
          private readonly Vector2Int[]      _neighborBuffer = new Vector2Int[4];

          private bool[,]       _visited;
          private Vector2Int[,] _parent;
          private int[,]        _distance;
          private int           _bufferWidth;
          private int           _bufferHeight;

          public bool TryFindPath(MazeData mazeData, Vector2Int start, Vector2Int target, out List<Vector2Int> path)
          {
               path = null;

               if (mazeData == null)
                    return false;

               if (!mazeData.IsInside(start.x, start.y))
                    return false;

               if (!mazeData.IsInside(target.x, target.y))
                    return false;

               EnsureBuffers(mazeData.Width, mazeData.Height);
               ResetVisited();
               ResetParent();

               _queue.Clear();
               _queue.Enqueue(start);
               _visited[start.x, start.y] = true;

               bool found = false;

               while (_queue.Count > 0)
               {
                    var current = _queue.Dequeue();

                    if (current == target)
                    {
                         found = true;

                         break;
                    }

                    int neighborCount = FillWalkableNeighbors(mazeData, current, _neighborBuffer);

                    for (int i = 0; i < neighborCount; i++)
                    {
                         var next = _neighborBuffer[i];

                         if (_visited[next.x, next.y])
                              continue;

                         _visited[next.x, next.y] = true;
                         _parent[next.x, next.y]  = current;
                         _queue.Enqueue(next);
                    }
               }

               if (!found)
                    return false;

               path = ReconstructPath(start, target);

               return path is
               {
                    Count: > 0
               };
          }

          public int[,] BuildDistanceMap(MazeData mazeData, Vector2Int start)
          {
               EnsureBuffers(mazeData.Width, mazeData.Height);
               ResetDistance();

               if (!mazeData.IsInside(start.x, start.y))
                    return _distance;

               _queue.Clear();
               _queue.Enqueue(start);
               _distance[start.x, start.y] = 0;

               while (_queue.Count > 0)
               {
                    var current         = _queue.Dequeue();
                    int currentDistance = _distance[current.x, current.y];
                    int neighborCount   = FillWalkableNeighbors(mazeData, current, _neighborBuffer);

                    for (int i = 0; i < neighborCount; i++)
                    {
                         var next = _neighborBuffer[i];

                         if (_distance[next.x, next.y] >= 0)
                              continue;

                         _distance[next.x, next.y] = currentDistance + 1;
                         _queue.Enqueue(next);
                    }
               }

               return _distance;
          }

          private void EnsureBuffers(int width, int height)
          {
               if (_visited != null && _bufferWidth == width && _bufferHeight == height)
                    return;

               _visited      = new bool[width, height];
               _parent       = new Vector2Int[width, height];
               _distance     = new int[width, height];
               _bufferWidth  = width;
               _bufferHeight = height;
          }

          private void ResetVisited()
          {
               for (int y = 0; y < _bufferHeight; y++)
               {
                    for (int x = 0; x < _bufferWidth; x++)
                    {
                         _visited[x, y] = false;
                    }
               }
          }

          private void ResetParent()
          {
               for (int y = 0; y < _bufferHeight; y++)
               {
                    for (int x = 0; x < _bufferWidth; x++)
                    {
                         _parent[x, y] = InvalidCell;
                    }
               }
          }

          private void ResetDistance()
          {
               for (int y = 0; y < _bufferHeight; y++)
               {
                    for (int x = 0; x < _bufferWidth; x++)
                    {
                         _distance[x, y] = -1;
                    }
               }
          }

          private List<Vector2Int> ReconstructPath(Vector2Int start, Vector2Int target)
          {
               _pathBuffer.Clear();

               var current = target;
               _pathBuffer.Add(current);

               while (current != start)
               {
                    current = _parent[current.x, current.y];

                    if (current == InvalidCell)
                         return null;

                    _pathBuffer.Add(current);
               }

               _pathBuffer.Reverse();

               return _pathBuffer;
          }

          private static int FillWalkableNeighbors(MazeData mazeData, Vector2Int position, Vector2Int[] neighbors)
          {
               int count = 0;
               int x     = position.x;
               int y     = position.y;

               var cell = mazeData.GetCell(x, y);

               if (mazeData.IsInside(x, y - 1))
               {
                    var topCell = mazeData.GetCell(x, y - 1);

                    if (!cell.TopWall && !topCell.BottomWall)
                         neighbors[count++] = new Vector2Int(x, y - 1);
               }

               if (mazeData.IsInside(x + 1, y))
               {
                    var rightCell = mazeData.GetCell(x + 1, y);

                    if (!cell.RightWall && !rightCell.LeftWall)
                         neighbors[count++] = new Vector2Int(x + 1, y);
               }

               if (mazeData.IsInside(x, y + 1))
               {
                    var bottomCell = mazeData.GetCell(x, y + 1);

                    if (!cell.BottomWall && !bottomCell.TopWall)
                         neighbors[count++] = new Vector2Int(x, y + 1);
               }

               if (mazeData.IsInside(x - 1, y))
               {
                    var leftCell = mazeData.GetCell(x - 1, y);

                    if (!cell.LeftWall && !leftCell.RightWall)
                         neighbors[count++] = new Vector2Int(x - 1, y);
               }

               return count;
          }
     }

     public sealed class MazeTargetSelector
     {
          private readonly MazePathfinder   _pathfinder;
          private readonly List<Vector2Int> _candidates = new();

          public MazeTargetSelector(MazePathfinder pathfinder) { _pathfinder = pathfinder; }

          public Vector2Int SelectTarget(MazeData mazeData, Vector2Int startCell, int seed, int minDistance)
          {
               var distanceMap = _pathfinder.BuildDistanceMap(mazeData, startCell);

               _candidates.Clear();

               for (int y = 0; y < mazeData.Height; y++)
               {
                    for (int x = 0; x < mazeData.Width; x++)
                    {
                         var cell = new Vector2Int(x, y);

                         if (cell == startCell)
                              continue;

                         int distance = distanceMap[x, y];

                         if (distance >= minDistance)
                              _candidates.Add(cell);
                    }
               }

               if (_candidates.Count == 0)
                    FillFallbackCandidates(mazeData, startCell, distanceMap);

               if (_candidates.Count == 0)
                    return startCell;

               return _candidates[GetDeterministicIndex(seed, _candidates.Count)];
          }

          private void FillFallbackCandidates(MazeData mazeData, Vector2Int startCell, int[,] distanceMap)
          {
               _candidates.Clear();

               for (int y = 0; y < mazeData.Height; y++)
               {
                    for (int x = 0; x < mazeData.Width; x++)
                    {
                         var cell = new Vector2Int(x, y);

                         if (cell == startCell)
                              continue;

                         if (distanceMap[x, y] > 0)
                              _candidates.Add(cell);
                    }
               }
          }

          private static int GetDeterministicIndex(int seed, int maxExclusive)
          {
               unchecked
               {
                    uint value = (uint)seed;
                    value ^= value << 13;
                    value ^= value >> 17;
                    value ^= value << 5;

                    return (int)(value % (uint)maxExclusive);
               }
          }
     }
}
