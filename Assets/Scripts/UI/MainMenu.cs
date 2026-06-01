using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public Fade fade;
	public int gameplaySceneIndex = 1;
	public string dinoResource = "Dino3D/Voxels/t-rex/0";
	public string cactusResource = "Dino3D/Voxels/cactus/cactus_tall";
	public float dinoVoxelSize = 0.08f;
	public float cactusVoxelSize = 0.09f;

	private bool loadingScene;
	private bool presentationBuilt;
	private Transform presentationRoot;
	private Material sandMaterial;
	private Material laneMaterial;
	private Material duneMaterial;
	private Material accentDuneMaterial;
	private Material sunMaterial;
	private Material waterMaterial;
	private Material patchLightMaterial;
	private Material patchDarkMaterial;
	private Material roadAccentMaterial;

	private IEnumerator SceneTransition(int scene)
	{
		if (fade != null)
		{
			yield return new WaitForSeconds(fade.BeginFade(1));
		}

		SceneManager.LoadScene(scene, LoadSceneMode.Single);
	}

	public void ExitGame()
	{
		Application.Quit();
	}

	public void StartGame()
	{
		if (loadingScene)
		{
			return;
		}

		loadingScene = true;
		StartCoroutine(SceneTransition(gameplaySceneIndex));
	}

	public void StartGame(int ignoredValue)
	{
		StartGame();
	}

	private void Start()
	{
		BuildPresentation();

		if (fade != null)
		{
			fade.BeginFade(-1);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
		{
			StartGame();
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ExitGame();
		}
	}

	private void BuildPresentation()
	{
		if (presentationBuilt)
		{
			return;
		}

		presentationBuilt = true;
		ConfigureCameraAndLighting();
		CreateMaterials();

		presentationRoot = new GameObject("Menu Presentation").transform;
		presentationRoot.SetParent(transform);

		CreateGround();
		CreateSun();
		CreateShowpiece("Menu Dino", dinoResource, dinoVoxelSize, new Vector3(-0.95f, 0.34f, 10.9f), Quaternion.Euler(0f, -26f, 0f), 1.82f);
		CreateShowpiece("Menu Cactus", cactusResource, cactusVoxelSize, new Vector3(3.25f, 0.02f, 14.7f), Quaternion.Euler(0f, 8f, 0f), 1.5f);
		CreateShowpiece("Menu Side Cactus", "Dino3D/Voxels/cactus/fcactus_tall", 0.08f, new Vector3(-17.4f, 0.3f, 16.8f), Quaternion.Euler(0f, -12f, 0f), 1.34f);
		CreateDecorPiece("Menu Flower", "Dino3D/Voxels/misc/flowers_1", 0.08f, new Vector3(8.0f, 0.02f, 11.8f), Quaternion.Euler(0f, -10f, 0f), 1.12f);
		CreateDecorPiece("Menu Rocks", "Dino3D/Voxels/misc/rocks_1", 0.08f, new Vector3(-8.0f, 0.02f, 13.8f), Quaternion.Euler(0f, 20f, 0f), 1.08f);
		CreateDecorPiece("Menu Skull", "Dino3D/Voxels/misc/desert_skull", 0.08f, new Vector3(7.4f, 0.02f, 17.2f), Quaternion.Euler(0f, 18f, 0f), 0.62f);
	}

	private void ConfigureCameraAndLighting()
	{
		Camera camera = Camera.main;
		if (camera != null)
		{
			camera.clearFlags = CameraClearFlags.SolidColor;
			camera.backgroundColor = new Color(0.68f, 0.83f, 0.94f, 1f);
			camera.fieldOfView = 42f;
			camera.nearClipPlane = 0.1f;
			camera.farClipPlane = 156f;
			camera.transform.position = new Vector3(6.55f, 2.95f, -5.4f);
			camera.transform.LookAt(new Vector3(-0.68f, 0.96f, 14.4f));
		}

		RenderSettings.fog = true;
		RenderSettings.fogMode = FogMode.Linear;
		RenderSettings.fogStartDistance = 58f;
		RenderSettings.fogEndDistance = 148f;
		RenderSettings.fogColor = new Color(0.83f, 0.72f, 0.58f, 1f);
		RenderSettings.ambientMode = AmbientMode.Flat;
		RenderSettings.ambientLight = new Color(0.79f, 0.82f, 0.86f, 1f);
		RenderSettings.ambientIntensity = 1.08f;

		Light keyLight = FindDirectionalLight();
		if (keyLight == null)
		{
			GameObject lightObject = new GameObject("Menu Sun Light");
			lightObject.transform.SetParent(transform);
			keyLight = lightObject.AddComponent<Light>();
			keyLight.type = LightType.Directional;
		}

		keyLight.enabled = true;
		keyLight.color = new Color(1f, 0.93f, 0.80f, 1f);
		keyLight.intensity = 0.94f;
		keyLight.shadows = LightShadows.Soft;
		keyLight.shadowStrength = 0.18f;
		keyLight.transform.rotation = Quaternion.Euler(43f, -36f, 0f);
		RenderSettings.sun = keyLight;
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

	private void CreateMaterials()
	{
		sandMaterial = CreateLitMaterial("Menu Sand", new Color(0.82f, 0.67f, 0.44f, 1f));
		laneMaterial = CreateLitMaterial("Menu Lane", new Color(0.73f, 0.58f, 0.38f, 1f));
		duneMaterial = CreateLitMaterial("Menu Dune", new Color(0.77f, 0.61f, 0.39f, 1f));
		accentDuneMaterial = CreateLitMaterial("Menu Accent Dune", new Color(0.72f, 0.57f, 0.37f, 1f));
		sunMaterial = CreateUnlitMaterial("Menu Sun", new Color(1f, 0.80f, 0.47f, 1f));
		waterMaterial = CreateTransparentMaterial("Menu Water", new Color(0.43f, 0.74f, 0.87f, 0.86f));
		patchLightMaterial = CreateLitMaterial("Menu Patch Light", new Color(0.86f, 0.71f, 0.48f, 1f));
		patchDarkMaterial = CreateLitMaterial("Menu Patch Dark", new Color(0.76f, 0.61f, 0.40f, 1f));
		roadAccentMaterial = CreateLitMaterial("Menu Road Accent", new Color(0.79f, 0.64f, 0.43f, 1f));
	}

	private Material CreateLitMaterial(string materialName, Color color)
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
			material.SetInt("_SrcBlend", (int)BlendMode.One);
		}

		if (material.HasProperty("_DstBlend"))
		{
			material.SetInt("_DstBlend", (int)BlendMode.Zero);
		}

		if (material.HasProperty("_ZWrite"))
		{
			material.SetInt("_ZWrite", 1);
		}

		if (material.HasProperty("_Glossiness"))
		{
			material.SetFloat("_Glossiness", 0f);
		}

		if (material.HasProperty("_Metallic"))
		{
			material.SetFloat("_Metallic", 0f);
		}

		if (material.HasProperty("_Cull"))
		{
			material.SetInt("_Cull", (int)CullMode.Back);
		}

		material.DisableKeyword("_ALPHATEST_ON");
		material.DisableKeyword("_ALPHABLEND_ON");
		material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
		material.renderQueue = -1;
		return material;
	}

	private Material CreateUnlitMaterial(string materialName, Color color)
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
			material.SetInt("_Cull", (int)CullMode.Back);
		}

		material.renderQueue = -1;
		return material;
	}

	private Material CreateTransparentMaterial(string materialName, Color color)
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
			material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
			material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
			material.SetInt("_ZWrite", 0);
			material.DisableKeyword("_ALPHATEST_ON");
			material.EnableKeyword("_ALPHABLEND_ON");
			material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = (int)RenderQueue.Transparent;
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

	private void CreateGround()
	{
		CreateBlock("Menu Desert Ground", new Vector3(0f, -0.35f, 64f), new Vector3(80f, 0.72f, 186f), sandMaterial);
		CreateBlock("Menu Runner Bed", new Vector3(0f, -0.06f, 52f), new Vector3(14.6f, 0.12f, 158f), sandMaterial);
		CreateBlock("Menu Runner Lane", new Vector3(0f, 0.01f, 52f), new Vector3(9.4f, 0.06f, 154f), laneMaterial);
		CreateRoadPatchPattern();
		CreateGroundPatches();
		CreateTerrainShelf("Menu Decor Shelf First", new Vector3(-17.6f, 0.02f, 60f), 12.5f, 176f, 0.36f, duneMaterial, 0.010f, 0.012f, 0.016f);
		CreateTerrainShelf("Menu Decor Shelf Second", new Vector3(-30.4f, 0.34f, 60f), 14.2f, 176f, 0.40f, accentDuneMaterial, 0.012f, 0.014f, 0.018f);
		CreateTerrainShelf("Menu Decor Shelf Third", new Vector3(-43.2f, 0.74f, 60f), 16.4f, 176f, 0.44f, duneMaterial, 0.013f, 0.016f, 0.020f);
		CreateRibbon("Menu Water Ribbon", new Vector3(-9.1f, -0.08f, 60f), 6.3f, 176f, 0.20f, waterMaterial, 0.016f, 0.008f, 0.024f);
		CreateRibbon("Menu Water Bank Inner", new Vector3(-5.35f, -0.03f, 60f), 1.9f, 176f, 0.10f, duneMaterial, 0.010f, 0.004f, 0.012f);
		CreateRibbon("Menu Water Bank Outer", new Vector3(-13.0f, -0.02f, 60f), 2.6f, 176f, 0.12f, accentDuneMaterial, 0.012f, 0.005f, 0.014f);
		CreateBackdropPiece("Menu Far Cactus Left", "Dino3D/Voxels/cactus/fcactus_tall", new Vector3(-31f, 0.72f, 102f), Quaternion.Euler(0f, -12f, 0f), 1.02f);
		CreateBackdropPiece("Menu Far Tree Left", "Dino3D/Voxels/misc/dead_tree", new Vector3(-45f, 1.06f, 122f), Quaternion.Euler(0f, 10f, 0f), 1.12f);
		CreateBackdropPiece("Menu Far Cactus Right", "Dino3D/Voxels/cactus/fcactus_thin", new Vector3(20f, 0.28f, 112f), Quaternion.Euler(0f, -8f, 0f), 0.82f);
	}

	private void CreateSun()
	{
		GameObject sun = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sun.name = "Menu Sun";
		sun.transform.SetParent(presentationRoot);
		sun.transform.position = new Vector3(-24f, 11.5f, 84f);
		sun.transform.localScale = Vector3.one * 5.2f;

		MeshRenderer renderer = sun.GetComponent<MeshRenderer>();
		renderer.sharedMaterial = sunMaterial;
		renderer.shadowCastingMode = ShadowCastingMode.Off;
		renderer.receiveShadows = false;

		Collider collider = sun.GetComponent<Collider>();
		if (collider != null)
		{
			Destroy(collider);
		}
	}

	private void CreateShowpiece(string objectName, string resourcePath, float voxelSize, Vector3 position, Quaternion rotation, float scale)
	{
		GameObject visual = new GameObject(objectName);
		visual.transform.SetParent(presentationRoot);
		visual.transform.position = position;
		visual.transform.rotation = rotation;
		visual.transform.localScale = Vector3.one * scale;

		VoxMeshData data = VoxMeshFactory.ApplyToObject(visual, resourcePath, voxelSize);
		if (data != null)
		{
			Vector3 groundedPosition = visual.transform.position;
			groundedPosition.y -= data.Mesh.bounds.min.y * scale;
			visual.transform.position = groundedPosition;
		}

		CreateShadow(objectName + " Shadow", visual.transform.position + new Vector3(0f, 0.02f, 0.12f), new Vector3(0.82f, 0.03f, 1.12f) * scale);
	}

	private void CreateShadow(string objectName, Vector3 position, Vector3 scale)
	{
		GameObject shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		shadow.name = objectName;
		shadow.transform.SetParent(presentationRoot);
		shadow.transform.position = position;
		shadow.transform.localScale = scale;

		MeshRenderer renderer = shadow.GetComponent<MeshRenderer>();
		renderer.sharedMaterial = CreateLitMaterial(objectName + " Material", new Color(0.47f, 0.34f, 0.20f, 1f));
		renderer.shadowCastingMode = ShadowCastingMode.Off;
		renderer.receiveShadows = false;

		Collider collider = shadow.GetComponent<Collider>();
		if (collider != null)
		{
			Destroy(collider);
		}
	}

	private void CreateDecorPiece(string objectName, string resourcePath, float voxelSize, Vector3 position, Quaternion rotation, float scale)
	{
		GameObject visual = new GameObject(objectName);
		visual.transform.SetParent(presentationRoot);
		visual.transform.position = position;
		visual.transform.rotation = rotation;
		visual.transform.localScale = Vector3.one * scale;

		VoxMeshData data = VoxMeshFactory.ApplyToObject(visual, resourcePath, voxelSize);
		if (data != null)
		{
			Vector3 groundedPosition = visual.transform.position;
			groundedPosition.y -= data.Mesh.bounds.min.y * scale;
			visual.transform.position = groundedPosition;
		}
	}

	private void CreateBackdropPiece(string objectName, string resourcePath, Vector3 position, Quaternion rotation, float scale)
	{
		CreateDecorPiece(objectName, resourcePath, 0.08f, position, rotation, scale);
	}

	private void CreateRoadPatchPattern()
	{
		for (int i = 0; i < 14; i++)
		{
			float x = Mathf.Sin(i * 1.07f) * 2.0f;
			float z = -8f + i * 7.5f + Mathf.Cos(i * 0.57f) * 0.9f;
			float width = 1.2f + Mathf.Abs(Mathf.Sin(i * 0.83f)) * 1.5f;
			float length = 3.2f + Mathf.Abs(Mathf.Cos(i * 0.49f)) * 4.0f;
			CreateBlock(
				"Menu Road Patch " + i,
				new Vector3(x, 0.05f, z),
				new Vector3(width, 0.02f, length),
				i % 2 == 0 ? roadAccentMaterial : sandMaterial);
		}
	}

	private void CreateGroundPatches()
	{
		for (int i = 0; i < 20; i++)
		{
			bool rightSide = i % 2 == 0;
			float z = 10f + i * 5.0f;
			float x;
			if (rightSide)
			{
				float lane = (i / 2) % 3;
				x = 11.0f + lane * 8.2f + Mathf.Sin(i * 1.73f) * 1.2f;
			}
			else
			{
				float lane = (i / 2) % 3;
				x = -16.2f - lane * 8.6f + Mathf.Cos(i * 1.41f) * 1.1f;
			}

			float width = 1.8f + Mathf.Abs(Mathf.Sin(i * 0.82f)) * 3.0f;
			float length = 2.0f + Mathf.Abs(Mathf.Cos(i * 0.53f)) * 4.8f;
			float height = 0.03f + Mathf.Abs(Mathf.Sin(i * 0.37f)) * 0.02f;
			CreateBlock(
				"Menu Sand Patch " + i,
				new Vector3(x, -0.26f, z),
				new Vector3(width, height, length),
				i % 3 == 0 ? patchLightMaterial : patchDarkMaterial);
		}
	}

	private void CreateTerrainShelf(string objectName, Vector3 center, float width, float length, float thickness, Material material, float wave, float edgeLift, float sideSlope)
	{
		CreateRibbon(objectName, center, width, length, thickness, material, wave, edgeLift, sideSlope);
	}

	private void CreateBlock(string objectName, Vector3 position, Vector3 scale, Material material)
	{
		GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
		block.name = objectName;
		block.transform.SetParent(presentationRoot);
		block.transform.position = position;
		block.transform.localScale = scale;

		MeshRenderer renderer = block.GetComponent<MeshRenderer>();
		renderer.sharedMaterial = material;
		renderer.shadowCastingMode = ShadowCastingMode.Off;
		renderer.receiveShadows = false;

		Collider collider = block.GetComponent<Collider>();
		if (collider != null)
		{
			Destroy(collider);
		}
	}

	private void CreateVoxelStripPiece(string objectName, string resourcePath, float voxelSize, Vector3 position, Vector3 scale, float topY)
	{
		GameObject visual = new GameObject(objectName);
		visual.transform.SetParent(presentationRoot);
		visual.transform.position = new Vector3(position.x, 0f, position.z);

		VoxMeshData data = VoxMeshFactory.ApplyToObject(visual, resourcePath, voxelSize);
		visual.transform.localScale = scale;
		if (data != null)
		{
			float groundedY = topY - data.Mesh.bounds.max.y * scale.y;
			visual.transform.position = new Vector3(position.x, groundedY, position.z);
		}
	}

	private void CreateRibbon(string objectName, Vector3 center, float width, float length, float thickness, Material material, float wave, float edgeLift, float sideSlope)
	{
		GameObject ribbon = new GameObject(objectName);
		ribbon.transform.SetParent(presentationRoot);
		ribbon.transform.position = center;

		MeshFilter filter = ribbon.AddComponent<MeshFilter>();
		filter.sharedMesh = CreateRibbonMesh(width, length, thickness, wave, edgeLift, sideSlope);

		MeshRenderer renderer = ribbon.AddComponent<MeshRenderer>();
		renderer.sharedMaterial = material;
		renderer.shadowCastingMode = ShadowCastingMode.Off;
		renderer.receiveShadows = false;
	}

	private Mesh CreateRibbonMesh(float width, float length, float thickness, float wave, float edgeLift, float sideSlope)
	{
		int columns = 8;
		int rows = 16;
		System.Collections.Generic.List<Vector3> vertices = new System.Collections.Generic.List<Vector3>();
		System.Collections.Generic.List<int> triangles = new System.Collections.Generic.List<int>();

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
		mesh.name = "Menu Ribbon Mesh";
		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	}

}
