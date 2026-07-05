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
     }
}
