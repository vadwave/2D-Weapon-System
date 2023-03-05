using System;
using System.Collections;
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

        [SerializeField] Transform leftHand;
        [SerializeField] Transform rightHand;

        private CapacityType typeCapacity;
        private bool isReloaded;
        private float currentCapacity;
        private float maxCapacity;
        private int currentIndex = 0;
        Coroutine corSwitch;

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
            UpdateHandPoints();
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
        IEnumerator SwitchWeapon()
        {
            weapons[currentIndex].Hide();
            yield return new WaitForSeconds(0.3f);
            weapons[currentIndex].gameObject.SetActive(false);
            if (currentIndex == weapons.Length - 1) currentIndex = 0;
            else currentIndex++;
            yield return new WaitForSeconds(0.1f);
            weapons[currentIndex].gameObject.SetActive(true);
            weapons[currentIndex].Draw();
            yield return new WaitForSeconds(1f);
            ChangeCapacity(weapons[currentIndex].CurrentRounds, weapons[currentIndex].Data.MagazineCapacity, weapons[currentIndex].Data.Origin.CapacityType);
            yield return new WaitForSeconds(0.1f);
            corSwitch = null;
        }
        void ChangeWeapon()
        {
            if(corSwitch == null) corSwitch = StartCoroutine(SwitchWeapon());
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
                var weaponPos = weapons[currentIndex].Model.ShootPoint.transform.position - transform.right;
                Vector2 bodyPoint = weaponPos;
                Vector2 direction = mousePos - bodyPoint;

                DebugCross(bodyPoint, Color.red);
                Debug.DrawRay(bodyPoint, direction, Color.white);

                float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                var lookRotation = Quaternion.Euler(rotZ * Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 100f * Time.deltaTime);
            }
        }
        void UpdateHandPoints()
        {
            var weapon = weapons[currentIndex];
            if (weapon)
            {
                rightHand.position = weapon.Model.HandPoint.position;
                rightHand.rotation = weapon.Model.HandPoint.rotation;
                if (weapon.Model.HandHelpPoint)
                {
                    leftHand.position = weapon.Model.HandHelpPoint.position;
                    leftHand.rotation = weapon.Model.HandHelpPoint.rotation;
                }
                    
            }
        }

        private void OnDrawGizmos()
        {
            DebugUpdateHandPoints();
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
        void DebugUpdateHandPoints()
        {
            Weapon weapon = null;
            foreach (Weapon weaponTemp in weapons)
            {
                if (weaponTemp.enabled && weaponTemp.gameObject.activeInHierarchy)
                    weapon = weaponTemp;
            }
            if (weapon)
            {
                rightHand.position = weapon.Model.HandPoint.position;
                rightHand.rotation = weapon.Model.HandPoint.rotation;
                if (weapon.Model.HandHelpPoint)
                {
                    leftHand.position = weapon.Model.HandHelpPoint.position;
                    leftHand.rotation = weapon.Model.HandHelpPoint.rotation;
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