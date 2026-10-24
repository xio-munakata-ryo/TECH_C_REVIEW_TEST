using UnityEngine;

namespace TowerDefense
{
    public sealed class DefenseTower : MonoBehaviour
    {
        [SerializeField] private Transform _turret;
        private DefenseGame _game;
        private TowerDefinition _definition;
        private float _cooldown;
        private LineRenderer _shot;
        private float _shotRemaining;

        /// <summary>Set combat stats and create a short-lived hitscan tracer.</summary>
        public void Initialize(DefenseGame game, TowerDefinition definition)
        {
            _game = game;
            _definition = definition;
            _shot = gameObject.AddComponent<LineRenderer>();
            _shot.sharedMaterial = game.Settings.ShotMaterial;
            _shot.positionCount = 2;
            _shot.startWidth = 0.07f;
            _shot.endWidth = 0.025f;
            _shot.useWorldSpace = true;
            _shot.enabled = false;
        }

        /// <summary>Attack the enemy furthest along the route within range.</summary>
        private void Update()
        {
            _shotRemaining -= Time.deltaTime;
            _shot.enabled = _shotRemaining > 0f && _game.IsPlaying;
            if (!_game.IsPlaying)
                return;

            _cooldown -= Time.deltaTime;
            DefenseEnemy target = null;
            float bestProgress = -1f;
            float rangeSquared = _definition.Range * _definition.Range;
            foreach (DefenseEnemy enemy in _game.Enemies)
            {
                Vector3 offset = enemy.transform.position - transform.position;
                offset.y = 0f;
                if (enemy.IsAlive && offset.sqrMagnitude <= rangeSquared && enemy.Progress > bestProgress)
                {
                    target = enemy;
                    bestProgress = enemy.Progress;
                }
            }

            if (target == null)
                return;

            Vector3 direction = target.transform.position - _turret.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
                _turret.rotation = Quaternion.LookRotation(direction);

            if (_cooldown > 0f)
                return;

            _shot.SetPosition(0, _turret.position + _turret.forward * 0.45f);
            _shot.SetPosition(1, target.transform.position);
            _shotRemaining = 0.09f;
            _shot.enabled = true;
            _cooldown = _definition.AttackInterval;
            target.TakeDamage(_definition.Damage);
        }
    }
}
