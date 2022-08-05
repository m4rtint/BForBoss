using System.Collections;
using Perigon.Entities;
using UnityEngine;
using UnityEngine.Pool;

namespace BForBoss
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyBehaviour _enemyAgent;
        [SerializeField] [Min(1)] private int _enemiesToSpawn = 3;
        [SerializeField] [Min(0.0f)] private float _timeBetweenSpawns = 2.5f; //In seconds

        [SerializeField, Tooltip("Should max amount of enemies spawn on initialization")]
        private bool _burstInitialSpawn = true;

        private ObjectPool<EnemyBehaviour> _pool;
        private LifeCycleManager _lifeCycleManager = null;
        private bool _canSpawn = true;

        private WaveModel _waveModel;

        public void Initialize(LifeCycleManager lifeCycleManager, WaveModel waveModel)
        {
            _lifeCycleManager = lifeCycleManager;
            _waveModel = waveModel;
            
            if (_pool == null)
            {
                SetupPool();
            }
            
            SpawnInitialEnemies();
        }

        public void Reset()
        {
            if (_pool != null)
            {
                _pool.Clear();
            }
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
        
        private void SetupPool()
       {
           _pool = new ObjectPool<EnemyBehaviour>(() => GenerateEnemy(_enemyAgent),
                (enemy) =>
                {
                    Transform spawnTransform = transform;
                    enemy.transform.SetParent(spawnTransform);
                    enemy.transform.SetPositionAndRotation(spawnTransform.position, spawnTransform.rotation);
                    enemy.gameObject.SetActive(true);
                },
                (enemy) =>
                {
                    enemy.gameObject.SetActive(false);
                });
        }

        private void Release(EnemyBehaviour behaviour)
        {
            _pool.Release(behaviour);
            _waveModel?.OnEnemyKilled?.Invoke();
            StartCoroutine(SpawnEnemies(1));
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
                yield return new WaitForSeconds(_timeBetweenSpawns);
                
                if (!_canSpawn)
                {
                    yield break;
                }
                
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            _lifeCycleManager.AddEnemyBehaviourFromSpawner(_pool.Get(), Release);
            _waveModel?.OnEnemySpawned?.Invoke();
        }

        private EnemyBehaviour GenerateEnemy(EnemyBehaviour enemyAgent)
        {
            return Instantiate(enemyAgent);
        }
    }
}
