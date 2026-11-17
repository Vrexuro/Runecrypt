using UnityEngine;

namespace Runecrypt.Core
{
    // GDD §5.5: Tersingkap (+damage di terang) dan gelap menguatkan musuh (+kecepatan).
    [CreateAssetMenu(menuName = "Runecrypt/Light Rules")]
    public class LightRules : ScriptableObject
    {
        public float exposedDamageBonus = 0.25f;
        public float darkSpeedBonus = 0.2f;

        public float ApplyDamage(float baseDamage, bool lit) => lit ? baseDamage * (1f + exposedDamageBonus) : baseDamage;
        public float SpeedMultiplier(bool lit) => lit ? 1f : 1f + darkSpeedBonus;
    }
}
