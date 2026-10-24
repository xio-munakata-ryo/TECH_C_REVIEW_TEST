using UnityEngine;

namespace TowerDefense
{
    public sealed class BuildPad : MonoBehaviour
    {
        public bool IsOccupied { get; private set; }

        /// <summary>Reserve a construction pad for the lifetime of this match.</summary>
        public void Occupy()
        {
            IsOccupied = true;
        }
    }
}
