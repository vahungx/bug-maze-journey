namespace _Project.Scripts.Maze
{
     using System;
     using UnityEngine;

     public sealed class MazeBraider
     {
          private const int InitialBufferCapacity = 256;

          private WallCandidate[] _candidateBuffer = new WallCandidate[InitialBufferCapacity];
          private int[]           _openedPassageCountBuffer = new int[InitialBufferCapacity];

          public int Braid(
               MazeData mazeData,
               int seed,
               int passageCount,
               Vector2Int protectedCell,
               int protectedRadius,
               int maxNewPassagesPerCell,
               bool preventOpenTwoByTwoAreas)
          {
               if (mazeData == null || passageCount <= 0)
                    return 0;

               int cellCount = checked(mazeData.Width * mazeData.Height);
               int maximumCandidateCount = checked(
                    (mazeData.Width - 1) * mazeData.Height +
                    (mazeData.Height - 1) * mazeData.Width);

               EnsureCandidateCapacity(maximumCandidateCount);
               EnsurePassageCountCapacity(cellCount);
               Array.Clear(_openedPassageCountBuffer, 0, cellCount);

               int candidateCount = CollectCandidates(mazeData, protectedCell, protectedRadius);

               if (candidateCount == 0)
                    return 0;

               var random = new MazeRandom(seed);
               ShuffleCandidates(candidateCount, ref random);

               int targetPassageCount = Math.Min(passageCount, candidateCount);
               int openedPassageCount = 0;
               int passageLimitPerCell = maxNewPassagesPerCell > 0
                    ? maxNewPassagesPerCell
                    : int.MaxValue;

               for (int i = 0; i < candidateCount && openedPassageCount < targetPassageCount; i++)
               {
                    WallCandidate candidate = _candidateBuffer[i];
                    int currentIndex = candidate.Y * mazeData.Width + candidate.X;
                    int nextIndex = candidate.NextY * mazeData.Width + candidate.NextX;

                    if (_openedPassageCountBuffer[currentIndex] >= passageLimitPerCell ||
                        _openedPassageCountBuffer[nextIndex] >= passageLimitPerCell)
                         continue;

                    if (preventOpenTwoByTwoAreas && WouldCreateOpenTwoByTwoArea(mazeData, candidate))
                         continue;

                    if (!mazeData.TryOpenPassage(
                             candidate.X,
                             candidate.Y,
                             candidate.NextX,
                             candidate.NextY))
                         continue;

                    _openedPassageCountBuffer[currentIndex]++;
                    _openedPassageCountBuffer[nextIndex]++;
                    openedPassageCount++;
               }

               return openedPassageCount;
          }

          private int CollectCandidates(
               MazeData mazeData,
               Vector2Int protectedCell,
               int protectedRadius)
          {
               int count = 0;

               for (int y = 0; y < mazeData.Height; y++)
               {
                    for (int x = 0; x < mazeData.Width; x++)
                    {
                         MazeCell cell = mazeData.GetCell(x, y);

                         if (x + 1 < mazeData.Width)
                         {
                              MazeCell right = mazeData.GetCell(x + 1, y);

                              if (cell.RightWall &&
                                  right.LeftWall &&
                                  !TouchesProtectedArea(
                                       x,
                                       y,
                                       x + 1,
                                       y,
                                       protectedCell,
                                       protectedRadius))
                              {
                                   _candidateBuffer[count++] = new WallCandidate(x, y, x + 1, y);
                              }
                         }

                         if (y + 1 < mazeData.Height)
                         {
                              MazeCell bottom = mazeData.GetCell(x, y + 1);

                              if (cell.BottomWall &&
                                  bottom.TopWall &&
                                  !TouchesProtectedArea(
                                       x,
                                       y,
                                       x,
                                       y + 1,
                                       protectedCell,
                                       protectedRadius))
                              {
                                   _candidateBuffer[count++] = new WallCandidate(x, y, x, y + 1);
                              }
                         }
                    }
               }

               return count;
          }

          private void ShuffleCandidates(int candidateCount, ref MazeRandom random)
          {
               for (int i = candidateCount - 1; i > 0; i--)
               {
                    int swapIndex = random.Next(i + 1);

                    if (swapIndex == i)
                         continue;

                    WallCandidate temp = _candidateBuffer[i];
                    _candidateBuffer[i] = _candidateBuffer[swapIndex];
                    _candidateBuffer[swapIndex] = temp;
               }
          }

          private static bool TouchesProtectedArea(
               int x,
               int y,
               int nextX,
               int nextY,
               Vector2Int protectedCell,
               int protectedRadius)
          {
               if (protectedRadius < 0)
                    return false;

               return IsInsideProtectedArea(x, y, protectedCell, protectedRadius) ||
                      IsInsideProtectedArea(nextX, nextY, protectedCell, protectedRadius);
          }

          private static bool IsInsideProtectedArea(
               int x,
               int y,
               Vector2Int protectedCell,
               int protectedRadius)
          {
               int distance = Math.Abs(x - protectedCell.x) + Math.Abs(y - protectedCell.y);

               return distance <= protectedRadius;
          }

          private static bool WouldCreateOpenTwoByTwoArea(
               MazeData mazeData,
               WallCandidate candidate)
          {
               if (candidate.Y == candidate.NextY)
               {
                    return IsTwoByTwoAreaOpenAfter(mazeData, candidate.X, candidate.Y - 1, candidate) ||
                           IsTwoByTwoAreaOpenAfter(mazeData, candidate.X, candidate.Y, candidate);
               }

               return IsTwoByTwoAreaOpenAfter(mazeData, candidate.X - 1, candidate.Y, candidate) ||
                      IsTwoByTwoAreaOpenAfter(mazeData, candidate.X, candidate.Y, candidate);
          }

          private static bool IsTwoByTwoAreaOpenAfter(
               MazeData mazeData,
               int topLeftX,
               int topLeftY,
               WallCandidate candidate)
          {
               if (!mazeData.IsInside(topLeftX, topLeftY) ||
                   !mazeData.IsInside(topLeftX + 1, topLeftY + 1))
                    return false;

               return IsPassageOpenAfter(
                           mazeData,
                           topLeftX,
                           topLeftY,
                           topLeftX + 1,
                           topLeftY,
                           candidate) &&
                      IsPassageOpenAfter(
                           mazeData,
                           topLeftX,
                           topLeftY + 1,
                           topLeftX + 1,
                           topLeftY + 1,
                           candidate) &&
                      IsPassageOpenAfter(
                           mazeData,
                           topLeftX,
                           topLeftY,
                           topLeftX,
                           topLeftY + 1,
                           candidate) &&
                      IsPassageOpenAfter(
                           mazeData,
                           topLeftX + 1,
                           topLeftY,
                           topLeftX + 1,
                           topLeftY + 1,
                           candidate);
          }

          private static bool IsPassageOpenAfter(
               MazeData mazeData,
               int x,
               int y,
               int nextX,
               int nextY,
               WallCandidate candidate)
          {
               return candidate.Connects(x, y, nextX, nextY) ||
                      mazeData.IsPassageOpen(x, y, nextX, nextY);
          }

          private void EnsureCandidateCapacity(int requiredCapacity)
          {
               if (_candidateBuffer.Length >= requiredCapacity)
                    return;

               int newCapacity = GetExpandedCapacity(_candidateBuffer.Length, requiredCapacity);
               Array.Resize(ref _candidateBuffer, newCapacity);
          }

          private void EnsurePassageCountCapacity(int requiredCapacity)
          {
               if (_openedPassageCountBuffer.Length >= requiredCapacity)
                    return;

               int newCapacity = GetExpandedCapacity(
                    _openedPassageCountBuffer.Length,
                    requiredCapacity);

               Array.Resize(ref _openedPassageCountBuffer, newCapacity);
          }

          private static int GetExpandedCapacity(int currentCapacity, int requiredCapacity)
          {
               int newCapacity = Math.Max(currentCapacity, 1);

               while (newCapacity < requiredCapacity)
               {
                    if (newCapacity > int.MaxValue / 2)
                         return requiredCapacity;

                    newCapacity *= 2;
               }

               return newCapacity;
          }

          private readonly struct WallCandidate
          {
               public readonly int X;
               public readonly int Y;
               public readonly int NextX;
               public readonly int NextY;

               public WallCandidate(int x, int y, int nextX, int nextY)
               {
                    X = x;
                    Y = y;
                    NextX = nextX;
                    NextY = nextY;
               }

               public bool Connects(int x, int y, int nextX, int nextY)
               {
                    return X == x && Y == y && NextX == nextX && NextY == nextY ||
                           X == nextX && Y == nextY && NextX == x && NextY == y;
               }
          }
     }
}
