namespace _Project.Scripts.Shared.Pooling
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Auto-component được gắn vào mọi pooled instance.
    /// Lưu state, back-reference tới pool nguồn, và cung cấp Recycle() API.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PoolItem : MonoBehaviour
    {
        // ── Public state ───────────────────────────────────────────────────
        public PoolState  State        { get; internal set; } = PoolState.Pooled;

        /// <summary>Prefab gốc – dùng làm key tra cứu pool.</summary>
        public GameObject SourcePrefab { get; internal set; }

        internal PrefabPool Pool { get; set; }

        // ── API ────────────────────────────────────────────────────────────

        /// <summary>Trả object về pool ngay lập tức.</summary>
        public void Recycle()
        {
            if (State == PoolState.Pooled || State == PoolState.Recyclable) return;

            State = PoolState.Recyclable;
            Pool?.Recycle(this);
        }

        /// <summary>Trả object về pool sau <paramref name="delay"/> giây.
        /// Hữu ích khi cần chờ particle / animation kết thúc.</summary>
        public void RecycleAfter(float delay)
        {
            if (State == PoolState.Pooled || State == PoolState.Recyclable) return;

            State = PoolState.Recyclable;          // chặn double-recycle ngay
            StartCoroutine(DelayedRecycle(delay));
        }

        // ── Internals ──────────────────────────────────────────────────────
        private IEnumerator DelayedRecycle(float delay)
        {
            yield return new WaitForSeconds(delay);
            Pool?.Recycle(this);
        }
    }
}
