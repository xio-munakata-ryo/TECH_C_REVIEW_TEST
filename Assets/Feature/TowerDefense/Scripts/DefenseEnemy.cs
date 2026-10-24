using UnityEngine;

namespace TowerDefense
{
    public sealed class DefenseEnemy : MonoBehaviour
    {
        private DefenseGame _game;
        private Vector3[] _route;
        private int _waypoint;
        private int _health;
        private int _reward;
        private float _speed;

        public bool IsAlive { get; private set; }
        public float Progress { get; private set; }

        /// <summary>Initialize a spawned enemy with its wave and route.</summary>
        public void Initialize(DefenseGame game, Vector3[] route, DefenseSettings.Wave wave)
        {
            _game = game;
            _route = route;
            _health = wave.Health;
            _reward = wave.Reward;
            _speed = wave.Speed;
            _waypoint = 1;
            Progress = 0f;
            transform.position = route[0];
            IsAlive = true;
        }

        /// <summary>Travel the route without losing movement at waypoint boundaries.</summary>
        private void Update()
        {
            if (!IsAlive || !_game.IsPlaying)
                return;

            float remaining = _speed * Time.deltaTime;
            while (remaining > 0f && _waypoint < _route.Length)
            {
                Vector3 target = _route[_waypoint];
                float distance = Vector3.Distance(transform.position, target);
                float step = Mathf.Min(distance, remaining);
                transform.position = Vector3.MoveTowards(transform.position, target, step);
                Progress += step;
                remaining -= step;
                if (distance <= step)
                    _waypoint++;
            }

            if (_waypoint == _route.Length)
                Resolve(false);
        }

        /// <summary>Apply one hit and award credits only once on death.</summary>
        public void TakeDamage(int damage)
        {
            if (!IsAlive || !_game.IsPlaying)
                return;

            _health -= damage;
            if (_health <= 0)
                Resolve(true);
        }

        /// <summary>Remove the enemy before notifying the match of its outcome.</summary>
        private void Resolve(bool defeated)
        {
            IsAlive = false;
            _game.ResolveEnemy(this, defeated, _reward);
            Destroy(gameObject);
        }
    }
}
