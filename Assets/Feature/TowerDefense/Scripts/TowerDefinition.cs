using UnityEngine;

namespace TowerDefense
{
    [CreateAssetMenu(menuName = "Tower Defense/Tower Definition")]
    public sealed class TowerDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName = "Pulse";
        [SerializeField, Min(1)] private int _cost = 60;
        [SerializeField, Min(0.1f)] private float _range = 3.2f;
        [SerializeField, Min(0.05f)] private float _attackInterval = 0.6f;
        [SerializeField, Min(1)] private int _damage = 14;
        [SerializeField] private DefenseTower _prefab;

        public string DisplayName => _displayName;
        public int Cost => _cost;
        public float Range => _range;
        public float AttackInterval => _attackInterval;
        public int Damage => _damage;
        public DefenseTower Prefab => _prefab;

        /// <summary>Reject unusable tower settings before starting a match.</summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(_displayName) && _cost > 0 && _range > 0f
                && _attackInterval > 0f && _damage > 0 && _prefab != null;
        }
    }
}
