using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponSystem
{
    public class AmmoModel : MonoBehaviour
    {
        public Transform HitPoint => hitPoint;
        public Transform TracerPoint => tracerPoint;

        [SerializeField] private Transform tracerPoint;
        [SerializeField] private Transform hitPoint;

        [SerializeField] private GameObject muzzle;
        [SerializeField] private GameObject shell;
        [SerializeField] private GameObject tracer;

        //Sound
    }
}