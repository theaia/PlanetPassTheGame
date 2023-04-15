using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretBehavior : MonoBehaviour
{
    public GameControl gameController;
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
    
    [Header("UI")]
    public Slider chargeBar, shootBar;

    private void Start()
    {
        camAnim = Camera.main.GetComponent<Animator>();
        
        curShootCountdown = maxShootCountdown;
        UpdateCharge();
    }

    public void AddCharge()
    {
        curCharge += woolChargeAmount;
        UpdateCharge();
    }

    private void UpdateCharge()
    {
        chargeBar.value = curCharge/maxCharge;
    }

    private void Update()
    {
        // attack loop
        shootBar.value = curShootCountdown/maxShootCountdown;

        if(curCharge >= shootChargeCost)
        {
            if(gameController.enemySpawner.enemyHolder.childCount == 0)
                return;
            
            curShootCountdown -= Time.deltaTime;
            
            if(curShootCountdown <= 0)
                DoAttack();
        }
    }

    private void DoAttack()
    {
        Transform ene = gameController.enemySpawner.enemyHolder.GetChild(0);
        ene.gameObject.GetComponent<EnemyRotation>().TakeDamage(turretDamage);

        shootPart.Play();
        camAnim.SetTrigger("shake");

        curCharge -= shootChargeCost;
        UpdateCharge();

        curShootCountdown = maxShootCountdown;
    }
}
