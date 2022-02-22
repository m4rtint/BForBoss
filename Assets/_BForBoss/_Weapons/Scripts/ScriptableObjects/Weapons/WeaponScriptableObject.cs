using Sirenix.OdinInspector;
using UnityEngine;

namespace Perigon.Weapons
{
    public interface IWeaponProperties
    {
        string NameOfWeapon { get; }
        float RateOfFire { get; }
        Sprite Crosshair { get; }
        float BulletSpread { get; }
        float GetBulletSpreadWeight(float timeSinceFiring);
        float ReloadDuration { get; }
        int BulletsPerShot { get; }
        int AmmunitionAmount { get; }
        float VisualRecoilForce { get; }
        BulletTypes TypeOfBullet { get; }

    }

    [CreateAssetMenu(fileName = "WeaponProperties", menuName = "PerigonGames/Weapon", order = 1)]
    public class WeaponScriptableObject : ScriptableObject, IWeaponProperties
    {
        [SerializeField] private string _nameOfWeapon = "";
        [SerializeField] private float _rateOfFire = 0.1f;
        [SerializeField] private float _bulletSpread = 1f;
        [SerializeField] private AnimationCurve _bulletSpreadMultipler;
        [SerializeField]
        [Range(0.01f, 10f)] private float _reloadDuration = 0.5f;
        [SerializeField] private int _bulletsPerShot = 1;
        [SerializeField] 
        [Range(1, 1000)] private int _ammunitionAmount = 20;
        [SerializeField] private BulletTypes _typeOfBullet = BulletTypes.NoPhysics;
        [Title("Visuals")]
        [PreviewField]
        [SerializeField] private Sprite _crosshair = null;

        [SerializeField] private float _visualRecoil = 0.5f;
        public string NameOfWeapon => _nameOfWeapon;
        public float RateOfFire => _rateOfFire;
        public float BulletSpread => _bulletSpread;
        public float ReloadDuration => _reloadDuration;
        public int BulletsPerShot => _bulletsPerShot;
        public int AmmunitionAmount => _ammunitionAmount;
        public float VisualRecoilForce => _visualRecoil;
        public Sprite Crosshair => _crosshair;
        public BulletTypes TypeOfBullet => _typeOfBullet;

        public float GetBulletSpreadWeight(float timeSinceFiring)
        {
            float clampedTimeSinceFiring = Mathf.Clamp(timeSinceFiring, 0, 1);
            return _bulletSpreadMultipler.Evaluate(clampedTimeSinceFiring);
        }
    }
}
