using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense
{
    public sealed class DefenseGame : MonoBehaviour
    {
        private const float HUD_WIDTH = 260f;
        private const float HUD_HEIGHT = 600f;
        private static readonly Vector2Int[] ROUTE_CORNERS =
        {
            new Vector2Int(0, 1), new Vector2Int(4, 1), new Vector2Int(4, 6),
            new Vector2Int(8, 6), new Vector2Int(8, 2), new Vector2Int(12, 2)
        };
        private static readonly Vector2Int[] PAD_CELLS =
        {
            new Vector2Int(1, 2), new Vector2Int(3, 0), new Vector2Int(3, 3),
            new Vector2Int(5, 2), new Vector2Int(3, 5), new Vector2Int(5, 7),
            new Vector2Int(7, 5), new Vector2Int(9, 6), new Vector2Int(7, 3),
            new Vector2Int(9, 1), new Vector2Int(10, 3), new Vector2Int(11, 1)
        };

        [SerializeField] private DefenseSettings _settings;
        [SerializeField] private Camera _camera;
        private readonly List<DefenseEnemy> _enemies = new List<DefenseEnemy>();
        private Transform _board;
        private Vector3[] _route;
        private int _credits;
        private int _health;
        private int _waveIndex;
        private int _spawned;
        private int _selectedTower;
        private float _spawnTimer;
        private bool _waveActive;
        private bool _finished;
        private string _message;
        private GUIStyle _titleStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _buttonStyle;

        public DefenseSettings Settings => _settings;
        public IReadOnlyList<DefenseEnemy> Enemies => _enemies;
        public bool IsPlaying => !_finished && enabled;
        public int Credits => _credits;
        public int Health => _health;
        public int CompletedWaves => _waveIndex;
        public bool IsWaveActive => _waveActive;
        public bool IsFinished => _finished;

        /// <summary>Validate scene dependencies before constructing the first match.</summary>
        private void Start()
        {
            if (_settings == null || !_settings.IsValid() || _camera == null)
            {
                _message = "Invalid game settings. Check the Console and scene references.";
                Debug.LogError("Tower Defense requires valid settings, prefabs, materials and a camera.", this);
                _finished = true;
                enabled = false;
                return;
            }
            RestartMatch();
        }

        /// <summary>Clear all match objects and reset economy, waves and the board.</summary>
        public void RestartMatch()
        {
            if (_board != null)
            {
                _board.gameObject.SetActive(false);
                Destroy(_board.gameObject);
            }
            _enemies.Clear();
            _credits = _settings.StartingCredits;
            _health = _settings.BaseHealth;
            _waveIndex = 0;
            _spawned = 0;
            _spawnTimer = 0f;
            _selectedTower = 0;
            _waveActive = false;
            _finished = false;
            _message = "Build on cyan pads, then start the wave.";
            _board = new GameObject("Match Board").transform;
            _board.SetParent(transform, false);
            CreateBoard();
        }

        /// <summary>Build a fixed winding route surrounded by designated construction pads.</summary>
        private void CreateBoard()
        {
            var pathCells = new HashSet<Vector2Int>();
            for (int i = 1; i < ROUTE_CORNERS.Length; i++)
            {
                Vector2Int cell = ROUTE_CORNERS[i - 1];
                Vector2Int end = ROUTE_CORNERS[i];
                Vector2Int step = new Vector2Int(
                    System.Math.Sign(end.x - cell.x), System.Math.Sign(end.y - cell.y));
                pathCells.Add(cell);
                while (cell != end)
                {
                    cell += step;
                    pathCells.Add(cell);
                }
            }

            var pads = new HashSet<Vector2Int>(PAD_CELLS);
            for (int x = 0; x < 13; x++)
            for (int z = 0; z < 9; z++)
            {
                var cell = new Vector2Int(x, z);
                GameObject tile = Instantiate(_settings.TilePrefab, CellPosition(cell, 0f),
                    Quaternion.identity, _board);
                bool isPad = pads.Contains(cell);
                tile.name = isPad ? $"Build Pad {x},{z}" : $"Tile {x},{z}";
                tile.GetComponent<Renderer>().sharedMaterial = pathCells.Contains(cell)
                    ? _settings.PathMaterial : isPad ? _settings.PadMaterial : _settings.GroundMaterial;
                if (isPad)
                    tile.AddComponent<BuildPad>();
            }

            _route = new Vector3[ROUTE_CORNERS.Length];
            for (int i = 0; i < _route.Length; i++)
                _route[i] = CellPosition(ROUTE_CORNERS[i], 0.55f);
            Instantiate(_settings.BasePrefab, CellPosition(ROUTE_CORNERS[_route.Length - 1], 0.7f),
                Quaternion.identity, _board);
        }

        /// <summary>Convert a grid location into a centered world-space position.</summary>
        private static Vector3 CellPosition(Vector2Int cell, float height)
        {
            return new Vector3(cell.x - 6f, height, cell.y - 4f);
        }

        /// <summary>Spawn one wave at a time and handle mouse construction with the Input System.</summary>
        private void Update()
        {
            if (!IsPlaying)
                return;

            if (_waveActive && _spawned < _settings.Waves[_waveIndex].Count)
            {
                _spawnTimer -= Time.deltaTime;
                if (_spawnTimer <= 0f)
                {
                    DefenseSettings.Wave wave = _settings.Waves[_waveIndex];
                    DefenseEnemy enemy = Instantiate(_settings.EnemyPrefab, _board);
                    enemy.Initialize(this, _route, wave);
                    _enemies.Add(enemy);
                    _spawned++;
                    _spawnTimer = wave.SpawnInterval;
                }
            }

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                StartWave();

            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
                return;
            Vector2 pointer = Mouse.current.position.ReadValue();
            float scale = HudScale();
            if (new Rect(12f * scale, 12f * scale, HUD_WIDTH * scale, HUD_HEIGHT * scale)
                .Contains(new Vector2(pointer.x, Screen.height - pointer.y)))
                return;
            if (Physics.Raycast(_camera.ScreenPointToRay(pointer), out RaycastHit hit, 100f)
                && hit.collider.TryGetComponent(out BuildPad pad))
                TryBuild(pad);
        }

        /// <summary>Spend credits only after a valid unoccupied pad receives a tower.</summary>
        public bool TryBuild(BuildPad pad)
        {
            if (!IsPlaying)
                return false;
            if (pad == null || pad.transform.parent != _board)
            {
                _message = "Choose a cyan construction pad.";
                return false;
            }
            if (pad.IsOccupied)
            {
                _message = "This pad already has a tower.";
                return false;
            }
            TowerDefinition definition = _settings.Towers[_selectedTower];
            if (_credits < definition.Cost)
            {
                _message = "Not enough credits. Defeat enemies to earn more.";
                return false;
            }
            DefenseTower tower = Instantiate(definition.Prefab,
                pad.transform.position + Vector3.up * 0.15f, Quaternion.identity, _board);
            tower.Initialize(this, definition);
            pad.Occupy();
            _credits -= definition.Cost;
            _message = $"{definition.DisplayName} constructed.";
            return true;
        }

        /// <summary>Start the next wave only after the previous wave has been resolved.</summary>
        public void StartWave()
        {
            if (!IsPlaying || _waveActive)
                return;
            _waveActive = true;
            _spawned = 0;
            _spawnTimer = 0f;
            _message = "Enemies incoming! You can keep building.";
        }

        /// <summary>Resolve rewards, base damage, wave completion and match outcomes exactly once.</summary>
        public void ResolveEnemy(DefenseEnemy enemy, bool defeated, int reward)
        {
            if (!_enemies.Remove(enemy) || _finished)
                return;
            if (defeated)
                _credits += reward;
            else
                _health = Mathf.Max(0, _health - 1);

            if (_health == 0)
            {
                _finished = true;
                _waveActive = false;
                _message = "BASE LOST\nTry defending the corners first.";
                return;
            }

            DefenseSettings.Wave wave = _settings.Waves[_waveIndex];
            if (_spawned != wave.Count || _enemies.Count != 0)
                return;
            _credits += wave.ClearBonus;
            _waveIndex++;
            _waveActive = false;
            _finished = _waveIndex == _settings.Waves.Length;
            _message = _finished ? "VICTORY!\nAll waves survived." : $"Wave cleared! Bonus +{wave.ClearBonus}.";
        }

        /// <summary>Keep the HUD usable on both small and large game windows.</summary>
        private static float HudScale()
        {
            return Mathf.Min(Screen.width / 1100f, Screen.height / 650f);
        }

        /// <summary>Draw the match HUD and controls without requiring external UI assets.</summary>
        private void OnGUI()
        {
            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(GUI.skin.label)
                    { fontSize = 24, fontStyle = FontStyle.Bold, wordWrap = true };
                _bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
                _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 15, wordWrap = true };
            }
            Matrix4x4 previous = GUI.matrix;
            float scale = HudScale();
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            GUILayout.BeginArea(new Rect(12f, 12f, HUD_WIDTH, HUD_HEIGHT), GUI.skin.box);
            GUILayout.Space(10f);
            GUILayout.Label("CIRCUIT\nDEFENSE", _titleStyle);
            if (_settings != null && enabled)
            {
                GUILayout.Label($"BASE  {_health} / {_settings.BaseHealth}\nCREDITS  {_credits}\n"
                    + $"WAVE  {Mathf.Min(_waveIndex + 1, _settings.Waves.Length)} / {_settings.Waves.Length}",
                    _bodyStyle);
                GUILayout.Space(12f);
                for (int i = 0; i < _settings.Towers.Length; i++)
                {
                    TowerDefinition tower = _settings.Towers[i];
                    GUI.enabled = !_finished;
                    string marker = i == _selectedTower ? "> " : "";
                    if (GUILayout.Button($"{marker}{tower.DisplayName}  ${tower.Cost}\n"
                        + $"DMG {tower.Damage} / {tower.AttackInterval:0.0}s / RNG {tower.Range:0.0}",
                        _buttonStyle, GUILayout.Height(62f)))
                    {
                        _selectedTower = i;
                        _message = $"{tower.DisplayName} selected. Click a cyan pad.";
                    }
                }
                GUILayout.Space(10f);
                GUI.enabled = !_finished && !_waveActive;
                if (GUILayout.Button(_waveActive ? "WAVE IN PROGRESS" : "START WAVE [SPACE]",
                    _buttonStyle, GUILayout.Height(42f)))
                    StartWave();
                GUI.enabled = true;
                if (GUILayout.Button("RESTART", _buttonStyle, GUILayout.Height(32f)))
                    RestartMatch();
            }
            GUILayout.Space(12f);
            GUILayout.Label(_message, _bodyStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Select a tower, then click a cyan pad.\nProtect the gold core.", _bodyStyle);
            GUILayout.EndArea();
            GUI.matrix = previous;
        }
    }
}
