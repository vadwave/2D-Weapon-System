using System;
using UnityEngine;
using WeaponSystem;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] Animator animator;

    const float crossFadeTime = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Shot(float speed)
    {
        SetSpeed(speed);
        CrossFade("Weapon Shot");

    }
    public void Initialize(FireMode fireMode)
    {
         animator.SetBool("Is Manually-Operated", fireMode == FireMode.Single);
    }
    public void Reload(bool roundInChamber, float speed)
    {
        SetSpeed(speed);
        if(roundInChamber) CrossFade("Weapon Reload");
        else CrossFade("Weapon Empty Reload");
    }
    void SetSpeed(float speed)
    {
        animator.SetFloat("Speed", speed);
    }
    void CrossFade(string stateName)
    {
        animator.CrossFadeInFixedTime(stateName, crossFadeTime);
    }
    public void Draw()
    {
        SetSpeed(1f);
        CrossFade("Weapon Draw");
    }
    public void Hide()
    {
        SetSpeed(1f);
        CrossFade("Weapon Hide");
    }
    public void SwitchMode()
    {

    }
    public void Insert(float speed)
    {
        SetSpeed(speed);
        CrossFade("Insert");
    }
    public void StartInserting(float speed)
    {
        SetSpeed(speed);
        CrossFade("Start Insert");
    }
    public void StopInserting(float speed)
    {
        SetSpeed(speed);
        CrossFade("Stop Insert");
    }
    public void Overheat(float speed)
    {
        SetSpeed(speed);
        CrossFade("Weapon Overheat");
    }
}
