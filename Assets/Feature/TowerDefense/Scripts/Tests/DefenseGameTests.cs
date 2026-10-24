#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TowerDefense.Tests
{
    public sealed class DefenseGameTests
    {
        private DefenseGame _game;
        private DefenseSettings _settings;
        private float _originalTimeScale;

        /// <summary>Load the shipped scene and clone its balance data to isolate each test.</summary>
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _originalTimeScale = Time.timeScale;
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/Main/TowerDefense.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            _game = Object.FindFirstObjectByType<DefenseGame>();
            Assert.That(_game, Is.Not.Null);
            _settings = Object.Instantiate(_game.Settings);
            var serialized = new SerializedObject(_game);
            serialized.FindProperty("_settings").objectReferenceValue = _settings;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _game.RestartMatch();
            yield return null;
        }

        /// <summary>Destroy only the cloned settings after each match.</summary>
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = _originalTimeScale;
            Object.Destroy(_settings);
            yield return null;
        }

        /// <summary>Verify the shipped board, asset wiring, initial credits and build transactions.</summary>
        [UnityTest]
        public IEnumerator ConstructionChargesOnceAndRejectsUnaffordableTowers()
        {
            Assert.That(_settings.IsValid(), Is.True);
            BuildPad[] pads = Object.FindObjectsByType<BuildPad>(FindObjectsSortMode.None);
            Assert.That(pads.Length, Is.EqualTo(12));
            Assert.That(_game.Credits, Is.EqualTo(180));
            Assert.That(_game.TryBuild(pads[0]), Is.True);
            Assert.That(_game.Credits, Is.EqualTo(120));
            Assert.That(_game.TryBuild(pads[0]), Is.False);
            Assert.That(_game.Credits, Is.EqualTo(120));
            Assert.That(_game.TryBuild(pads[1]), Is.True);
            Assert.That(_game.TryBuild(pads[2]), Is.True);
            Assert.That(_game.TryBuild(pads[3]), Is.False);
            Assert.That(_game.Credits, Is.Zero);
            yield return null;
        }

        /// <summary>Verify enemy movement, leaks, wave completion and non-duplicated wave starts.</summary>
        [UnityTest]
        public IEnumerator LeakedEnemyDamagesBaseAndCompletesWaveOnce()
        {
            ConfigureFastWaves();
            int startingCredits = _game.Credits;
            _game.StartWave();
            _game.StartWave();
            yield return WaitForWave();
            Assert.That(_game.Health, Is.EqualTo(_settings.BaseHealth - 1));
            Assert.That(_game.CompletedWaves, Is.EqualTo(1));
            Assert.That(_game.Credits, Is.EqualTo(startingCredits + _settings.Waves[0].ClearBonus));
            Assert.That(_game.Enemies.Count, Is.Zero);
        }

        /// <summary>Verify real towers acquire targets, kill enemies and grant a single reward.</summary>
        [UnityTest]
        public IEnumerator TowerAttacksEnemyAndAwardsRewardOnce()
        {
            ConfigureFastWaves();
            _settings.Waves[0].Speed = 2f;
            BuildPad pad = Object.FindObjectsByType<BuildPad>(FindObjectsSortMode.None)
                .OrderBy(candidate => Vector3.Distance(candidate.transform.position, new Vector3(-6f, 0f, -3f)))
                .First();
            Assert.That(_game.TryBuild(pad), Is.True);
            int credits = _game.Credits;
            _game.StartWave();
            yield return WaitForWave();
            Assert.That(_game.Health, Is.EqualTo(_settings.BaseHealth));
            Assert.That(_game.Credits, Is.EqualTo(credits + _settings.Waves[0].Reward
                + _settings.Waves[0].ClearBonus));
        }

        /// <summary>Verify all five waves lead to victory and restart clears every match object.</summary>
        [UnityTest]
        public IEnumerator VictoryAndRestartResetTheWholeMatch()
        {
            ConfigureFastWaves();
            for (int i = 0; i < _settings.Waves.Length; i++)
            {
                _game.StartWave();
                yield return WaitForWave();
            }
            Assert.That(_game.IsFinished, Is.True);
            Assert.That(_game.CompletedWaves, Is.EqualTo(5));
            _game.StartWave();
            Assert.That(_game.IsWaveActive, Is.False);
            _game.RestartMatch();
            yield return null;
            Assert.That(_game.IsFinished, Is.False);
            Assert.That(_game.CompletedWaves, Is.Zero);
            Assert.That(_game.Credits, Is.EqualTo(_settings.StartingCredits));
            Assert.That(_game.Health, Is.EqualTo(_settings.BaseHealth));
            Assert.That(Object.FindObjectsByType<BuildPad>(FindObjectsSortMode.None).Length, Is.EqualTo(12));
            Assert.That(Object.FindObjectsByType<DefenseTower>(FindObjectsSortMode.None).Length, Is.Zero);
            Assert.That(Object.FindObjectsByType<DefenseEnemy>(FindObjectsSortMode.None).Length, Is.Zero);
        }

        /// <summary>Verify a destroyed base stops further spawning and construction until restart.</summary>
        [UnityTest]
        public IEnumerator DefeatStopsTheMatchAndRestartRemovesLiveEnemies()
        {
            ConfigureFastWaves();
            _settings.Waves[0].Count = 20;
            _game.StartWave();
            yield return WaitForWave();
            Assert.That(_game.Health, Is.Zero);
            Assert.That(_game.IsFinished, Is.True);
            BuildPad pad = Object.FindFirstObjectByType<BuildPad>();
            Assert.That(_game.TryBuild(pad), Is.False);
            _game.RestartMatch();
            yield return null;
            Assert.That(_game.Health, Is.EqualTo(_settings.BaseHealth));
            Assert.That(_game.Enemies.Count, Is.Zero);
            Assert.That(Object.FindObjectsByType<DefenseEnemy>(FindObjectsSortMode.None).Length, Is.Zero);
        }

        /// <summary>Verify the actual shipped five-wave balance is winnable using earned credits.</summary>
        [UnityTest]
        public IEnumerator DefaultWavesAreWinnableWithConstructionBetweenWaves()
        {
            Time.timeScale = 10f;
            BuildPad[] pads = Object.FindObjectsByType<BuildPad>(FindObjectsSortMode.None)
                .OrderBy(pad => pad.transform.position.sqrMagnitude).ToArray();
            for (int i = 0; i < _settings.Waves.Length; i++)
            {
                foreach (BuildPad pad in pads)
                    if (!pad.IsOccupied && _game.Credits >= _settings.Towers[0].Cost)
                        Assert.That(_game.TryBuild(pad), Is.True);
                _game.StartWave();
                yield return WaitForWave();
                Assert.That(_game.Health, Is.GreaterThan(0), $"Base lost during default wave {i + 1}.");
            }
            Assert.That(_game.IsFinished, Is.True);
            Assert.That(_game.CompletedWaves, Is.EqualTo(5));
        }

        /// <summary>Verify an in-progress restart cancels spawns and removes towers and enemies.</summary>
        [UnityTest]
        public IEnumerator RestartDuringWaveCancelsSpawningAndClearsTheBoard()
        {
            Assert.That(_game.TryBuild(Object.FindFirstObjectByType<BuildPad>()), Is.True);
            _game.StartWave();
            yield return null;
            Assert.That(_game.Enemies.Count, Is.EqualTo(1));
            _game.RestartMatch();
            yield return null;
            Assert.That(_game.IsWaveActive, Is.False);
            Assert.That(_game.Enemies.Count, Is.Zero);
            Assert.That(_game.Credits, Is.EqualTo(_settings.StartingCredits));
            Assert.That(Object.FindObjectsByType<DefenseEnemy>(FindObjectsSortMode.None).Length, Is.Zero);
            Assert.That(Object.FindObjectsByType<DefenseTower>(FindObjectsSortMode.None).Length, Is.Zero);
        }

        /// <summary>Use short deterministic waves without editing persistent assets.</summary>
        private void ConfigureFastWaves()
        {
            foreach (DefenseSettings.Wave wave in _settings.Waves)
            {
                wave.Count = 1;
                wave.Health = 1;
                wave.Speed = 60f;
                wave.SpawnInterval = 0.1f;
            }
        }

        /// <summary>Fail explicitly if a wave does not complete within the expected time budget.</summary>
        private IEnumerator WaitForWave()
        {
            float deadline = Time.realtimeSinceStartup + 12f;
            while (_game.IsWaveActive && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(_game.IsWaveActive, Is.False, "Wave did not finish within 12 seconds.");
        }
    }
}
#endif
