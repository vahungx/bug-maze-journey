namespace _Project.Scripts.Maze
{
     using System;
     using UnityEngine;

     public sealed class MazeGenerator
     {
          private const int InitialStackCapacity = 256;

          private static readonly Vector2Int[] Directions =
          {
               new Vector2Int(0, -1), // Top
               new Vector2Int(1, 0),  // Right
               new Vector2Int(0, 1),  // Bottom
               new Vector2Int(-1, 0), // Left
          };

          private readonly MazeCell[] _unvisitedNeighborBuffer = new MazeCell[4];

          private MazeCell[] _stackBuffer = new MazeCell[InitialStackCapacity];

          public MazeData Generate(int width, int height, int seed)
          {
               if (width <= 0)
                    throw new ArgumentOutOfRangeException(nameof(width), width, "Maze width must be positive.");

               if (height <= 0)
                    throw new ArgumentOutOfRangeException(nameof(height), height, "Maze height must be positive.");

               var cells = CreateCells(width, height);

               var mazeData = new MazeData(width, height, cells)
               {
                    StartCell = Vector2Int.zero
               };

               EnsureStackCapacity(width * height);
               GenerateByDfs(mazeData, seed);
               ClearVisited(mazeData);

               return mazeData;
          }

          private static MazeCell[,] CreateCells(int width, int height)
          {
               var cells = new MazeCell[width, height];

               for (int y = 0; y < height; y++)
               {
                    for (int x = 0; x < width; x++)
                    {
                         cells[x, y] = new MazeCell(x, y);
                    }
               }

               return cells;
          }

          private void GenerateByDfs(MazeData mazeData, int seed)
          {
               var random = new MazeRandom(seed);

               int stackCount = 0;

               MazeCell start = mazeData.GetCell(0, 0);
               start.Visited = true;
               _stackBuffer[stackCount++] = start;

               while (stackCount > 0)
               {
                    MazeCell current = _stackBuffer[stackCount - 1];
                    int neighborCount = FillUnvisitedNeighbors(mazeData, current);

                    if (neighborCount == 0)
                    {
                         stackCount--;

                         continue;
                    }

                    MazeCell next = _unvisitedNeighborBuffer[random.Next(neighborCount)];

                    mazeData.TryOpenPassage(current.X, current.Y, next.X, next.Y);
                    next.Visited = true;
                    _stackBuffer[stackCount++] = next;
               }
          }

          private int FillUnvisitedNeighbors(MazeData mazeData, MazeCell cell)
          {
               int count = 0;

               for (int i = 0; i < Directions.Length; i++)
               {
                    Vector2Int direction = Directions[i];
                    int nextX = cell.X + direction.x;
                    int nextY = cell.Y + direction.y;

                    if (!mazeData.IsInside(nextX, nextY))
                         continue;

                    MazeCell neighbor = mazeData.GetCell(nextX, nextY);

                    if (!neighbor.Visited)
                         _unvisitedNeighborBuffer[count++] = neighbor;
               }

               return count;
          }

          private void EnsureStackCapacity(int requiredCapacity)
          {
               if (_stackBuffer.Length >= requiredCapacity)
                    return;

               int newCapacity = _stackBuffer.Length;

               while (newCapacity < requiredCapacity)
                    newCapacity *= 2;

               Array.Resize(ref _stackBuffer, newCapacity);
          }

          private static void ClearVisited(MazeData mazeData)
          {
               for (int y = 0; y < mazeData.Height; y++)
               {
                    for (int x = 0; x < mazeData.Width; x++)
                    {
                         mazeData.Cells[x, y].Visited = false;
                    }
               }
          }
     }
}
