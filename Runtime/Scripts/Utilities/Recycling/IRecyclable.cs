using UnityEngine;

namespace Utilities.Recycling
{
    public interface IRecyclable
    {
        GameObject gameObject { get; }
        Transform transform { get; }

        bool IsRecycled { get; set; }
        void OnRecycled();
    }
}