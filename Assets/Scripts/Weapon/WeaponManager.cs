using System;
using UnityEngine;
using UnityEngine.UI;

namespace WeaponSystem
{
    [Serializable]
    public enum InputType
    {
        Down,
        Hold,
        Up,
    }

    public class WeaponManager : MonoBehaviour
    {
        [SerializeField] LayerMask collidedMask;
        [SerializeField] Weapon[] weapons;
        [SerializeField] Transform[] arms;
        [SerializeField] Text text;
        [SerializeField] Text textCharge;
        [SerializeField] Text textHeat;
        private bool isReloaded;
        private float currentCapacity;
        private float maxCapacity;
        private CapacityType typeCapacity;
        private int currentIndex = 0;

        private void Awake()
        {
            foreach (Weapon weapon in weapons)
            {
                weapon.Initialize(collidedMask);
                weapon.OnChangedCapacity += ChangeCapacity;
                weapon.OnChangedCharge += ChangeCharge;
                weapon.OnChangedHeat += ChangeHeat;
                weapon.OnStartReloaded += StartReloaded;
                weapon.OnStopReloaded += StopReloaded;
                weapon.Model.CreateShellEjection(arms[0]);
            }
            weapons[currentIndex].Draw();
            ChangeCapacity(weapons[currentIndex].CurrentRounds, weapons[currentIndex].Data.MagazineCapacity, weapons[currentIndex].Data.Origin.CapacityType);
        }
        private void Update()
        {
            Rotate();
            if (Input.GetMouseButtonDown(0))
            {
                Trigger(InputType.Down);
            }
            else if (Input.GetMouseButton(0))
            {
                Trigger(InputType.Hold);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                Trigger(InputType.Up);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                Reload();
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ChangeWeapon();
            }
        }
        void ChangeWeapon()
        {
            weapons[currentIndex].Hide();
            weapons[currentIndex].gameObject.SetActive(false);
            if (currentIndex == weapons.Length - 1) currentIndex = 0;
            else currentIndex++;
            weapons[currentIndex].gameObject.SetActive(true);
            weapons[currentIndex].Draw();
            ChangeCapacity(weapons[currentIndex].CurrentRounds, weapons[currentIndex].Data.MagazineCapacity, weapons[currentIndex].Data.Origin.CapacityType);
        }
        void ChangeCapacity(float current, float max, CapacityType type)
        {
            currentCapacity = current;
            maxCapacity = max;
            typeCapacity = type;
            Show();
        }
        void ChangeCharge(float current, float max)
        {
            textCharge.text = "[";
            for (int i = 0; i < current; i++)
                textCharge.text += ">";
            textCharge.text += "]";
            //textCharge.text = $"[{current}|{max}]";
        }
        void ChangeHeat(float current, float max)
        {
            textHeat.text = $"[{Mathf.RoundToInt((current / max) * 100f)}%]";
        }
        void Show()
        {
            switch (typeCapacity)
            {
                case CapacityType.Numeric: text.text = $"[ {currentCapacity} | {maxCapacity} ]"; break;
                case CapacityType.Percent: text.text = $"[ {(float)System.Math.Round((currentCapacity / maxCapacity) * 100, 2)}% ]"; break;
                default: text.text = $"[ {currentCapacity} ]"; break;
            }
            if (isReloaded) text.text += " [Reload]";
        }
        void StartReloaded()
        {
            isReloaded = true;
            Show();
        }
        void StopReloaded()
        {
            isReloaded = false;
            Show();
        }

        void Trigger(InputType input)
        {
            foreach (Weapon weapon in weapons)
            {
                if(weapon.enabled && weapon.gameObject.activeInHierarchy)
                    weapon.Trigger(input);
            }
        }
        void Reload()
        {
            foreach (Weapon weapon in weapons)
            {
                if (weapon.enabled && weapon.gameObject.activeInHierarchy)
                    weapon.TriggerReload();
            }
        }
        void Rotate()
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            foreach(Transform transform in arms)
            {
                var weaponPos = weapons[0].Model.ShootPoint.transform.position - transform.right;
                Vector2 bodyPoint = weaponPos;
                Vector2 direction = mousePos - bodyPoint;

                DebugCross(bodyPoint, Color.red);
                Debug.DrawRay(bodyPoint, direction, Color.magenta);

                float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                var lookRotation = Quaternion.Euler(rotZ * Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 100f * Time.deltaTime);
            }
        }
        private void OnDrawGizmos()
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            DebugCross(mousePos, Color.yellow);
            foreach (Transform transform in arms)
            {
                Weapon weapon = transform.GetComponentInChildren<Weapon>();
                if(weapon != null && weapon.Data != null)
                {
                    Debug.DrawRay(transform.position, transform.right * weapon.Data.Range, Color.red);
                    Debug.DrawRay(weapon.Model.ShootPoint.position, weapon.Model.ShootPoint.right * weapon.Data.Range, Color.blue);
                }
            }
        }
        public static void DebugCross(Vector2 point, Color color, float size = 0.25f)
        {
            Debug.DrawLine(point, point + (Vector2.down * size), color);
            Debug.DrawLine(point, point + (Vector2.up * size), color);
            Debug.DrawLine(point, point + (Vector2.left * size), color);
            Debug.DrawLine(point, point + (Vector2.right * size), color);
        }
    }
}