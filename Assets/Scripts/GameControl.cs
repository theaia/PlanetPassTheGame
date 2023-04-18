using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using DG.Tweening;

public class GameControl : MonoBehaviour
{
    public static GameControl Instance;
    public LayerMask whatToHit;
    public float shearRadius = 1f;
    private Camera gameCam;

    public TurretBehavior turret;
    public EnemySpawner enemySpawner;
    public SheepSpawner sheepSpawner;
    public Build buildManager;

    public int Score;

    public SheepController mySheep;

    [Header("Leveling Up")]
    public int level = 1;
    public int maxLevel = 20;
    public int xp, maxXp;
    public float coveredPercent;
    public AnimationCurve xpCurve;
    public int startXpCost = 40, maxXpCost = 800;
    public TMP_Text levelCounter;
    public Slider xpSlider;
    public TMP_Text score;
    public PlayerUpgradeManager upgradeManager;
    public TextMeshProUGUI percentText;
    public Slider percentSlider;
    public Slider chargeSlider;
    public Slider shootSlider;
    public GameObject dummyPlanet;
    public GameObject activePlanet;
    public Transform placedObjects;
    public int UpgradePoints;
    public TextMeshProUGUI UpgradePointText;

    [Header("Sheep Management")]
    public int curSheep = 0;
    public int maxSheep = 1;
    public Slider sheepSlider;
    public RectTransform sheepBG;
    public RectTransform sheepFilled;

    Vector4 channelMask = new Vector4(0f, 0f, 1f, 0f);
    int spritesX = 1;
    int spritesY = 1;
    int worldCoverage = 0;

    public float grassScale = 1.0f;


    public GameObject GameOverScreen;
    private bool gameOver;
    public bool IsTransitioningWorlds;

    private async void Awake() {
		if(Instance == null) {
            Instance = this;
		} else {
            Destroy(this);
		}

        await UnityServices.InitializeAsync();
		if (AuthenticationService.Instance.IsSignedIn) {
            return;
		}
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        AddUpgradePoints(0); //Sets Display;
    }

    public bool IsGameOver() {
        return gameOver;
	}

    private void SpawnNewPlanet() {
        if(IsTransitioningWorlds) {
            return;
		}
        curSheep = -1;
        IsTransitioningWorlds = true;
        activePlanet.transform.parent.DOMove(-Camera.main.transform.right * 500f, .5f).SetEase(Ease.InBounce).OnComplete(() => {
            dummyPlanet.SetActive(true);
            activePlanet.transform.parent.position = -Camera.main.transform.right * 500f;
            sheepSpawner.ClearSheep();
            ClearPlacedObjects();
            StartCoroutine(MoveInNewPlanet());
        });
    }

    private void ClearPlacedObjects() {
        foreach(Transform _child in placedObjects) {
            Destroy(_child);
		}
	}

    IEnumerator MoveInNewPlanet() {
        yield return new WaitForSeconds(1f);
        activePlanet.transform.parent.DOMove(Vector3.zero, 1f).SetEase(Ease.OutBounce).OnComplete(() => {
            coveredPercent = 0f;
            worldCoverage = 0;
            SplatManager.Instance.Start();
            UpdateCoveredPercent();
            dummyPlanet.SetActive(false);
            sheepSpawner.SpawnSheep(1);
        });
        yield return new WaitUntil(() => curSheep > 0); 
        IsTransitioningWorlds = false;
    }

	private void Start()
    {
        AddScore(0);
        GameOverScreen.SetActive(false);
        gameOver = false;

        gameCam = Camera.main;	
        maxXp = startXpCost;
        xpSlider.value = 0;

        UpdateSheepCounter();
        UpdateCoveredPercent();
    }
    
    private void Update()
    {
		if (gameOver || IsTransitioningWorlds) {
            return;
		}

        if(Input.GetMouseButtonDown(1))
        {
            TryShearWool();
        }

        if (Input.GetMouseButtonUp(0)) {
            mySheep = null;
        }

        if (Input.GetMouseButtonDown(0) && !IsTransitioningWorlds)
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

    public void AddUpgradePoints(int _value) {
        UpgradePoints += _value;
        UpgradePointText.text = UpgradePoints.ToString();
	}

    public void AddScore(int _score) {
        int _startingScore = Score;
        Score += _score;
        StartCoroutine(LerpScore(_startingScore, Score, .5f));
    }

    public void ProcessWorldCoveragePoints() {
        if(coveredPercent >= 25 * (worldCoverage + 1)) {
            worldCoverage++;
            AddUpgradePoints(1);
            AddScore(25);
		}

        if(worldCoverage == 4) {
            SpawnNewPlanet();
		}
	}

    IEnumerator LerpScore(int _start, int _target, float _duration) {
        float _time = 0;
        float _score;
        while (_time < _duration) {
            _score = Mathf.Lerp(_start, _target, _time/_duration);
            _time += Time.deltaTime;
            score.text = _score.ToString("F0");
            yield return new WaitForEndOfFrame();
        }

        score.text = Score.ToString("F0");
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
        //upgradeManager.SetupUpgradeScreen();
    }

    public void SheepDied()
    {
        curSheep --;
        UpdateSheepCounter();
        if(curSheep == 0 && !IsTransitioningWorlds) {
            //Debug.Log("Ending game. Sheep:" + curSheep + " IsTransitioningWorlds:" + IsTransitioningWorlds);
            GameOver();
		}
    }

    public void UpdateSheepCounter()
    {
        sheepSlider.value = curSheep;
        sheepSlider.maxValue = maxSheep;
        if(sheepBG) sheepBG.sizeDelta = new Vector2(sheepBG.sizeDelta.x, 25.5f * maxSheep);
        if(sheepFilled) sheepFilled.sizeDelta = new Vector2(sheepFilled.sizeDelta.x, 25.5f * maxSheep);
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

    private void GameOver() {
        gameOver = true;
        if(GameOverScreen) GameOverScreen.SetActive(true);
	}

    public void Restart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

    public void UpdateCoveredPercent() {
        percentText.text = coveredPercent.ToString("F0") + "%";
        percentSlider.value = coveredPercent/100f;
    }
}
