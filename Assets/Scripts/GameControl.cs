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
    public float SplatScale = 5f;
    public int woolChargeAmount = 10;
    public float newWorldCooldown = 3f;
    public bool CanSuck = true;

    [Header("Sheep Management")]
    public int curSheep = 0;
    public int maxSheep = 1;
    public Slider sheepSlider;
    public RectTransform sheepBG;
    public RectTransform sheepFilled;

    int worldCoverage = 0;

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
        //Debug.Log("Getting new world");
        IsTransitioningWorlds = true;
        activePlanet.transform.parent.DOMove(-Camera.main.transform.right * 500f, .5f).SetEase(Ease.InElastic, .01f).OnComplete(() => {
            dummyPlanet.SetActive(true);
            activePlanet.transform.parent.position = -Camera.main.transform.right * 500f;
            sheepSpawner.ClearSheep();
            ClearPlacedObjects();
            StartCoroutine(MoveInNewPlanet());
        });
    }

    private void ClearPlacedObjects() {
        foreach(Transform _child in placedObjects) {
            Destroy(_child.gameObject);
		}
	}

    IEnumerator MoveInNewPlanet() {
        yield return new WaitForSeconds(.5f);
        activePlanet.transform.parent.DOMove(Vector3.zero, 1f).SetEase(Ease.OutElastic, .01f).OnComplete(() => {
            coveredPercent = 0f;
            worldCoverage = 0;
            SplatManager.Instance.Start();
            UpdateCoveredPercent();
            dummyPlanet.SetActive(false);
            sheepSpawner.SpawnSheep(1);
            IsTransitioningWorlds = false;
            CanSuck = false;
            Invoke("EnableCanSuck", newWorldCooldown);
        });

    }

    private void EnableCanSuck() {
        CanSuck = true;
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

        CanSuck = true;
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
		if (gameOver) {
            return;
		}
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

        if(worldCoverage == 4 && !IsTransitioningWorlds) {
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
		if (IsTransitioningWorlds) {
            return;
		}
        sheepSpawner.SpawnSheep(1);
        maxSheep ++;
        UpdateSheepCounter();

        // let you choose an upgrade
        //upgradeManager.SetupUpgradeScreen();
    }

    public void SheepDied()
    {
        UpdateSheepCounter();
        if(curSheep == 0 && !gameOver) {
            GameOver();
		}
    }

    public void UpdateSheepCounter()
    {
        int _curSheep = 0;
        foreach(Transform _child in sheepSpawner.transform) {
            if (_child.gameObject.activeSelf) {
                _curSheep++;
            }
		}
        curSheep = _curSheep;
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
