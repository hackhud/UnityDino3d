using System.Collections.Generic;
using UnityEngine;

public class DinoEnvironment : MonoBehaviour
{
	private sealed class LoopStrip
	{
		public readonly List<Transform> Pieces = new List<Transform>();
		public float ChunkLength;
		public float StartZ;
		public float X;
		public float Y;
		public float RecycleZ;
	}

	private struct DecorationBand
	{
		public float X;
		public float Y;
		public float JitterX;
		public float ScaleMultiplier;

		public DecorationBand(float x, float y, float jitterX, float scaleMultiplier)
		{
			X = x;
			Y = y;
			JitterX = jitterX;
			ScaleMultiplier = scaleMultiplier;
		}
	}

	private const string RunnerGroundResource = "Dino3D/Voxels/ground";
	[Header("References")]
	public GameSpeedController speedController;

	[Header("Track")]
	public int groundChunks = 1;
	public float firstGroundZ = 58f;
	public float groundTopY = 0f;
	public float trackWidth = 9.4f;
	public float trackChunkLength = 176f;
	public float trackHeight = 0.14f;
	public Color trackColor = new Color(0.73f, 0.58f, 0.38f, 1f);
	public Color trackBedColor = new Color(0.82f, 0.67f, 0.44f, 1f);

	[Header("Desert Surface")]
	public int desertChunks = 1;
	public float desertWidth = 88f;
	public float desertLength = 236f;
	public float desertStartZ = 58f;
	public float desertY = -0.28f;
	public Color desertColor = new Color(0.82f, 0.67f, 0.44f, 1f);

	[Header("Background")]
	public Color skyColor = new Color(0.68f, 0.83f, 0.94f, 1f);
	public Color fogColor = new Color(0.83f, 0.72f, 0.58f, 1f);
	public Color farDuneColor = new Color(0.72f, 0.57f, 0.37f, 1f);
	public Color nearDuneColor = new Color(0.77f, 0.61f, 0.39f, 1f);
	public Color sunColor = new Color(1f, 0.80f, 0.47f, 1f);
	public Color waterColor = new Color(0.43f, 0.74f, 0.87f, 0.86f);

	[Header("Decoration")]
	public float decorationVoxelSize = 0.08f;
	public int decorationCount = 22;
	public float decorationMinZ = 18f;
	public float decorationMaxZ = 142f;
	public float decorationRecycleZ = -22f;
	public float decorationInnerX = 15f;
	public float decorationOuterX = 32f;

	private readonly List<Transform> track = new List<Transform>();
	private readonly List<Transform> desert = new List<Transform>();
	private readonly List<Transform> decorations = new List<Transform>();
	private readonly List<LoopStrip> animatedStrips = new List<LoopStrip>();
	private readonly string[] decorationResources =
	{
		"Dino3D/Voxels/misc/rocks_0",
		"Dino3D/Voxels/misc/rocks_1",
		"Dino3D/Voxels/misc/rocks_2",
		"Dino3D/Voxels/misc/flowers_0",
		"Dino3D/Voxels/misc/flowers_1",
		"Dino3D/Voxels/misc/flowers_2",
		"Dino3D/Voxels/misc/tumbleweed",
		"Dino3D/Voxels/misc/desert_skull",
		"Dino3D/Voxels/misc/dead_tree",
		"Dino3D/Voxels/cactus/fcactus",
		"Dino3D/Voxels/cactus/fcactus_tall",
		"Dino3D/Voxels/cactus/fcactus_thin"
	};
	private readonly DecorationBand[] decorationBands =
	{
		new DecorationBand(-16.9f, 0.08f, 1.5f, 1.02f),
		new DecorationBand(-29.4f, 0.34f, 1.9f, 0.96f),
		new DecorationBand(-41.8f, 0.72f, 2.2f, 0.90f),
		new DecorationBand(13.8f, 0.06f, 1.8f, 0.82f),
		new DecorationBand(24.6f, 0.26f, 2.0f, 0.74f)
	};

	private Transform root;
	private Transform decorationRoot;
	private Material desertMaterial;
	private Material trackBedMaterial;
	private Material trackMaterial;
	private Material farDuneMaterial;
	private Material nearDuneMaterial;
	private Material sunMaterial;
	private Material waterMaterial;
	private Material desertPatchLightMaterial;
	private Material desertPatchDarkMaterial;
	private Material roadAccentMaterial;
	private float trackLength = 10f;
	private bool built;

	private void Awake()
	{
		Build();
	}

