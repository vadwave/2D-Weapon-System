using UnityEngine;

namespace WeaponSystem
{
    public class Target : MonoBehaviour, IDamageable
    {
        [SerializeField] float health = 1000;
        [SerializeField] Animator animator;
        [SerializeField] Rigidbody2D body;
        private float currentHealth;

        public bool IsAlive => true;

        public void Awake()
        {
            currentHealth = health;
        }
        public void Damage(float damage)
        {
            currentHealth -= damage;
        }

        public void Damage(float damage, Vector3 targetPosition, Vector3 hitPosition)
        {
            currentHealth -= damage;
            Debug.Log("Shot In Target");
            animator?.SetTrigger("Get Hit");
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
