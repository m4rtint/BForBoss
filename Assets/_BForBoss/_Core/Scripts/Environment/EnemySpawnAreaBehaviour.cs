using System.Collections;
using Perigon.Entities;
using UnityEngine;
using UnityEngine.AI;
using Logger = Perigon.Utility.Logger;

namespace BForBoss
{
    public class EnemySpawnAreaBehaviour : MonoBehaviour
    {
        [SerializeField] [Min(1)] private int _enemiesToSpawn = 3;
        [SerializeField] [Min(0.0f)] private float _secondsBetweenSpawns = 2.5f;
        [SerializeField, Tooltip("Should max amount of enemies spawn upon initialization")]
        private bool _burstInitialSpawn = true;
        
        private bool _canSpawn = true;
        private WaveModel _waveModel;
        private EnemyContainer _enemyContainer;
        
        public void Initialize(EnemyContainer enemyContainer, WaveModel waveModel)
        {
            _enemyContainer = enemyContainer;
            _waveModel = waveModel;

            SpawnInitialEnemies();
        }

        public void PauseSpawning()
        {
            _canSpawn = false;
            StopCoroutine(nameof(SpawnEnemies));
        }

        public void ResumeSpawning()
        {
            _canSpawn = true;
            SpawnInitialEnemies();
        }

        public void Reset()
        {
            _canSpawn = true;
        }

        public void CleanUp()
        {
            _canSpawn = false;
            StopCoroutine(nameof(SpawnEnemies));
        }
        
        private void SpawnInitialEnemies()
        {
            if (_burstInitialSpawn)
            {
                for (int i = 0; i < _enemiesToSpawn; i++)
                {
                    if (!_canSpawn)
                    {
                        return;
                    }

                    SpawnEnemy();
                }
            }
            else
            {
                StartCoroutine(SpawnEnemies(_enemiesToSpawn));
            }
        }

        private IEnumerator SpawnEnemies(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new WaitForSeconds(_secondsBetweenSpawns);
                
                if (!_canSpawn)
                {
                    yield break;
                }
                
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            Logger.LogString("<color=red>Spawn Enemy</color>", "wavesmode");
            var enemy = _enemyContainer.GetEnemy();
            
            enemy.OnDeath += HandleOnEnemyDeath;
            enemy.transform.SetPositionAndRotation(transform.position, transform.rotation);
            enemy.transform.SetParent(transform, true);
            
            //https://answers.unity.com/questions/771908/navmesh-issue-with-spawning-players.html
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            agent.enabled = false;
            agent.enabled = true;
            
            _waveModel?.IncrementSpawnCount();
        }

        private void HandleOnEnemyDeath(EnemyBehaviour enemy)
        {
            enemy.OnDeath -= HandleOnEnemyDeath;
            _waveModel?.IncrementKillCount();
        }
    }
}