	private void Update()
	{
		if (GameManager.Instance == null || !GameManager.Instance.IsRunning)
		{
			return;
		}

		float speed = speedController != null ? speedController.CurrentSpeed : GameManager.Instance.CurrentSpeed;
		Tick(Time.deltaTime, speed);
	}

	public void Build()
	{
		if (built)
		{
			return;
		}

		built = true;
		ApplyReferenceLayoutDefaults();
		DisableLegacyBlocks();
		ConfigureRenderSettings();
		CreateMaterials();
		CreateRoot();
		CreateProceduralBackground();
		CreateDesertSurface();
		CreateGroundPatches();
		CreateTrack();
		CreateDecorations();
	}

	public void ResetEnvironment()
	{
		Build();

		for (int i = 0; i < desert.Count; i++)
		{
			desert[i].position = new Vector3(0f, desertY, desertStartZ + i * desertLength);
		}

		for (int i = 0; i < track.Count; i++)
		{
			track[i].position = new Vector3(0f, GetTrackBaseY(), firstGroundZ + i * trackLength);
		}

		for (int i = 0; i < animatedStrips.Count; i++)
		{
			ResetStrip(animatedStrips[i]);
		}

		for (int i = 0; i < decorations.Count; i++)
		{
			PlaceDecoration(decorations[i], Random.Range(decorationMinZ, decorationMaxZ));
		}
	}

	public void Tick(float deltaTime, float speed)
	{
		float move = speed * deltaTime;

		for (int i = 0; i < animatedStrips.Count; i++)
		{
			MoveLooped(animatedStrips[i].Pieces, move, animatedStrips[i].ChunkLength, animatedStrips[i].RecycleZ);
		}

		for (int i = 0; i < decorations.Count; i++)
		{
			Transform item = decorations[i];
			item.position += Vector3.back * move;

			if (item.position.z < decorationRecycleZ)
			{
				PlaceDecoration(item, decorationMaxZ + Random.Range(6f, 16f));
			}

			if (item.name.IndexOf("tumbleweed") >= 0)
			{
				item.Rotate(Vector3.right, 160f * deltaTime, Space.Self);
			}
		}
	}

	private void ApplyReferenceLayoutDefaults()
	{
		groundChunks = 1;
		firstGroundZ = 58f;
		groundTopY = 0f;
		trackWidth = 9.4f;
		trackChunkLength = 176f;
		trackHeight = 0.14f;
		trackColor = new Color(0.73f, 0.58f, 0.38f, 1f);
		trackBedColor = new Color(0.82f, 0.67f, 0.44f, 1f);

		desertChunks = 1;
		desertWidth = 88f;
		desertLength = 236f;
		desertStartZ = 58f;
		desertY = -0.28f;
		desertColor = new Color(0.82f, 0.67f, 0.44f, 1f);

		skyColor = new Color(0.68f, 0.83f, 0.94f, 1f);
		fogColor = new Color(0.83f, 0.72f, 0.58f, 1f);
		farDuneColor = new Color(0.72f, 0.57f, 0.37f, 1f);
		nearDuneColor = new Color(0.77f, 0.61f, 0.39f, 1f);
		sunColor = new Color(1f, 0.80f, 0.47f, 1f);
		waterColor = new Color(0.43f, 0.74f, 0.87f, 0.86f);

		decorationCount = 22;
		decorationMinZ = 18f;
		decorationMaxZ = 142f;
		decorationRecycleZ = -22f;
		decorationInnerX = 15f;
		decorationOuterX = 32f;
	}

	private void CreateMaterials()
	{
		desertMaterial = CreateLitColorMaterial("Dino3D Desert Sand", desertColor, 0.01f);
		trackBedMaterial = CreateLitColorMaterial("Dino3D Runner Bed", trackBedColor, 0.01f);
		trackMaterial = CreateLitColorMaterial("Dino3D Runner Lane", trackColor, 0.01f);
		farDuneMaterial = CreateLitColorMaterial("Dino3D Far Dunes", farDuneColor, 0.02f);
		nearDuneMaterial = CreateLitColorMaterial("Dino3D Near Dunes", nearDuneColor, 0.02f);
		sunMaterial = CreateUnlitColorMaterial("Dino3D Sun", sunColor);
		waterMaterial = CreateTransparentColorMaterial("Dino3D Water", waterColor);
		desertPatchLightMaterial = CreateLitColorMaterial("Dino3D Sand Patch Light", new Color(0.86f, 0.71f, 0.48f, 1f), 0f);
		desertPatchDarkMaterial = CreateLitColorMaterial("Dino3D Sand Patch Dark", new Color(0.76f, 0.61f, 0.40f, 1f), 0f);
		roadAccentMaterial = CreateLitColorMaterial("Dino3D Road Accent", new Color(0.79f, 0.64f, 0.43f, 1f), 0f);
	}

