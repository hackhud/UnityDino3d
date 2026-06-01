using UnityEngine;

public class CactusObstacle : MonoBehaviour
{
	public GameManager gameManager;
	public GameSpeedController speedController;
	public float destroyZ = -16f;
	public float voxelSize = 0.07f;
	public float visualYOffset = 0.04f;
	public bool autoBuildOnStart = true;

	private readonly string[] cactusResources =
	{
		"Dino3D/Voxels/cactus/cactus",
		"Dino3D/Voxels/cactus/cactus_tall",
		"Dino3D/Voxels/cactus/cactus_thin"
	};

	private BoxCollider hitbox;
	private bool built;

	private void Awake()
	{
		ConfigurePhysics();
	}

	private void Start()
	{
		if (autoBuildOnStart && !built)
		{
			BuildRandomCactusGroup(1f, 0.3f, 0.12f);
		}
	}

	private void Update()
	{
		if (gameManager == null)
		{
			gameManager = GameManager.Instance;
		}

		if (gameManager == null || !gameManager.IsRunning)
		{
			return;
		}

		float speed = speedController != null ? speedController.CurrentSpeed : gameManager.CurrentSpeed;
		transform.position += Vector3.back * speed * Time.deltaTime;

		if (transform.position.z < destroyZ)
		{
			Destroy(gameObject);
		}
	}

	public void BuildRandomCactusGroup(float mainScale, float clusterChance, float secondTailChance)
	{
		built = true;

		for (int i = transform.childCount - 1; i >= 0; i--)
		{
			Destroy(transform.GetChild(i).gameObject);
		}

		float minZ = 0f;
		float maxZ = 0f;
		float maxHeight = 0f;
		float maxWidth = 0f;

		AddCactusPart("Main", RandomResource(), 0f, mainScale, ref minZ, ref maxZ, ref maxHeight, ref maxWidth);

		if (Random.value < clusterChance)
		{
			float tailScale = Random.Range(0.55f, 0.82f) * mainScale;
			AddCactusPart("Tail", RandomResource(), 0.78f, tailScale, ref minZ, ref maxZ, ref maxHeight, ref maxWidth);

			if (Random.value < secondTailChance)
			{
				float secondScale = Random.Range(0.45f, 0.68f) * mainScale;
				AddCactusPart("Tail2", RandomResource(), 1.35f, secondScale, ref minZ, ref maxZ, ref maxHeight, ref maxWidth);
			}
		}

		if (hitbox != null)
		{
			hitbox.center = new Vector3(0f, Mathf.Max(0.8f, maxHeight * 0.5f), (minZ + maxZ) * 0.5f);
			hitbox.size = new Vector3(Mathf.Max(0.75f, maxWidth), Mathf.Max(1.25f, maxHeight), Mathf.Max(0.75f, maxZ - minZ + 0.55f));
		}
	}

	private void AddCactusPart(
		string label,
		string resource,
		float localZ,
		float scale,
		ref float minZ,
		ref float maxZ,
		ref float maxHeight,
		ref float maxWidth)
	{
		GameObject visual = new GameObject("Voxel Cactus " + label);
		visual.transform.SetParent(transform);
		float localX = Random.Range(-0.05f, 0.05f);
		visual.transform.localPosition = new Vector3(localX, visualYOffset, localZ);
		visual.transform.localRotation = Quaternion.Euler(0f, Random.Range(-10f, 10f), 0f);
		visual.transform.localScale = Vector3.one * scale;

		VoxMeshData data = VoxMeshFactory.ApplyToObject(visual, resource, voxelSize);
		if (data != null)
		{
			float groundedY = visualYOffset - data.Mesh.bounds.min.y * scale;
			visual.transform.localPosition = new Vector3(localX, groundedY, localZ);
			maxHeight = Mathf.Max(maxHeight, data.Mesh.bounds.max.y * scale + groundedY);
			maxWidth = Mathf.Max(maxWidth, Mathf.Max(data.Mesh.bounds.size.x, data.Mesh.bounds.size.z) * scale);
			minZ = Mathf.Min(minZ, localZ - data.Mesh.bounds.extents.z * scale);
			maxZ = Mathf.Max(maxZ, localZ + data.Mesh.bounds.extents.z * scale);
		}
	}

	private string RandomResource()
	{
		return cactusResources[Random.Range(0, cactusResources.Length)];
	}

	private void ConfigurePhysics()
	{
		Rigidbody obstacleBody = GetComponent<Rigidbody>();
		if (obstacleBody == null)
		{
			obstacleBody = gameObject.AddComponent<Rigidbody>();
		}

		obstacleBody.useGravity = false;
		obstacleBody.isKinematic = true;

		hitbox = GetComponent<BoxCollider>();
		if (hitbox == null)
		{
			hitbox = gameObject.AddComponent<BoxCollider>();
		}

		hitbox.isTrigger = true;
		hitbox.size = new Vector3(0.9f, 1.6f, 0.8f);
		hitbox.center = new Vector3(0f, 0.8f, 0f);

		Renderer rootRenderer = GetComponent<Renderer>();
		if (rootRenderer != null)
		{
			rootRenderer.enabled = false;
		}

		MeshFilter rootFilter = GetComponent<MeshFilter>();
		if (rootFilter != null)
		{
			rootFilter.sharedMesh = null;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<DinoController>() != null)
		{
			if (gameManager == null)
			{
				gameManager = GameManager.Instance;
			}

			if (gameManager != null)
			{
				gameManager.GameOver();
			}
		}
	}
}
