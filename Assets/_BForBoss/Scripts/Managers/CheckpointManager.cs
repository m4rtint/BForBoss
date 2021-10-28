using PerigonGames;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BForBoss
{
    public interface IDetectInput
    {
        string GetInput();
    }
    public partial class CheckpointManager : MonoBehaviour
    {
        [SerializeField] private Checkpoint _spawnPoint = null;
        [SerializeField] private Checkpoint[] _checkpoints = null;
        [SerializeField] private Checkpoint _endPoint = null;
        private Checkpoint _activeCheckpoint = null;

        public Vector3 CheckpointPosition => _activeCheckpoint == null ? _spawnPoint.transform.position : _activeCheckpoint.transform.position;
        public Quaternion CheckpointRotation => _activeCheckpoint == null ? _spawnPoint.transform.rotation : _activeCheckpoint.transform.rotation;

        public void Initialize()
        {
            if (_checkpoints.IsNullOrEmpty())
            {
                Debug.LogError("No Checkpoints found in the CheckPointManager!");
            }

            for (int i = 0; i < _checkpoints.Length; i++)
            {
                Checkpoint checkpoint = _checkpoints[i];

                if (checkpoint == null)
                {
                    Debug.LogError("There are null Checkpoints associated with the CheckpointManager");
                }

                checkpoint.OnEnterArea += SetNewCheckpoint;
            }

            _endPoint.OnEnterArea += OnEnteredLastPoint;
        }

        public void Reset()
        {
            foreach (Checkpoint checkpoint in _checkpoints)
            {
                checkpoint.Reset();
            }

            _activeCheckpoint = null;
        }

        private void SetNewCheckpoint(Checkpoint checkpoint)
        {
            CheckInputHack();
            _activeCheckpoint = checkpoint;
            _activeCheckpoint.SetCheckpoint();
        }

        private void CheckInputHack()
        {
            if (Keyboard.current.anyKey.isPressed)
            {
                IncrementMkb();
            }
            else
            {
                IncrementController();
            }
        }

        private void OnEnteredLastPoint(Checkpoint _)
        {
            CheckInputHack();
            StateManager.Instance.SetState(State.EndRace);
        }
        
        private void OnDestroy()
        {
            foreach (Checkpoint checkpoint in _checkpoints)
            {
                checkpoint.OnEnterArea -= SetNewCheckpoint;
            }
        }
    }

    public partial class CheckpointManager : IDetectInput
    {
        private int _controllerCount = 0;
        private int _mkbCount = 0;

        public void IncrementController()
        {
            _controllerCount++;
        }

        public void IncrementMkb()
        {
            _mkbCount++;
        }
        
        public string GetInput()
        {
            if (_mkbCount > _controllerCount)
            {
                return "MKB";
            }

            return "Controller";
        }
    }
}
