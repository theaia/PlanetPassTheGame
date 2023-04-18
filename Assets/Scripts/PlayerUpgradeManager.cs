using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUpgradeManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerUpgrade
    {
        public string name;
        public int stacks;
        public Sprite icon;
    }
    
    public PlayerUpgrade[] playerUpgrades;
    //int[] upgradeChoices = new int[3];

    [Header("Refrences")]
    public GameObject upgradeScreen;
    public GameControl gameController;
    public TextMeshProUGUI bombUpgradeText;
    public TextMeshProUGUI shieldUpgradeText;
    
    [Header("UI")]
    public Image[] upgradeIcons;
    public TMP_Text[] upgradeLabels;
    
    /*public void SetupUpgradeScreen()
    {
        upgradeScreen.SetActive(true);
        Time.timeScale = 0;

        List<int> pickedChoices = new List<int>();
        
        int i = 2;
        while(i >= 0)
        {
            upgradeChoices[i] = Random.Range(0,playerUpgrades.Length);
            while(pickedChoices.Contains(upgradeChoices[i]))
            {
                upgradeChoices[i] = Random.Range(0,playerUpgrades.Length);
            }
            pickedChoices.Add(upgradeChoices[i]);

            PlayerUpgrade randUpgrade = playerUpgrades[upgradeChoices[i]];

            upgradeIcons[i].sprite = randUpgrade.icon;
            upgradeLabels[i].text = randUpgrade.name;

            i--;
        }

    }*/

    public void UpgradeButton(int buttonIndex)
    {
        if(GameControl.Instance.UpgradePoints <= 0) {
            return;
		}

        ActivateUpgrade(buttonIndex);
    }

    private void ActivateUpgrade(int index)
    {
        playerUpgrades[index].stacks ++;
        
        if(index == 0) // revive dead sheep
        {
            int sheepToSpawn = gameController.maxSheep - gameController.curSheep;
            if(sheepToSpawn != 0)
                gameController.sheepSpawner.SpawnSheep(sheepToSpawn);
        }
        else if(index == 1) // decrease shoot delay by 25%
        {
            gameController.turret.upgradeShootSpeedMod *= 0.75f;
        }
        else if(index == 2) // 1st = unlock bombs, stacks = decrease bomb recharge time by 25%
        {
            if (playerUpgrades[index].stacks == 1) {
                gameController.buildManager.UnlockBuilding(0);
                bombUpgradeText.text = "Faster bomb recharge";
            } else {
                gameController.buildManager.buildings[0].rechargeSpeedMod *= 0.75f;
            }
        }
        else if(index == 3) // 1st = unlock shield, stacks = decrease shield recharge time by 25%
        {
            if (playerUpgrades[index].stacks == 1) {
                gameController.buildManager.UnlockBuilding(1);
                shieldUpgradeText.text = "Faster shield recharge";
            } else {
                gameController.buildManager.buildings[1].rechargeSpeedMod *= 0.75f;
            }
        }
        else if(index == 4) // increase shoot damage by 25%
        {
            gameController.turret.upgradeShootDamMod *= 0.75f;
        }
        GameControl.Instance.AddUpgradePoints(-1);
    }
}
