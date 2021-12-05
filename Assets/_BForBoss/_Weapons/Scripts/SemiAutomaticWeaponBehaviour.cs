using UnityEngine;
using UnityEngine.InputSystem;

namespace Perigon.Weapons
{
    public class SemiAutomaticWeaponBehaviour : WeaponBehaviour
    {
        protected override void OnFire(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                _weapon.FireIfPossible();
            }
        }

        protected override void Update()
        {
            _weapon.DecrementElapsedTimeRateOfFire(Time.deltaTime);
        }
    }
}
