using UnityEngine;

public class CactusSpawner : MonoBehaviour
{
	[Header("References")]
	public GameManager gameManager;
	public GameSpeedController speedController;
	public GameObject cactusPrefab;
	public Transform obstacleParent;

	[Header("Spawn")]
	public float spawnZ = 55f;
	public float spawnX = 0f;
	public float spawnY = 0f;
	public float xRandomRange = 0.42f;
	public float minDistance = 22f;
	public float maxDistance = 44f;
	public float minScale = 0.9f;
	public float maxScale = 1.25f;
	public float clusterChance = 0.35f;
	public float secondTailChance = 0.15f;

	private float distanceUntilNextSpawn;
	private bool spawning;

	public void StartSpawning()
	{
		spawning = true;
		ScheduleNextSpawn(true);
	}

	public void StopSpawning()
	{
		spawning = false;
	}

	public void Tick(float deltaTime)
	{
		if (!spawning)
		{
			return;
		}

		float speed = speedController != null ? speedController.CurrentSpeed : (gameManager != null ? gameManager.CurrentSpeed : 15f);
		distanceUntilNextSpawn -= speed * deltaTime;

		if (distanceUntilNextSpawn <= 0f)
		{
			SpawnCactus();
			ScheduleNextSpawn(false);
		}
	}

	private void SpawnCactus()
	{
		Vector3 position = new Vector3(spawnX + Random.Range(-xRandomRange, xRandomRange), spawnY, spawnZ);
		GameObject cactus = cactusPrefab != null ? Instantiate(cactusPrefab, position, Quaternion.identity) as GameObject : new GameObject("Cactus");
		cactus.name = "Voxel Cactus Obstacle";
		cactus.transform.position = position;
		cactus.transform.rotation = Quaternion.identity;

		if (obstacleParent != null)
		{
			cactus.transform.SetParent(obstacleParent);
		}

		CactusObstacle obstacle = cactus.GetComponent<CactusObstacle>();
		if (obstacle == null)
		{
			obstacle = cactus.AddComponent<CactusObstacle>();
		}

		obstacle.gameManager = gameManager;
		obstacle.speedController = speedController;
		obstacle.autoBuildOnStart = false;
		obstacle.BuildRandomCactusGroup(Random.Range(minScale, maxScale), clusterChance, secondTailChance);
	}

	private void ScheduleNextSpawn(bool firstSpawn)
	{
		float speed = speedController != null ? speedController.CurrentSpeed : 15f;
		float speedPadding = Mathf.Clamp(speed * 0.18f, 0f, 8f);
		distanceUntilNextSpawn = Random.Range(minDistance + speedPadding, maxDistance + speedPadding);

		if (firstSpawn)
		{
			distanceUntilNextSpawn *= 0.65f;
		}
	}
}