	private Material CreateLitColorMaterial(string materialName, Color color, float glossiness)
	{
		Shader shader = Shader.Find("Legacy Shaders/Diffuse");
		if (shader == null)
		{
			shader = Shader.Find("Mobile/Diffuse");
		}

		if (shader == null)
		{
			shader = Shader.Find("Standard");
		}

		if (shader == null)
		{
			shader = Shader.Find("Diffuse");
		}

		if (shader == null)
		{
			shader = Shader.Find("Unlit/Color");
		}

		Material material = new Material(shader);
		material.name = materialName;
		material.color = color;
		if (material.HasProperty("_Mode"))
		{
			material.SetFloat("_Mode", 0f);
		}

		if (material.HasProperty("_SrcBlend"))
		{
			material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
		}

		if (material.HasProperty("_DstBlend"))
		{
			material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
		}

		if (material.HasProperty("_ZWrite"))
		{
			material.SetInt("_ZWrite", 1);
		}

		if (material.HasProperty("_Glossiness"))
		{
			material.SetFloat("_Glossiness", glossiness);
		}

		if (material.HasProperty("_Metallic"))
		{
			material.SetFloat("_Metallic", 0f);
		}

		if (material.HasProperty("_Cull"))
		{
			material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Back);
		}

		material.DisableKeyword("_ALPHATEST_ON");
		material.DisableKeyword("_ALPHABLEND_ON");
		material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
		material.renderQueue = -1;
		return material;
	}

	private Material CreateUnlitColorMaterial(string materialName, Color color)
	{
		Shader shader = Shader.Find("Unlit/Color");
		if (shader == null)
		{
			shader = Shader.Find("Diffuse");
		}

		Material material = new Material(shader);
		material.name = materialName;
		material.color = color;
		if (material.HasProperty("_ZWrite"))
		{
			material.SetInt("_ZWrite", 1);
		}

		if (material.HasProperty("_Cull"))
		{
			material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Back);
		}

		material.renderQueue = -1;
		return material;
	}

	private Material CreateTransparentColorMaterial(string materialName, Color color)
	{
		Shader shader = Shader.Find("Standard");
		if (shader == null)
		{
			shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
		}

		if (shader == null)
		{
			shader = Shader.Find("Transparent/Diffuse");
		}

		if (shader == null)
		{
			shader = Shader.Find("Unlit/Transparent");
		}

		Material material = new Material(shader);
		material.name = materialName;
		material.color = color;

		if (material.shader != null && material.shader.name == "Standard")
		{
			material.SetFloat("_Mode", 2f);
			material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			material.SetInt("_ZWrite", 0);
			material.DisableKeyword("_ALPHATEST_ON");
			material.EnableKeyword("_ALPHABLEND_ON");
			material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
			if (material.HasProperty("_Glossiness"))
			{
				material.SetFloat("_Glossiness", 0.02f);
			}

			if (material.HasProperty("_Metallic"))
			{
				material.SetFloat("_Metallic", 0f);
			}
		}

		return material;
	}

	private void CreateRoot()
	{
		root = new GameObject("Dino3D Environment").transform;
		root.SetParent(transform);

		decorationRoot = new GameObject("Side Decoration").transform;
		decorationRoot.SetParent(root);
	}

	private void CreateDesertSurface()
	{
		for (int i = 0; i < desertChunks; i++)
		{
			GameObject chunk = new GameObject("Procedural Desert " + i);
			chunk.transform.SetParent(root);
			chunk.transform.position = new Vector3(0f, desertY, desertStartZ + i * desertLength);

			MeshFilter filter = chunk.AddComponent<MeshFilter>();
			filter.sharedMesh = CreateDesertMesh(desertWidth, desertLength, i);

			MeshRenderer renderer = chunk.AddComponent<MeshRenderer>();
			renderer.sharedMaterial = desertMaterial;
			renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			renderer.receiveShadows = false;
			desert.Add(chunk.transform);
		}
	}

	private Mesh CreateDesertMesh(float width, float length, int seed)
	{
		int columns = 18;
		int rows = 22;
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();

		for (int z = 0; z <= rows; z++)
		{
			float zPosition = -length * 0.5f + length * z / rows;
			for (int x = 0; x <= columns; x++)
			{
				float xPosition = -width * 0.5f + width * x / columns;
				float distanceFromTrack = Mathf.Abs(xPosition);
				float flatLaneWidth = trackWidth * 0.5f + 4f;
				float sideFactor = Mathf.Clamp01((distanceFromTrack - flatLaneWidth) / 16f);
				float worldZ = desertStartZ + zPosition + seed * length;
				float worldX = xPosition;
				float dune = Mathf.Sin(worldZ * 0.032f + worldX * 0.052f) * Mathf.Lerp(0.002f, 0.028f, sideFactor);
				float secondary = Mathf.Cos(worldZ * 0.018f - worldX * 0.028f) * Mathf.Lerp(0.002f, 0.016f, sideFactor);
				float sideLift = sideFactor * 0.12f;
				vertices.Add(new Vector3(xPosition, dune + secondary + sideLift, zPosition));
			}
		}

		for (int z = 0; z < rows; z++)
		{
			for (int x = 0; x < columns; x++)
			{
				int a = z * (columns + 1) + x;
				int b = a + 1;
				int c = a + columns + 1;
				int d = c + 1;

				triangles.Add(a);
				triangles.Add(c);
				triangles.Add(b);
				triangles.Add(b);
				triangles.Add(c);
				triangles.Add(d);
			}
		}

		Mesh mesh = new Mesh();
		mesh.name = "Procedural Desert Mesh";
		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	}

	private void CreateGroundPatches()
	{
		for (int i = 0; i < 26; i++)
		{
			bool rightSide = i % 2 == 0;
			float z = 16f + i * 4.8f;
			float x;
			if (rightSide)
			{
				float lane = (i / 2) % 3;
				x = 11.5f + lane * 8.4f + Mathf.Sin(i * 1.73f) * 1.4f;
			}
			else
			{
				float lane = (i / 2) % 3;
				x = -16.0f - lane * 8.8f + Mathf.Cos(i * 1.41f) * 1.2f;
			}

			float patchWidth = 1.8f + Mathf.Abs(Mathf.Sin(i * 0.82f)) * 3.4f;
			float patchLength = 2.2f + Mathf.Abs(Mathf.Cos(i * 0.53f)) * 5.4f;
			float patchHeight = 0.03f + Mathf.Abs(Mathf.Sin(i * 0.37f)) * 0.02f;
			Material patchMaterial = i % 3 == 0 ? desertPatchLightMaterial : desertPatchDarkMaterial;
			CreateGroundPatch("Sand Patch " + i, new Vector3(x, desertY + 0.02f, z), new Vector3(patchWidth, patchHeight, patchLength), patchMaterial);
		}
	}

	private void CreateGroundPatch(string objectName, Vector3 position, Vector3 scale, Material material)
	{
		GameObject patch = GameObject.CreatePrimitive(PrimitiveType.Cube);
		patch.name = objectName;
		patch.transform.SetParent(root);
		patch.transform.position = position;
		patch.transform.localScale = scale;

		MeshRenderer renderer = patch.GetComponent<MeshRenderer>();
		renderer.sharedMaterial = material;
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		renderer.receiveShadows = false;

		Collider collider = patch.GetComponent<Collider>();
		if (collider != null)
		{
			Destroy(collider);
		}
	}

	private void CreateTrack()
	{
		trackLength = trackChunkLength;
		animatedStrips.Clear();

		for (int i = 0; i < groundChunks; i++)
		{
			GameObject chunk = new GameObject("Runner Lane " + i);
			chunk.transform.SetParent(root);
			chunk.transform.position = new Vector3(0f, GetTrackBaseY(), firstGroundZ + i * trackLength);
			CreateTrackChunk(chunk.transform);
			track.Add(chunk.transform);
		}
	}

	private LoopStrip CreateVoxelStrip(
		Transform parent,
		string stripName,
		string resourcePath,
		int chunkCount,
		float startZ,
		float x,
		float topY,
		Vector3 scale,
		float voxelSize,
		float recycleZ)
	{
		LoopStrip strip = new LoopStrip();
		strip.StartZ = startZ;
		strip.X = x;
		strip.Y = 0f;
		strip.RecycleZ = recycleZ;
		strip.ChunkLength = 12f;

		for (int i = 0; i < chunkCount; i++)
		{
			GameObject chunk = new GameObject(stripName + " " + i);
			chunk.transform.SetParent(parent);

			VoxMeshData data = CreateVoxelVisual(chunk.transform, resourcePath, voxelSize, scale, topY);
			if (data != null)
			{
				strip.ChunkLength = Mathf.Max(strip.ChunkLength, data.Mesh.bounds.size.z * scale.z);
			}

			strip.Pieces.Add(chunk.transform);
		}

		ResetStrip(strip);
		return strip;
	}

	private VoxMeshData CreateVoxelVisual(Transform parent, string resourcePath, float voxelSize, Vector3 scale, float topY)
	{
		GameObject visual = new GameObject(resourcePath.Substring(resourcePath.LastIndexOf('/') + 1));
		visual.transform.SetParent(parent, false);

		VoxMeshData data = VoxMeshFactory.ApplyToObject(visual, resourcePath, voxelSize);
		visual.transform.localScale = scale;

		if (data != null)
		{
			float groundedY = topY - data.Mesh.bounds.max.y * scale.y;
			visual.transform.localPosition = new Vector3(0f, groundedY, 0f);
		}

		return data;
	}

	private void ResetStrip(LoopStrip strip)
	{
		for (int i = 0; i < strip.Pieces.Count; i++)
		{
			strip.Pieces[i].position = new Vector3(strip.X, strip.Y, strip.StartZ + i * strip.ChunkLength);
		}
	}

	private void CreateDecorations()
	{
		for (int i = 0; i < decorationCount; i++)
		{
			string resource = decorationResources[Random.Range(0, decorationResources.Length)];
			GameObject item = new GameObject("Dino3D " + resource.Substring(resource.LastIndexOf('/') + 1));
			item.transform.SetParent(decorationRoot);

			VoxMeshFactory.ApplyToObject(item, resource, decorationVoxelSize);
			PlaceDecoration(item.transform, Random.Range(decorationMinZ, decorationMaxZ));
			decorations.Add(item.transform);
		}
	}

	private void PlaceDecoration(Transform item, float z)
	{
		int bandIndex = PickDecorationBand(item.name, z);
		DecorationBand band = decorationBands[bandIndex];
		float x = band.X + Random.Range(-band.JitterX, band.JitterX);
		float y = band.Y;
		float scale = Random.Range(0.8f, 1.15f) * band.ScaleMultiplier;

		if (item.name.IndexOf("dead_tree") >= 0)
		{
			scale = Random.Range(1.28f, 1.82f) * band.ScaleMultiplier;
			y += 0.02f;
		}
		else if (item.name.IndexOf("tumbleweed") >= 0)
		{
			scale = Random.Range(0.94f, 1.18f) * band.ScaleMultiplier;
			y += 0.06f;
		}
		else if (item.name.IndexOf("desert_skull") >= 0)
		{
			scale = Random.Range(0.52f, 0.78f) * band.ScaleMultiplier;
			y += 0.04f;
		}
		else if (item.name.IndexOf("flowers") >= 0)
		{
			scale = Random.Range(1.12f, 1.55f) * band.ScaleMultiplier;
			y += 0.03f;
		}
		else if (item.name.IndexOf("rocks") >= 0)
		{
			scale = Random.Range(1.08f, 1.56f) * band.ScaleMultiplier;
			y += 0.04f;
		}
		else if (item.name.IndexOf("cactus") >= 0)
		{
			scale = Random.Range(0.92f, 1.28f) * band.ScaleMultiplier;
		}

		item.position = new Vector3(x, y, z);
		item.localScale = Vector3.one * scale;

		float yaw = Random.Range(0f, 360f);
		if (item.name.IndexOf("cactus") >= 0)
		{
			yaw = Random.Range(-22f, 18f);
		}
		else if (item.name.IndexOf("dead_tree") >= 0)
		{
			yaw = Random.Range(-35f, 24f);
		}

		item.rotation = Quaternion.Euler(0f, yaw, 0f);
	}

	private int PickDecorationBand(string itemName, float z)
	{
		if (itemName.IndexOf("flowers") >= 0)
		{
			return Random.value > 0.45f ? 3 : 4;
		}

		if (itemName.IndexOf("desert_skull") >= 0)
		{
			return Random.value > 0.65f ? 4 : (Random.value > 0.45f ? 1 : 0);
		}

		if (itemName.IndexOf("rocks") >= 0 || itemName.IndexOf("tumbleweed") >= 0)
		{
			return Random.value > 0.62f ? 3 : (Random.value > 0.35f ? 1 : 0);
		}

		if (z < 32f)
		{
			return itemName.IndexOf("dead_tree") >= 0 ? 2 : 1;
		}

		if (itemName.IndexOf("dead_tree") >= 0)
		{
			return Random.value > 0.4f ? 2 : 1;
		}

		if (itemName.IndexOf("tumbleweed") >= 0 || itemName.IndexOf("desert_skull") >= 0)
		{
			return Random.value > 0.7f ? 1 : 0;
		}

		return Random.value > 0.78f ? 2 : (Random.value > 0.48f ? 1 : 0);
	}

	private void MoveLooped(List<Transform> items, float move, float length, float recycleZ)
	{
		float frontZ = float.MinValue;
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].position.z > frontZ)
			{
				frontZ = items[i].position.z;
			}
		}

		for (int i = 0; i < items.Count; i++)
		{
			Transform item = items[i];
			item.position += Vector3.back * move;

			if (item.position.z < recycleZ)
			{
				frontZ += length;
				item.position = new Vector3(item.position.x, item.position.y, frontZ);
			}
		}
	}

	private void CreateProceduralBackground()
	{
		GameObject sun = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sun.name = "Low Desert Sun";
		sun.transform.SetParent(root);
		sun.transform.position = new Vector3(-24f, 12f, 82f);
		sun.transform.localScale = Vector3.one * 6f;
		sun.GetComponent<MeshRenderer>().sharedMaterial = sunMaterial;
		Collider sunCollider = sun.GetComponent<Collider>();
		if (sunCollider != null)
		{
			Destroy(sunCollider);
		}

		CreateTerrainShelf("Decor Shelf First", new Vector3(-17.6f, 0.02f, 64f), 12.5f, 214f, 0.36f, nearDuneMaterial, 0.010f, 0.012f, 0.016f);
		CreateTerrainShelf("Decor Shelf Second", new Vector3(-30.4f, 0.34f, 64f), 14.2f, 214f, 0.40f, farDuneMaterial, 0.012f, 0.014f, 0.018f);
		CreateTerrainShelf("Decor Shelf Third", new Vector3(-43.2f, 0.74f, 64f), 16.4f, 214f, 0.44f, nearDuneMaterial, 0.013f, 0.016f, 0.020f);
		CreateRibbon("Water Ribbon", new Vector3(-9.1f, -0.08f, 64f), 6.3f, 212f, 0.20f, waterMaterial, 0.016f, 0.008f, 0.024f);
		CreateRibbon("Water Bank Inner", new Vector3(-5.35f, -0.03f, 64f), 1.9f, 212f, 0.10f, nearDuneMaterial, 0.010f, 0.004f, 0.012f);
		CreateRibbon("Water Bank Outer", new Vector3(-13.0f, -0.02f, 64f), 2.6f, 212f, 0.12f, farDuneMaterial, 0.012f, 0.005f, 0.014f);
		CreateBackdropPiece("Far Cactus Left", "Dino3D/Voxels/cactus/fcactus_tall", new Vector3(-31f, 0.72f, 118f), Quaternion.Euler(0f, -12f, 0f), 1.08f);
		CreateBackdropPiece("Far Tree Left", "Dino3D/Voxels/misc/dead_tree", new Vector3(-45f, 1.06f, 136f), Quaternion.Euler(0f, 10f, 0f), 1.18f);
		CreateBackdropPiece("Far Cactus Mid", "Dino3D/Voxels/cactus/fcactus", new Vector3(-20f, 0.42f, 146f), Quaternion.Euler(0f, 8f, 0f), 0.98f);
		CreateBackdropPiece("Far Cactus Right", "Dino3D/Voxels/cactus/fcactus_thin", new Vector3(22f, 0.30f, 128f), Quaternion.Euler(0f, -8f, 0f), 0.88f);
		CreateBackdropPiece("Far Rock Right", "Dino3D/Voxels/misc/rocks_2", new Vector3(30f, 0.18f, 112f), Quaternion.Euler(0f, 18f, 0f), 0.94f);
	}

	private void CreateTerrainShelf(string objectName, Vector3 center, float width, float length, float thickness, Material material, float wave, float edgeLift, float sideSlope)
	{
		CreateRibbon(objectName, center, width, length, thickness, material, wave, edgeLift, sideSlope);
	}

	private void CreateBackdropPiece(string objectName, string resourcePath, Vector3 position, Quaternion rotation, float scale)
	{
		GameObject visual = new GameObject(objectName);
		visual.transform.SetParent(root);
		visual.transform.position = position;
		visual.transform.rotation = rotation;
		visual.transform.localScale = Vector3.one * scale;

		VoxMeshData data = VoxMeshFactory.ApplyToObject(visual, resourcePath, decorationVoxelSize);
		if (data != null)
		{
			Vector3 grounded = visual.transform.position;
			grounded.y -= data.Mesh.bounds.min.y * scale;
			visual.transform.position = grounded;
		}
	}

	private void CreateRibbon(string objectName, Vector3 center, float width, float length, float thickness, Material material, float wave, float edgeLift, float sideSlope)
	{
		GameObject ribbon = new GameObject(objectName);
		ribbon.transform.SetParent(root);
		ribbon.transform.position = center;

		MeshFilter filter = ribbon.AddComponent<MeshFilter>();
		filter.sharedMesh = CreateRibbonMesh(width, length, thickness, wave, edgeLift, sideSlope);

		MeshRenderer renderer = ribbon.AddComponent<MeshRenderer>();
		renderer.sharedMaterial = material;
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		renderer.receiveShadows = false;
	}

	private Mesh CreateRibbonMesh(float width, float length, float thickness, float wave, float edgeLift, float sideSlope)
	{
		int columns = 8;
		int rows = 18;
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();

		for (int z = 0; z <= rows; z++)
		{
			float zPos = -length * 0.5f + length * z / rows;
			for (int x = 0; x <= columns; x++)
			{
				float xPos = -width * 0.5f + width * x / columns;
				float edge = Mathf.Abs(xPos / Mathf.Max(0.001f, width * 0.5f));
				float yPos = Mathf.Sin((zPos + xPos * 0.35f) * 0.06f) * wave;
				yPos += edge * edgeLift;
				yPos -= Mathf.Clamp01(1f - edge) * sideSlope;
				vertices.Add(new Vector3(xPos, yPos, zPos));
			}
		}

		for (int z = 0; z < rows; z++)
		{
			for (int x = 0; x < columns; x++)
			{
				int a = z * (columns + 1) + x;
				int b = a + 1;
				int c = a + columns + 1;
				int d = c + 1;

				triangles.Add(a);
				triangles.Add(c);
				triangles.Add(b);
				triangles.Add(b);
				triangles.Add(c);
				triangles.Add(d);
			}
		}

		int topVertexCount = vertices.Count;
		for (int i = 0; i < topVertexCount; i++)
		{
			Vector3 bottom = vertices[i];
			bottom.y -= thickness;
			vertices.Add(bottom);
		}

		for (int z = 0; z < rows; z++)
		{
			for (int x = 0; x < columns; x++)
			{
				int a = topVertexCount + z * (columns + 1) + x;
				int b = a + 1;
				int c = a + columns + 1;
				int d = c + 1;

				triangles.Add(a);
				triangles.Add(b);
				triangles.Add(c);
				triangles.Add(b);
				triangles.Add(d);
				triangles.Add(c);
			}
		}

		for (int z = 0; z < rows; z++)
		{
			int topLeft = z * (columns + 1);
			int topLeftNext = (z + 1) * (columns + 1);
			int bottomLeft = topVertexCount + topLeft;
			int bottomLeftNext = topVertexCount + topLeftNext;
			triangles.Add(topLeft);
			triangles.Add(bottomLeft);
			triangles.Add(topLeftNext);
			triangles.Add(topLeftNext);
			triangles.Add(bottomLeft);
			triangles.Add(bottomLeftNext);

			int topRight = z * (columns + 1) + columns;
			int topRightNext = (z + 1) * (columns + 1) + columns;
			int bottomRight = topVertexCount + topRight;
			int bottomRightNext = topVertexCount + topRightNext;
			triangles.Add(topRight);
			triangles.Add(topRightNext);
			triangles.Add(bottomRight);
			triangles.Add(topRightNext);
			triangles.Add(bottomRightNext);
			triangles.Add(bottomRight);
		}

		for (int x = 0; x < columns; x++)
		{
			int topNearA = x;
			int topNearB = x + 1;
			int bottomNearA = topVertexCount + x;
			int bottomNearB = topVertexCount + x + 1;
			triangles.Add(topNearA);
			triangles.Add(topNearB);
			triangles.Add(bottomNearA);
			triangles.Add(topNearB);
			triangles.Add(bottomNearB);
			triangles.Add(bottomNearA);

			int topFarA = rows * (columns + 1) + x;
			int topFarB = topFarA + 1;
			int bottomFarA = topVertexCount + topFarA;
			int bottomFarB = topVertexCount + topFarB;
			triangles.Add(topFarA);
			triangles.Add(bottomFarA);
			triangles.Add(topFarB);
			triangles.Add(topFarB);
			triangles.Add(bottomFarA);
			triangles.Add(bottomFarB);
		}

		Mesh mesh = new Mesh();
		mesh.name = "Ribbon Mesh";
		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	}

	private void CreateTrackChunk(Transform parent)
	{
		CreateTrackBlock("Track Bed", parent, new Vector3(0f, -trackHeight * 0.10f, 0f), new Vector3(trackWidth + 5.2f, trackHeight * 0.90f, trackLength), trackBedMaterial);
		CreateTrackBlock("Lane", parent, new Vector3(0f, trackHeight * 0.12f, 0f), new Vector3(trackWidth, trackHeight * 0.50f, trackLength * 0.988f), trackMaterial);
		CreateRoadPatchPattern(parent);
	}

	private void CreateRoadPatchPattern(Transform parent)
	{
		for (int i = 0; i < 16; i++)
		{
			float localX = Mathf.Sin(i * 1.07f) * 2.1f;
			float localZ = -trackLength * 0.42f + i * (trackLength / 15f) + Mathf.Cos(i * 0.57f) * 1.2f;
			float patchWidth = 1.2f + Mathf.Abs(Mathf.Sin(i * 0.83f)) * 1.6f;
			float patchLength = 3.6f + Mathf.Abs(Mathf.Cos(i * 0.49f)) * 4.2f;
			float patchHeight = trackHeight * 0.12f;
			CreateTrackBlock(
				"Road Patch " + i,
				parent,
				new Vector3(localX, trackHeight * 0.31f, localZ),
				new Vector3(patchWidth, patchHeight, patchLength),
				i % 2 == 0 ? roadAccentMaterial : trackBedMaterial);
		}
	}

	private void CreateTrackBlock(string blockName, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
	{
		GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
		block.name = blockName;
		block.transform.SetParent(parent, false);
		block.transform.localPosition = localPosition;
		block.transform.localScale = localScale;

		MeshRenderer renderer = block.GetComponent<MeshRenderer>();
		renderer.sharedMaterial = material;
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		renderer.receiveShadows = false;

		Collider collider = block.GetComponent<Collider>();
		if (collider != null)
		{
			Destroy(collider);
		}
	}

	private float GetTrackBaseY()
	{
		return groundTopY - trackHeight * 0.18f;
	}

	private void ConfigureRenderSettings()
	{
		Camera camera = Camera.main;
		if (camera != null)
		{
			camera.clearFlags = CameraClearFlags.SolidColor;
			camera.backgroundColor = skyColor;
			camera.fieldOfView = 42f;
			camera.nearClipPlane = 0.1f;
			camera.farClipPlane = 176f;
			camera.transform.position = new Vector3(6.65f, 3.02f, -5.85f);
			camera.transform.LookAt(new Vector3(-0.55f, 0.98f, 14.8f));
		}

		RenderSettings.fog = true;
		RenderSettings.fogMode = FogMode.Linear;
		RenderSettings.fogStartDistance = 60f;
		RenderSettings.fogEndDistance = 154f;
		RenderSettings.fogColor = fogColor;
		RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
		RenderSettings.ambientLight = new Color(0.79f, 0.82f, 0.86f, 1f);
		RenderSettings.ambientIntensity = 1.10f;

		Light sunLight = FindDirectionalLight();
		if (sunLight == null)
		{
			GameObject lightObject = new GameObject("Dino3D Sun Light");
			lightObject.transform.SetParent(transform);
			sunLight = lightObject.AddComponent<Light>();
			sunLight.type = LightType.Directional;
		}

		if (sunLight != null)
		{
			sunLight.enabled = true;
			sunLight.color = new Color(1f, 0.93f, 0.80f, 1f);
			sunLight.intensity = 0.94f;
			sunLight.shadows = LightShadows.Soft;
			sunLight.shadowStrength = 0.18f;
			sunLight.transform.rotation = Quaternion.Euler(43f, -36f, 0f);
			RenderSettings.sun = sunLight;
		}
	}

	private Light FindDirectionalLight()
	{
		Light[] lights = FindObjectsOfType<Light>();
		for (int i = 0; i < lights.Length; i++)
		{
			if (lights[i] != null && lights[i].type == LightType.Directional)
			{
				return lights[i];
			}
		}

		return null;
	}

	private void DisableLegacyBlocks()
	{
		DisableByName("Ground");
		DisableByName("DesertWallL");
		DisableByName("DesertWallR");
	}

	private void DisableByName(string objectName)
	{
		GameObject legacy = GameObject.Find(objectName);
		if (legacy != null)
		{
			legacy.SetActive(false);
		}
	}
}
