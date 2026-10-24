using System;
using UnityEngine;

namespace TowerDefense
{
    [CreateAssetMenu(menuName = "Tower Defense/Game Settings")]
    public sealed class DefenseSettings : ScriptableObject
    {
        [Serializable]
        public sealed class Wave
        {
            [Min(1)] public int Count = 8;
            [Min(1)] public int Health = 40;
            [Min(0.1f)] public float Speed = 1.8f;
            [Min(0.1f)] public float SpawnInterval = 0.9f;
            [Min(0)] public int Reward = 12;
            [Min(0)] public int ClearBonus = 35;
        }

        [SerializeField, Min(1)] private int _startingCredits = 180;
        [SerializeField, Min(1)] private int _baseHealth = 12;
        [SerializeField] private TowerDefinition[] _towers;
        [SerializeField] private DefenseEnemy _enemyPrefab;
        [SerializeField] private GameObject _tilePrefab;
        [SerializeField] private GameObject _basePrefab;
        [SerializeField] private Material _groundMaterial;
        [SerializeField] private Material _pathMaterial;
        [SerializeField] private Material _padMaterial;
        [SerializeField] private Material _shotMaterial;
        [SerializeField] private Wave[] _waves;

        public int StartingCredits => _startingCredits;
        public int BaseHealth => _baseHealth;
        public TowerDefinition[] Towers => _towers;
        public DefenseEnemy EnemyPrefab => _enemyPrefab;
        public GameObject TilePrefab => _tilePrefab;
        public GameObject BasePrefab => _basePrefab;
        public Material GroundMaterial => _groundMaterial;
        public Material PathMaterial => _pathMaterial;
        public Material PadMaterial => _padMaterial;
        public Material ShotMaterial => _shotMaterial;
        public Wave[] Waves => _waves;

        /// <summary>Validate all serialized dependencies and wave values.</summary>
        public bool IsValid()
        {
            if (_startingCredits <= 0 || _baseHealth <= 0 || _towers == null || _towers.Length == 0
                || _enemyPrefab == null || _tilePrefab == null || _basePrefab == null
                || _groundMaterial == null || _pathMaterial == null || _padMaterial == null
                || _shotMaterial == null || _waves == null || _waves.Length == 0)
                return false;

            foreach (TowerDefinition tower in _towers)
                if (tower == null || !tower.IsValid())
                    return false;

            foreach (Wave wave in _waves)
                if (wave == null || wave.Count <= 0 || wave.Health <= 0 || wave.Speed <= 0f
                    || wave.SpawnInterval <= 0f || wave.Reward < 0 || wave.ClearBonus < 0)
                    return false;

            return true;
        }
    }
}
