using UnityEngine;
using UnityEngine.Rendering;

public class DinoController : MonoBehaviour
{
	[Header("Jump")]
	public float jumpVelocity = 15f;
	public float gravity = -36f;
	public float holdBoostDelay = 0.2f;
	public float holdBoostVelocityMultiplier = 1.15f;
	public float holdBoostGravity = -24f;
	public float maxFallSpeed = 30f;
	public float groundY = 0.38f;

	[Header("Voxel Animation")]
	public string frameResourcePrefix = "Dino3D/Voxels/t-rex/";
	public float voxelSize = 0.07f;
	public float baseFrameTime = 0.18f;
	public Vector3 visualOffset = new Vector3(0f, 0.10f, 0.03f);
	public Vector3 visualRotation = new Vector3(0f, -90f, 0f);
	public float visualScale = 1.38f;

	[Header("Presentation")]
	public float shadowGroundOffset = 0.04f;
	public Vector3 shadowLocalOffset = new Vector3(0f, 0f, 0.08f);
	public Vector3 shadowScale = new Vector3(0.86f, 0.03f, 1.14f);
	public Color shadowColor = new Color(0.43f, 0.31f, 0.18f, 1f);

	[Header("Runtime")]
	public GameManager gameManager;

	private Rigidbody body;
	private BoxCollider hitbox;
	private Transform shadowTransform;
	private Transform visualTransform;
	private MeshFilter visualFilter;
	private MeshRenderer visualRenderer;
	private VoxMeshData[] runFrames;
	private VoxMeshData deathFrame;
	private Vector3 startPosition;
	private float verticalVelocity;
	private float currentGravity;
	private float jumpHeldTime;
	private float frameTimer;
	private int currentFrame;
	private bool isJumping;
	private bool holdBoosted;
	private bool deadFrameShown;

	private void Awake()
	{
		ApplyReferencePresentationDefaults();
		body = GetComponent<Rigidbody>();
		hitbox = GetComponent<BoxCollider>();
		startPosition = new Vector3(transform.position.x, groundY, transform.position.z);

		ConfigureBody();
		ConfigureHitbox();
		BuildVoxelDino();
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

		UpdateJumpProfile();
		HandleJump(Time.deltaTime);
		UpdateShadow();
		AnimateRun(Time.deltaTime);
	}

	public void ResetDino()
	{
		transform.position = startPosition;
		transform.rotation = Quaternion.identity;

		verticalVelocity = 0f;
		currentGravity = gravity;
		jumpHeldTime = 0f;
		frameTimer = 0f;
		currentFrame = 0;
		isJumping = false;
		holdBoosted = false;
		deadFrameShown = false;

		if (body != null)
		{
			body.velocity = Vector3.zero;
			body.angularVelocity = Vector3.zero;
		}

		ShowFrame(0);
		UpdateShadow();
	}

	public void ShowDeathFrame()
	{
		if (deadFrameShown)
		{
			return;
		}

		deadFrameShown = true;
		if (deathFrame != null)
		{
			ApplyVisual(deathFrame);
		}
	}

	private void HandleJump(float deltaTime)
	{
		if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
		{
			StartJump();
		}

		if (!isJumping)
		{
			transform.position = new Vector3(startPosition.x, groundY, startPosition.z);
			return;
		}

		jumpHeldTime += deltaTime;

		if (!holdBoosted && Input.GetKey(KeyCode.Space) && jumpHeldTime >= holdBoostDelay && verticalVelocity > 0f)
		{
			verticalVelocity *= holdBoostVelocityMultiplier;
			currentGravity = holdBoostGravity;
			holdBoosted = true;
		}

		verticalVelocity += currentGravity * deltaTime;
		if (verticalVelocity < -maxFallSpeed)
		{
			verticalVelocity = -maxFallSpeed;
		}

		Vector3 position = transform.position;
		position.y += verticalVelocity * deltaTime;

		if (position.y <= groundY)
		{
			position.y = groundY;
			verticalVelocity = 0f;
			currentGravity = gravity;
			jumpHeldTime = 0f;
			isJumping = false;
			holdBoosted = false;
		}

		transform.position = position;
	}

	private void StartJump()
	{
		isJumping = true;
		holdBoosted = false;
		jumpHeldTime = 0f;
		currentGravity = gravity;
		verticalVelocity = jumpVelocity;
		ShowFrame((currentFrame + 1) % runFrames.Length);

		if (gameManager != null)
		{
			gameManager.PlayJumpSound();
		}
	}

	private void AnimateRun(float deltaTime)
	{
		if (runFrames == null || runFrames.Length == 0 || isJumping)
		{
			return;
		}

		float speed = gameManager != null ? Mathf.Max(1f, gameManager.CurrentSpeed) : 15f;
		float frameTime = baseFrameTime / (speed / 2f);

		frameTimer += deltaTime;
		if (frameTimer >= frameTime)
		{
			frameTimer = 0f;
			currentFrame++;
			if (currentFrame >= runFrames.Length)
			{
				currentFrame = 0;
			}

			ShowFrame(currentFrame);
		}
	}

	private void UpdateJumpProfile()
	{
		float speed = gameManager != null ? gameManager.CurrentSpeed : 15f;

		if (speed >= 30f)
		{
			jumpVelocity = 20f;
			gravity = -70f;
			holdBoostVelocityMultiplier = 1.2f;
			holdBoostGravity = -44f;
		}
		else if (speed >= 20f)
		{
			jumpVelocity = 17.5f;
			gravity = -52f;
			holdBoostVelocityMultiplier = 1.17f;
			holdBoostGravity = -34f;
		}
		else
		{
			jumpVelocity = 15f;
			gravity = -36f;
			holdBoostVelocityMultiplier = 1.15f;
			holdBoostGravity = -24f;
		}

		if (!isJumping)
		{
			currentGravity = gravity;
		}
	}

