using System;
using UnityEngine;

namespace BForBoss
{
    [RequireComponent(typeof(MeshRenderer))]
    public class SimonSaysBlockBehaviour : MonoBehaviour, ISimonSaysBlock
    {
        public event Action<ISimonSaysBlock, SimonSaysColor> OnBlockCompleted;

        private SimonSaysColorData _data;
        private MeshRenderer _renderer;
        
        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _renderer.material.color = _data.Color;
        }
        
        public void SetColorData(SimonSaysColorData data)
        {
            _data = data;
            _renderer.material.SetColor("_BaseColor", data.Color);
        }

        public void Reset()
        {
            SetColorData(new SimonSaysColorData(color: SimonSaysUtility.DefaultNoneColor, simonSaysColor: SimonSaysColor.None));
        }
        
        private void OnCollisionEnter(Collision other)
        {
            if (_data.SimonSaysColor != SimonSaysColor.None)
            {
                OnBlockCompleted?.Invoke(this, _data.SimonSaysColor);
            }
        }
    }
}
