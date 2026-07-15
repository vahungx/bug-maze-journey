namespace _Project.Scripts.Maze
{
     using UnityEngine;

     public sealed class MazeCell
     {
          public readonly int X;
          public readonly int Y;

          public bool Visited;

          public bool TopWall    = true;
          public bool RightWall  = true;
          public bool BottomWall = true;
          public bool LeftWall   = true;

          public MazeCell(int x, int y)
          {
               X = x;
               Y = y;
          }
     }

     public sealed class MazeData
     {
          public readonly int         Width;
          public readonly int         Height;
          public readonly MazeCell[,] Cells;

          public Vector2Int StartCell { get; set; }

          public MazeData(int width, int height, MazeCell[,] cells)
          {
               Width  = width;
               Height = height;
               Cells  = cells;
          }

          public bool IsInside(int x, int y) { return x >= 0 && x < Width && y >= 0 && y < Height; }

          public MazeCell GetCell(int x, int y) { return Cells[x, y]; }
     

public bool IsPassageOpen(int x, int y, int nextX, int nextY)
          {
               if (!IsInside(x, y) || !IsInside(nextX, nextY))
                    return false;

               int dx = nextX - x;
               int dy = nextY - y;

               if (dx * dx + dy * dy != 1)
                    return false;

               MazeCell current = GetCell(x, y);
               MazeCell next    = GetCell(nextX, nextY);

               if (dx == 1)
                    return !current.RightWall && !next.LeftWall;

               if (dx == -1)
                    return !current.LeftWall && !next.RightWall;

               if (dy == 1)
                    return !current.BottomWall && !next.TopWall;

               return !current.TopWall && !next.BottomWall;
          }


public bool TryOpenPassage(int x, int y, int nextX, int nextY)
          {
               if (!IsInside(x, y) || !IsInside(nextX, nextY))
                    return false;

               int dx = nextX - x;
               int dy = nextY - y;

               if (dx * dx + dy * dy != 1)
                    return false;

               MazeCell current = GetCell(x, y);
               MazeCell next    = GetCell(nextX, nextY);

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
               else
               {
                    current.TopWall = false;
                    next.BottomWall = false;
               }

               return true;
          }
}
}
