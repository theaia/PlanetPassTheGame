using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameControl : MonoBehaviour
{
    public LayerMask whatToHit;
    public float shearRadius = 1f;
    private Camera gameCam;

    public TurretBehavior turret;
    public EnemySpawner enemySpawner;
    public SheepSpawner sheepSpawner;
    public Build buildManager;

    SheepController mySheep;

    [Header("Leveling Up")]
    public int level = 1;
    public int maxLevel = 20;
    public int xp, maxXp;
    public AnimationCurve xpCurve;
    public int startXpCost = 40, maxXpCost = 800;
    public TMP_Text levelCounter;
    public Slider xpSlider;
    public PlayerUpgradeManager upgradeManager;

    [Header("Sheep Management")]
    public int curSheep = 0;
    public int maxSheep = 1;
    public TMP_Text sheepCounter;
    
    private void Start()
    {
        gameCam = Camera.main;	
        maxXp = startXpCost;
        xpSlider.value = 0;

        UpdateSheepCounter();
    }
    
    private void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            TryShearWool();
        }

        if(Input.GetMouseButtonDown(0))
        {
            TryStartDragSheep();
        }
    }

    public void IncreaseXP(int xpGain)
    {
        xp += xpGain;
        if(xp >= maxXp)
        {
            LevelUp();
        }

        xpSlider.value = (float)xp/(float)maxXp;
    }

    private void LevelUp()
    {
        level ++;
        levelCounter.text = $"Level {level}";
        xp = 0;
        maxXp = (int)(startXpCost + (xpCurve.Evaluate((float)level/(float)maxLevel) * maxXpCost));

        // spawn an extra sheep
        sheepSpawner.SpawnSheep(1);
        maxSheep ++;
        UpdateSheepCounter();

        // let you choose an upgrade
        upgradeManager.SetupUpgradeScreen();
    }

    public void SheepDied()
    {
        curSheep --;
        UpdateSheepCounter();
    }

    public void UpdateSheepCounter()
    {
        sheepCounter.text = $"{curSheep} / {maxSheep} Sheep";
    }

    private SheepController FindSheep(Ray ray)
    {
		RaycastHit hitInfo;

		if(Physics.SphereCast(ray, shearRadius, out hitInfo, Mathf.Infinity, whatToHit))
		{ 
            SheepController sheep = hitInfo.transform.gameObject.GetComponent<SheepController>();
            return sheep;
		}
        else
        {
            return null;
        }
    }

    // private void ReleaseSheep()
    // {
    //     mySheep.StopDraggingSheep();
    //     mySheep = null;
    // }

    private void TryStartDragSheep()
    {
        SheepController sheep = FindSheep(gameCam.ScreenPointToRay(Input.mousePosition));

        if(sheep == null)
            return;

        sheep.StartDraggingSheep();
        mySheep = sheep;
    }

    private void TryShearWool()
    {
        SheepController sheep = FindSheep(gameCam.ScreenPointToRay(Input.mousePosition));

        if(sheep == null)
            return;

		if(sheep.hasWool)
        {
            sheep.HarvestWool();
            turret.AddCharge();
        }
    }
}