	private void BuildVoxelDino()
	{
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

		GameObject visual = new GameObject("Voxel T-Rex");
		visualTransform = visual.transform;
		visual.transform.SetParent(transform);
		visualTransform.localPosition = visualOffset;
		visualTransform.localRotation = Quaternion.Euler(visualRotation);
		visualTransform.localScale = Vector3.one * visualScale;
		visualFilter = visual.AddComponent<MeshFilter>();
		visualRenderer = visual.AddComponent<MeshRenderer>();

		BuildGroundShadow();

		runFrames = new VoxMeshData[8];
		for (int i = 0; i < runFrames.Length; i++)
		{
			runFrames[i] = VoxMeshFactory.Load(frameResourcePrefix + i, voxelSize);
		}

		deathFrame = VoxMeshFactory.Load(frameResourcePrefix + "wow", voxelSize);
		ShowFrame(0);
	}

	private void ShowFrame(int frame)
	{
		if (runFrames == null || runFrames.Length == 0)
		{
			return;
		}

		frame = Mathf.Clamp(frame, 0, runFrames.Length - 1);
		currentFrame = frame;
		ApplyVisual(runFrames[frame]);
	}

	private void ApplyVisual(VoxMeshData frame)
	{
		if (frame == null || visualFilter == null || visualRenderer == null)
		{
			return;
		}

		visualFilter.sharedMesh = frame.Mesh;
		visualRenderer.sharedMaterials = frame.Materials;
		visualRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		visualRenderer.receiveShadows = false;

		if (visualTransform != null)
		{
			float groundedY = visualOffset.y - frame.Mesh.bounds.min.y * visualScale;
			visualTransform.localPosition = new Vector3(visualOffset.x, groundedY, visualOffset.z);
		}
	}

	private void ConfigureBody()
	{
		if (body == null)
		{
			body = gameObject.AddComponent<Rigidbody>();
		}

		body.useGravity = false;
		body.isKinematic = true;
		body.collisionDetectionMode = CollisionDetectionMode.Continuous;
		body.constraints = RigidbodyConstraints.FreezePositionX |
		                   RigidbodyConstraints.FreezePositionZ |
		                   RigidbodyConstraints.FreezeRotationX |
		                   RigidbodyConstraints.FreezeRotationY |
		                   RigidbodyConstraints.FreezeRotationZ;
	}

	private void ApplyReferencePresentationDefaults()
	{
		visualOffset = new Vector3(0f, 0.09f, 0.02f);
		visualScale = 1.46f;
		shadowLocalOffset = new Vector3(0f, 0f, 0.06f);
		shadowScale = new Vector3(0.9f, 0.03f, 1.18f);
	}

	private void ConfigureHitbox()
	{
		if (hitbox == null)
		{
			hitbox = gameObject.AddComponent<BoxCollider>();
		}

		hitbox.isTrigger = false;
		hitbox.center = new Vector3(0f, 1f, 0.04f);
		hitbox.size = new Vector3(0.78f, 1.9f, 0.92f);
	}

	private void BuildGroundShadow()
	{
		GameObject shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		shadow.name = "Jump Shadow";
		shadow.transform.SetParent(transform);
		shadow.transform.localScale = shadowScale;

		MeshRenderer shadowRenderer = shadow.GetComponent<MeshRenderer>();
		shadowRenderer.shadowCastingMode = ShadowCastingMode.Off;
		shadowRenderer.receiveShadows = false;
		shadowRenderer.sharedMaterial = CreateShadowMaterial();

		Collider shadowCollider = shadow.GetComponent<Collider>();
		if (shadowCollider != null)
		{
			Destroy(shadowCollider);
		}

		shadowTransform = shadow.transform;
	}

	private Material CreateShadowMaterial()
	{
		Shader shader = Shader.Find("Standard");
		if (shader == null)
		{
			shader = Shader.Find("Legacy Shaders/Diffuse");
		}

		if (shader == null)
		{
			shader = Shader.Find("Diffuse");
		}

		Material material = new Material(shader);
		material.name = "DinoShadow";
		material.color = shadowColor;
		if (material.HasProperty("_Mode"))
		{
			material.SetFloat("_Mode", 0f);
		}

		if (material.HasProperty("_Glossiness"))
		{
			material.SetFloat("_Glossiness", 0f);
		}

		if (material.HasProperty("_Metallic"))
		{
			material.SetFloat("_Metallic", 0f);
		}

		return material;
	}

	private void UpdateShadow()
	{
		if (shadowTransform == null)
		{
			return;
		}

		float jumpHeight = Mathf.Max(0f, transform.position.y - groundY);
		float shrink = Mathf.Lerp(1f, 0.68f, Mathf.Clamp01(jumpHeight / 3f));
		shadowTransform.localPosition = new Vector3(
			shadowLocalOffset.x,
			-groundY + shadowGroundOffset - jumpHeight,
			shadowLocalOffset.z);
		shadowTransform.localScale = new Vector3(shadowScale.x * shrink, shadowScale.y, shadowScale.z * shrink);
	}

	private void OnTriggerEnter(Collider other)
	{
		CactusObstacle cactus = other.GetComponent<CactusObstacle>();
		if (cactus == null)
		{
			cactus = other.GetComponentInParent<CactusObstacle>();
		}

		if (cactus != null && gameManager != null)
		{
			gameManager.GameOver();
		}
	}
}
