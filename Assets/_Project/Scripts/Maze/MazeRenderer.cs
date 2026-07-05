namespace _Project.Scripts.Maze
{
     using UnityEngine;

     public sealed class MazeRenderer : MonoBehaviour
     {
          [Header("Prefab")]
          [SerializeField] private SpriteRenderer _spritePrefab;

          [Header("Roots")]
          [SerializeField] private Transform _wallRoot;

          [Header("Layout")]
          [SerializeField] private float _cellSize = 0.7f;

          [SerializeField] private float   _wallThickness = 0.08f;
          [SerializeField] private Vector3 _origin;

          [Header("Colors")]
          [SerializeField] private Color _wallColor = new Color(0.36f, 0.22f, 0.08f, 1f);

          public void Render(MazeData mazeData)
          {
               Clear();

               _origin = CalculateOrigin(mazeData.Width, mazeData.Height);

               for (int y = 0; y < mazeData.Height; y++)
               {
                    for (int x = 0; x < mazeData.Width; x++)
                    {
                         var cell = mazeData.GetCell(x, y);

                         if (cell.TopWall)
                              RenderHorizontalWall(GetTopWallPosition(x, y));

                         if (cell.LeftWall)
                              RenderVerticalWall(GetLeftWallPosition(x, y));

                         if (x == mazeData.Width - 1 && cell.RightWall)
                              RenderVerticalWall(GetRightWallPosition(x, y));

                         if (y == mazeData.Height - 1 && cell.BottomWall)
                              RenderHorizontalWall(GetBottomWallPosition(x, y));
                    }
               }
          }

          public Vector3 GetCellCenter(Vector2Int cellPosition) { return GetCellCenter(cellPosition.x, cellPosition.y); }

          Vector3 GetCellCenter(int x, int y)
          {
               return _origin + new Vector3(x * _cellSize + _cellSize * 0.5f, -y * _cellSize - _cellSize * 0.5f, 0f);
          }

          private Vector3 CalculateOrigin(int width, int height)
          {
               float mazeWidth  = width  * _cellSize;
               float mazeHeight = height * _cellSize;

               return new Vector3(-mazeWidth * 0.5f, mazeHeight * 0.5f, 0f);
          }

          private void RenderHorizontalWall(Vector3 position)
          {
               var wall = CreateSprite(_wallRoot, "HorizontalWall", _wallColor, 2);
               wall.transform.position   = position;
               wall.transform.localScale = new Vector3(_cellSize + _wallThickness, _wallThickness, 1f);
          }

          private void RenderVerticalWall(Vector3 position)
          {
               var wall = CreateSprite(_wallRoot, "VerticalWall", _wallColor, 2);
               wall.transform.position   = position;
               wall.transform.localScale = new Vector3(_wallThickness, _cellSize + _wallThickness, 1f);
          }

          private Vector3 GetTopWallPosition(int x, int y)
          {
               return GetCellCenter(x, y) + new Vector3(0f, _cellSize * 0.5f, 0f);
          }

          private Vector3 GetBottomWallPosition(int x, int y)
          {
               return GetCellCenter(x, y) + new Vector3(0f, -_cellSize * 0.5f, 0f);
          }

          private Vector3 GetLeftWallPosition(int x, int y)
          {
               return GetCellCenter(x, y) + new Vector3(-_cellSize * 0.5f, 0f, 0f);
          }

          private Vector3 GetRightWallPosition(int x, int y)
          {
               return GetCellCenter(x, y) + new Vector3(_cellSize * 0.5f, 0f, 0f);
          }

          private SpriteRenderer CreateSprite(Transform parent, string objectName, Color color, int sortingOrder)
          {
               var sprite = Instantiate(_spritePrefab, parent);
               sprite.name         = objectName;
               sprite.color        = color;
               sprite.sortingOrder = sortingOrder;

               return sprite;
          }

          void Clear() { ClearRoot(_wallRoot); }

          private static void ClearRoot(Transform root)
          {
               for (int i = root.childCount - 1; i >= 0; i--)
               {
                    DestroyImmediate(root.GetChild(i).gameObject);
               }
          }
     }
}
