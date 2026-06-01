using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	[Header("Legacy Scene References")]
	public Text scoreUI;
	public Text highscoreUI;
	public Transform player;
	public GameObject obstaclePrefab;
	public Transform obstacles;
	public int obstacleStartX = 55;
	public GameObject deathOverlayUI;
	public Fade fade;

	[Header("Dino Runner Systems")]
	public DinoController dinoController;
	public CactusSpawner cactusSpawner;
	public ScoreManager scoreManager;
	public GameSpeedController speedController;
	public UIManager uiManager;
	public DinoEnvironment environment;

	private AudioSource audioSource;
	private AudioClip jumpClip;
	private AudioClip hitClip;
	private AudioClip scoreClip;
	private bool isRunning;
	private bool isGameOver;

	public bool IsRunning
	{
		get { return isRunning; }
	}

	public bool IsGameOver
	{
		get { return isGameOver; }
	}

	public float CurrentSpeed
	{
		get { return speedController != null ? speedController.CurrentSpeed : 0f; }
	}

	private void Awake()
	{
		Instance = this;
		FindSceneSystems();
		ConfigureAudio();
	}

	private void Start()
	{
		if (fade != null)
		{
			fade.BeginFade(-1);
		}

		StartRun();
	}

	private void Update()
	{
		if (isRunning)
		{
			speedController.Tick(Time.deltaTime);
			cactusSpawner.Tick(Time.deltaTime);
			scoreManager.Tick(Time.deltaTime, speedController.CurrentSpeed);
		}

		if (Input.GetKeyDown(KeyCode.R))
		{
			Restart();
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			SwitchScene(0);
		}
	}

	public void StartRun()
	{
		FindSceneSystems();
		ClearObstacles();

		isRunning = true;
		isGameOver = false;

		speedController.ResetSpeed();
		scoreManager.ResetScore();
		uiManager.ShowHud();

		if (environment != null)
		{
			environment.ResetEnvironment();
		}

		if (dinoController != null)
		{
			dinoController.ResetDino();
		}

		cactusSpawner.StartSpawning();
	}

	public void GameOver()
	{
		if (isGameOver)
		{
			return;
		}

		isRunning = false;
		isGameOver = true;

		cactusSpawner.StopSpawning();
		scoreManager.FinishScore();

		if (dinoController != null)
		{
			dinoController.ShowDeathFrame();
		}

		PlayHitSound();
		uiManager.ShowGameOver(scoreManager.Score, scoreManager.BestScore);
	}

	public void Restart()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
	}

	public void SwitchScene(int scene)
	{
		if (fade != null)
		{
			StartCoroutine(SceneTransition(scene));
			return;
		}

		SceneManager.LoadScene(scene, LoadSceneMode.Single);
	}

	public void PlayJumpSound()
	{
		PlaySound(jumpClip);
	}

	public void PlayHitSound()
	{
		PlaySound(hitClip);
	}

	public void PlayScoreSound()
	{
		PlaySound(scoreClip);
	}

	private IEnumerator SceneTransition(int scene)
	{
		yield return new WaitForSeconds(fade.BeginFade(1));
		SceneManager.LoadScene(scene, LoadSceneMode.Single);
	}

	private void FindSceneSystems()
	{
		if (speedController == null)
		{
			speedController = GetComponent<GameSpeedController>();
		}

		if (scoreManager == null)
		{
			scoreManager = GetComponent<ScoreManager>();
		}

		if (cactusSpawner == null)
		{
			cactusSpawner = GetComponent<CactusSpawner>();
		}

		if (uiManager == null)
		{
			uiManager = GetComponent<UIManager>();
		}

		if (environment == null)
		{
			environment = GetComponent<DinoEnvironment>();
		}

		if (dinoController == null)
		{
			if (player != null)
			{
				dinoController = player.GetComponent<DinoController>();
			}
			else
			{
				dinoController = FindObjectOfType<DinoController>();
			}
		}

		if (speedController == null)
		{
			speedController = gameObject.AddComponent<GameSpeedController>();
		}

		if (scoreManager == null)
		{
			scoreManager = gameObject.AddComponent<ScoreManager>();
		}

		if (cactusSpawner == null)
		{
			cactusSpawner = gameObject.AddComponent<CactusSpawner>();
		}

		if (uiManager == null)
		{
			uiManager = gameObject.AddComponent<UIManager>();
		}

		if (environment == null)
		{
			environment = gameObject.AddComponent<DinoEnvironment>();
		}

		if (dinoController == null)
		{
			dinoController = FindObjectOfType<DinoController>();
		}

		if (dinoController == null)
		{
			GameObject dino = new GameObject("Dino");
			dino.transform.position = Vector3.zero;
			dinoController = dino.AddComponent<DinoController>();
			player = dino.transform;
		}

		scoreManager.scoreText = scoreUI;
		scoreManager.finalScoreText = highscoreUI;
		scoreManager.speedController = speedController;

		uiManager.scoreText = scoreUI;
		uiManager.gameOverPanel = deathOverlayUI;
		uiManager.gameOverText = highscoreUI;

		cactusSpawner.gameManager = this;
		cactusSpawner.speedController = speedController;
		cactusSpawner.cactusPrefab = obstaclePrefab;
		cactusSpawner.obstacleParent = obstacles;
		cactusSpawner.spawnZ = obstacleStartX;

		environment.speedController = speedController;

		if (dinoController != null)
		{
			dinoController.gameManager = this;
		}
	}

	private void ConfigureAudio()
	{
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
		}

		audioSource.playOnAwake = false;
		jumpClip = Resources.Load<AudioClip>("Dino3D/Sound/jump");
		hitClip = Resources.Load<AudioClip>("Dino3D/Sound/hit");
		scoreClip = Resources.Load<AudioClip>("Dino3D/Sound/score");
	}

	private void PlaySound(AudioClip clip)
	{
		if (audioSource != null && clip != null)
		{
			audioSource.PlayOneShot(clip);
		}
	}

	private void ClearObstacles()
	{
		if (obstacles == null)
		{
			return;
		}

		for (int i = obstacles.childCount - 1; i >= 0; i--)
		{
			Destroy(obstacles.GetChild(i).gameObject);
		}
	}
}
