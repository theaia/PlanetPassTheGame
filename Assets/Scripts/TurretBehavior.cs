using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretBehavior : MonoBehaviour
{
    public ParticleSystem shootPart;
    Animator camAnim;
    
    [Header("Properties")]
    public float maxCharge = 100, curCharge;
    public float woolChargeAmount = 10;

    [HideInInspector]
    public float upgradeShootSpeedMod = 1f, upgradeShootDamMod = 1f;

    public float maxShootCountdown = 2f;
    float curShootCountdown;

    public float shootChargeCost = 25;

    public float turretDamage = 5f;
   

    private void Start()
    {
        camAnim = Camera.main.GetComponent<Animator>();
        
        curShootCountdown = maxShootCountdown;
        UpdateCharge();
        GameControl.Instance.turret = this;
    }

    public void AddCharge()
    {
        curCharge += woolChargeAmount;
        UpdateCharge();
    }

    private void UpdateCharge()
    {
        GameControl.Instance.chargeSlider.value = curCharge/maxCharge;
    }

    private void Update()
    {
        // attack loop
        GameControl.Instance.shootSlider.value = curShootCountdown/maxShootCountdown;

        if(curCharge >= shootChargeCost)
        {
            if(GameControl.Instance.enemySpawner.enemyHolder.childCount == 0)
                return;
            
            curShootCountdown -= Time.deltaTime;
            
            if(curShootCountdown <= 0)
                DoAttack();
        }
    }

    private void DoAttack()
    {
        Transform ene = GameControl.Instance.enemySpawner.enemyHolder.GetChild(0);
        ene.gameObject.GetComponent<EnemyRotation>().TakeDamage(turretDamage);

        shootPart.Play();
        camAnim.SetTrigger("shake");

        curCharge -= shootChargeCost;
        UpdateCharge();

        curShootCountdown = maxShootCountdown;
    }
}
