namespace _Project.Scripts.Maze
{
     internal struct MazeRandom
     {
          private const uint NonZeroFallback = 0xA3C59AC3u;

          private uint _state;

          public MazeRandom(int seed)
          {
               _state = Mix((uint)seed);

               if (_state == 0)
                    _state = NonZeroFallback;
          }

          public int Next(int maxExclusive)
          {
               if (maxExclusive <= 1)
                    return 0;

               return (int)(((ulong)NextUInt() * (uint)maxExclusive) >> 32);
          }

          private uint NextUInt()
          {
               uint value = _state;
               value ^= value << 13;
               value ^= value >> 17;
               value ^= value << 5;
               _state = value;

               return value;
          }

          private static uint Mix(uint value)
          {
               value ^= value >> 16;
               value *= 0x7FEB352Du;
               value ^= value >> 15;
               value *= 0x846CA68Bu;
               value ^= value >> 16;

               return value;
          }
     }
}
