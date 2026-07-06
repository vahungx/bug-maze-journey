namespace _Project.Scripts.Maze
{
     using System.Collections.Generic;
     using UnityEngine;
     using Random = System.Random;

     public sealed class MazePathfinder
     {
          private static readonly Vector2Int InvalidCell = new(-1, -1);

          public bool TryFindPath(MazeData mazeData, Vector2Int start, Vector2Int target, out List<Vector2Int> path)
          {
               path = null;

               if (mazeData == null)
                    return false;

               if (!mazeData.IsInside(start.x, start.y))
                    return false;

               if (!mazeData.IsInside(target.x, target.y))
                    return false;

               var visited = new bool[mazeData.Width, mazeData.Height];
               var parent  = new Vector2Int[mazeData.Width, mazeData.Height];

               for (int y = 0; y < mazeData.Height; y++)
               {
                    for (int x = 0; x < mazeData.Width; x++)
                    {
                         parent[x, y] = InvalidCell;
                    }
               }

               var queue = new Queue<Vector2Int>();
               queue.Enqueue(start);
               visited[start.x, start.y] = true;

               bool found = false;

               while (queue.Count > 0)
               {
                    var current = queue.Dequeue();

                    if (current == target)
                    {
                         found = true;

                         break;
                    }

                    foreach (var next in GetWalkableNeighbors(mazeData, current))
                    {
                         if (visited[next.x, next.y])
                              continue;

                         visited[next.x, next.y] = true;
                         parent[next.x, next.y]  = current;
                         queue.Enqueue(next);
                    }
               }

               if (!found)
                    return false;

               path = ReconstructPath(parent, start, target);

               return path is
               {
                    Count: > 0
               };
          }

          public int[,] BuildDistanceMap(MazeData mazeData, Vector2Int start)
          {
               var distance = new int[mazeData.Width, mazeData.Height];

               for (int y = 0; y < mazeData.Height; y++)
               {
                    for (int x = 0; x < mazeData.Width; x++)
                    {
                         distance[x, y] = -1;
                    }
               }

               if (!mazeData.IsInside(start.x, start.y))
                    return distance;

               var queue = new Queue<Vector2Int>();
               queue.Enqueue(start);
               distance[start.x, start.y] = 0;

               while (queue.Count > 0)
               {
                    var current         = queue.Dequeue();
                    int currentDistance = distance[current.x, current.y];

                    foreach (var next in GetWalkableNeighbors(mazeData, current))
                    {
                         if (distance[next.x, next.y] >= 0)
                              continue;

                         distance[next.x, next.y] = currentDistance + 1;
                         queue.Enqueue(next);
                    }
               }

               return distance;
          }

          private static List<Vector2Int> ReconstructPath(Vector2Int[,] parent, Vector2Int start, Vector2Int target)
          {
               var path    = new List<Vector2Int>();
               var current = target;

               path.Add(current);

               while (current != start)
               {
                    current = parent[current.x, current.y];

                    if (current == InvalidCell)
                         return null;

                    path.Add(current);
               }

               path.Reverse();

               return path;
          }

          private static IEnumerable<Vector2Int> GetWalkableNeighbors(MazeData mazeData, Vector2Int position)
          {
               int x = position.x;
               int y = position.y;

               var cell = mazeData.GetCell(x, y);

               // Top
               if (mazeData.IsInside(x, y - 1))
               {
                    var topCell = mazeData.GetCell(x, y - 1);

                    if (!cell.TopWall && !topCell.BottomWall)
                         yield return new Vector2Int(x, y - 1);
               }

               // Right
               if (mazeData.IsInside(x + 1, y))
               {
                    var rightCell = mazeData.GetCell(x + 1, y);

                    if (!cell.RightWall && !rightCell.LeftWall)
                         yield return new Vector2Int(x + 1, y);
               }

               // Bottom
               if (mazeData.IsInside(x, y + 1))
               {
                    var bottomCell = mazeData.GetCell(x, y + 1);

                    if (!cell.BottomWall && !bottomCell.TopWall)
                         yield return new Vector2Int(x, y + 1);
               }

               // Left
               if (mazeData.IsInside(x - 1, y))
               {
                    var leftCell = mazeData.GetCell(x - 1, y);

                    if (!cell.LeftWall && !leftCell.RightWall)
                         yield return new Vector2Int(x - 1, y);
               }
          }
     }

     public sealed class MazeTargetSelector
     {
          private readonly MazePathfinder _pathfinder;

          public MazeTargetSelector(MazePathfinder pathfinder) { _pathfinder = pathfinder; }

          public Vector2Int SelectTarget(MazeData mazeData, Vector2Int startCell, int seed, int minDistance)
          {
               var distanceMap = _pathfinder.BuildDistanceMap(mazeData, startCell);

               var candidates = new List<Vector2Int>();

               for (int y = 0; y < mazeData.Height; y++)
               {
                    for (int x = 0; x < mazeData.Width; x++)
                    {
                         var cell = new Vector2Int(x, y);

                         if (cell == startCell)
                              continue;

                         int distance = distanceMap[x, y];

                         if (distance >= minDistance)
                              candidates.Add(cell);
                    }
               }

               if (candidates.Count == 0)
               {
                    candidates = GetFallbackCandidates(mazeData, startCell, distanceMap);
               }

               if (candidates.Count == 0)
                    return startCell;

               var random = new Random(seed);

               return candidates[random.Next(candidates.Count)];
          }

          private static List<Vector2Int> GetFallbackCandidates(MazeData mazeData, Vector2Int startCell, int[,] distanceMap)
          {
               var result = new List<Vector2Int>();

               for (int y = 0; y < mazeData.Height; y++)
               {
                    for (int x = 0; x < mazeData.Width; x++)
                    {
                         var cell = new Vector2Int(x, y);

                         if (cell == startCell)
                              continue;

                         if (distanceMap[x, y] > 0)
                              result.Add(cell);
                    }
               }

               return result;
          }
     }
}
