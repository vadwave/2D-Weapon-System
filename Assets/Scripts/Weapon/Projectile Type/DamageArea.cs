using UnityEngine;

namespace WeaponSystem
{
    public class DamageArea : MonoBehaviour
    {
        protected MeleeWeapon weapon;
        protected Collider2D trigger;
        private void Awake()
        {
            trigger = GetComponent<Collider2D>();
        }
        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (!weapon.Targets.Contains(collision))
                weapon.Targets.Add(collision);
        }
        protected virtual void OnTriggerStay2D(Collider2D collision)
        {

        }
        protected virtual void OnTriggerExit2D(Collider2D collision)
        {
            if (weapon.Targets.Contains(collision))
                weapon.Targets.Remove(collision);
        }
        internal void Enabled(bool value)
        {
            trigger.enabled = value;
        }
        internal virtual void Init(MeleeWeapon weapon)
        {
            this.weapon = weapon;
        }
    }
}
