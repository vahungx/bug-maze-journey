namespace _Project.Scripts.Shared.Pooling
{
     public enum PoolState
     {
          /// <summary>Nằm trong pool, GameObject inactive.</summary>
          Pooled,

          /// <summary>Đang active, đang được dùng.</summary>
          Spawned,

          /// <summary>Đang chờ trả về pool (vd: đợi animation kết thúc).</summary>
          Recyclable
     }
}
