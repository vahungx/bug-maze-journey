namespace _Project.Scripts.Maze
{
     using System.Collections.Generic;
     using UnityEngine;

     public sealed class MazeGenerator
     {
          private readonly static Vector2Int[] Directions =
          {
               new Vector2Int(0, -1), // Top
               new Vector2Int(1, 0),  // Right
               new Vector2Int(0, 1),  // Bottom
               new Vector2Int(-1, 0), // Left
          };

          public MazeData Generate(int width, int height, int seed)
          {
               var cells = CreateCells(width, height);

               var mazeData = new MazeData(width, height, cells)
               {
                    StartCell = Vector2Int.zero
               };

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

          private static void GenerateByDfs(MazeData mazeData, int seed)
          {
               var random = new System.Random(seed);
               var stack  = new Stack<MazeCell>();
               var unvisitedNeighbors = new MazeCell[4];

               var start = mazeData.GetCell(0, 0);
               start.Visited = true;
               stack.Push(start);

               while (stack.Count > 0)
               {
                    var current       = stack.Peek();
                    int neighborCount = FillUnvisitedNeighbors(mazeData, current, unvisitedNeighbors);

                    if (neighborCount == 0)
                    {
                         stack.Pop();

                         continue;
                    }

                    var next = unvisitedNeighbors[random.Next(neighborCount)];

                    RemoveWallBetween(current, next);
                    next.Visited = true;
                    stack.Push(next);
               }
          }

          private static int FillUnvisitedNeighbors(MazeData mazeData, MazeCell cell, MazeCell[] neighbors)
          {
               int count = 0;

               foreach (var direction in Directions)
               {
                    int nextX = cell.X + direction.x;
                    int nextY = cell.Y + direction.y;

                    if (!mazeData.IsInside(nextX, nextY))
                         continue;

                    var neighbor = mazeData.GetCell(nextX, nextY);

                    if (!neighbor.Visited)
                         neighbors[count++] = neighbor;
               }

               return count;
          }

          private static void RemoveWallBetween(MazeCell current, MazeCell next)
          {
               int dx = next.X - current.X;
               int dy = next.Y - current.Y;

               if (dx == 1)
               {
                    current.RightWall = false;
                    next.LeftWall     = false;
               }
               else if (dx == -1)
               {
                    current.LeftWall = false;
                    next.RightWall   = false;
               }
               else if (dy == 1)
               {
                    current.BottomWall = false;
                    next.TopWall       = false;
               }
               else if (dy == -1)
               {
                    current.TopWall = false;
                    next.BottomWall = false;
               }
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
