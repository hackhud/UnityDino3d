using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class VoxMeshData
{
	public Mesh Mesh;
	public Material[] Materials;
	public Vector3 Size;
}

public static class VoxMeshFactory
{
	private struct Voxel
	{
		public byte X;
		public byte Y;
		public byte Z;
		public byte Color;
	}

	private sealed class VoxModel
	{
		public int SizeX;
		public int SizeY;
		public int SizeZ;
		public List<Voxel> Voxels = new List<Voxel>();
		public Color32[] Palette = CreateDefaultPalette();
	}

	private static readonly Dictionary<string, VoxMeshData> Cache = new Dictionary<string, VoxMeshData>();

	public static VoxMeshData ApplyToObject(GameObject target, string resourcePath, float voxelSize)
	{
		VoxMeshData data = Load(resourcePath, voxelSize);
		if (data == null || target == null)
		{
			return data;
		}

		MeshFilter filter = target.GetComponent<MeshFilter>();
		if (filter == null)
		{
			filter = target.AddComponent<MeshFilter>();
		}

		MeshRenderer renderer = target.GetComponent<MeshRenderer>();
		if (renderer == null)
		{
			renderer = target.AddComponent<MeshRenderer>();
		}

		filter.sharedMesh = data.Mesh;
		renderer.sharedMaterials = data.Materials;
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		renderer.receiveShadows = false;
		renderer.lightProbeUsage = LightProbeUsage.Off;
		renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

		return data;
	}

	private static Material CreateOpaqueColorMaterial(string materialName, Color color, string resourcePath)
	{
		Shader shader = Shader.Find("Standard");
		if (shader == null)
		{
			shader = Shader.Find("Legacy Shaders/Diffuse");
		}

		if (shader == null)
		{
			shader = Shader.Find("Mobile/Diffuse");
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
		color.a = 1f;
		material.color = color;
		material.renderQueue = -1;

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

		if (material.HasProperty("_Cull"))
		{
			material.SetInt("_Cull", (int)CullMode.Back);
		}

		if (material.HasProperty("_Glossiness"))
		{
			material.SetFloat("_Glossiness", 0.04f);
		}

		if (material.HasProperty("_Metallic"))
		{
			material.SetFloat("_Metallic", 0f);
		}

		if (material.HasProperty("_EmissionColor"))
		{
			Color emissionColor = color * GetEmissionStrength(resourcePath);
			material.EnableKeyword("_EMISSION");
			material.SetColor("_EmissionColor", emissionColor);
			material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
		}

		material.DisableKeyword("_ALPHATEST_ON");
		material.DisableKeyword("_ALPHABLEND_ON");
		material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
		return material;
	}

	private static Color StylizePaletteColor(string resourcePath, Color rawColor)
	{
		rawColor.a = 1f;

		if (resourcePath.IndexOf("/t-rex/", System.StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return RemapPaletteColor(rawColor, new Color(0.40f, 0.76f, 0.30f, 1f), 0.18f, 1.02f, 0.10f, 1.08f, 0.06f, 0.22f);
		}

		if (resourcePath.IndexOf("/cactus/fcactus", System.StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return RemapPaletteColor(rawColor, new Color(0.47f, 0.68f, 0.35f, 1f), 0.10f, 0.96f, 0.08f, 1.08f, 0.08f, 0.20f);
		}

		if (resourcePath.IndexOf("/cactus/", System.StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return RemapPaletteColor(rawColor, new Color(0.30f, 0.62f, 0.24f, 1f), 0.18f, 1.00f, 0.12f, 1.08f, 0.06f, 0.18f);
		}

		if (resourcePath.IndexOf("/ground", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
		    resourcePath.IndexOf("/earth", System.StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return RemapPaletteColor(rawColor, new Color(0.89f, 0.75f, 0.50f, 1f), 0.06f, 0.96f, 0.08f, 1.04f, 0.06f, 0.24f);
		}

		if (resourcePath.IndexOf("/misc/rocks", System.StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return RemapPaletteColor(rawColor, new Color(0.77f, 0.64f, 0.47f, 1f), 0.04f, 0.88f, 0.04f, 1.04f, 0.05f, 0.20f);
		}

		if (resourcePath.IndexOf("dead_tree", System.StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return RemapPaletteColor(rawColor, new Color(0.61f, 0.44f, 0.28f, 1f), 0.08f, 0.96f, 0.08f, 1.04f, 0.05f, 0.18f);
		}

		if (resourcePath.IndexOf("desert_skull", System.StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return RemapPaletteColor(rawColor, new Color(0.88f, 0.81f, 0.66f, 1f), 0.02f, 0.82f, 0.04f, 1.08f, 0.08f, 0.28f);
		}

		if (resourcePath.IndexOf("scorpion", System.StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return RemapPaletteColor(rawColor, new Color(0.64f, 0.42f, 0.24f, 1f), 0.08f, 1.00f, 0.10f, 1.05f, 0.05f, 0.16f);
		}

		return RemapPaletteColor(rawColor, rawColor, 0f, 1f, 0.04f, 1.03f, 0.04f, 0.16f);
	}

	private static float GetEmissionStrength(string resourcePath)
	{
		if (resourcePath.IndexOf("/t-rex/", System.StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return 0.08f;
		}

		if (resourcePath.IndexOf("/cactus/fcactus", System.StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return 0.02f;
		}

		if (resourcePath.IndexOf("/cactus/", System.StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return 0.04f;
		}

		if (resourcePath.IndexOf("/misc/", System.StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return 0.01f;
		}

		return 0f;
	}

	private static Color RemapPaletteColor(
		Color source,
		Color target,
		float hueBlend,
		float saturationScale,
		float saturationFloor,
		float valueScale,
		float valueAdd,
		float valueFloor)
	{
		float h;
		float s;
		float v;
		Color.RGBToHSV(source, out h, out s, out v);

		float targetHue;
		float targetSaturation;
		float targetValue;
		Color.RGBToHSV(target, out targetHue, out targetSaturation, out targetValue);

		bool brightAccent = s < 0.16f && v > 0.68f;
		if (brightAccent)
		{
			hueBlend *= 0.22f;
			saturationScale *= 0.5f;
			saturationFloor *= 0.35f;
			valueAdd += 0.08f;
			valueFloor = Mathf.Max(valueFloor, 0.52f);
		}

		if (hueBlend > 0f)
		{
			h = Mathf.Repeat(Mathf.LerpAngle(h * 360f, targetHue * 360f, hueBlend) / 360f, 1f);
		}

		s = Mathf.Clamp01(Mathf.Max(s * saturationScale, saturationFloor));
		v = Mathf.Clamp01(Mathf.Max(v * valueScale + valueAdd, valueFloor));

		Color result = Color.HSVToRGB(h, s, v);
		result.a = 1f;
		return result;
	}

	public static VoxMeshData Load(string resourcePath, float voxelSize)
	{
		string key = resourcePath + "@" + voxelSize.ToString("0.0000");
		VoxMeshData cached;
		if (Cache.TryGetValue(key, out cached))
		{
			return cached;
		}

		TextAsset asset = Resources.Load<TextAsset>(resourcePath);
		if (asset == null)
		{
			Debug.LogWarning("Missing VOX resource: " + resourcePath);
			return null;
		}

		VoxModel model = Parse(asset.bytes);
		if (model == null)
		{
			Debug.LogWarning("Unable to parse VOX resource: " + resourcePath);
			return null;
		}

		VoxMeshData data = BuildMesh(model, voxelSize, resourcePath);
		Cache[key] = data;
		return data;
	}

	private static VoxModel Parse(byte[] bytes)
	{
		if (bytes == null || bytes.Length < 8 || ReadString(bytes, 0, 4) != "VOX ")
		{
			return null;
		}

		VoxModel model = new VoxModel();
		int index = 8;

		while (index + 12 <= bytes.Length)
		{
			string chunkId = ReadString(bytes, index, 4);
			int contentSize = ReadInt(bytes, index + 4);
			index += 12;

			if (index + contentSize > bytes.Length)
			{
				break;
			}

			if (chunkId == "SIZE" && contentSize >= 12)
			{
				model.SizeX = ReadInt(bytes, index);
				model.SizeY = ReadInt(bytes, index + 4);
				model.SizeZ = ReadInt(bytes, index + 8);
			}
			else if (chunkId == "XYZI" && contentSize >= 4)
			{
				int count = ReadInt(bytes, index);
				int cursor = index + 4;
				model.Voxels.Clear();

				for (int i = 0; i < count && cursor + 3 < index + contentSize; i++)
				{
					Voxel voxel = new Voxel();
					voxel.X = bytes[cursor];
					voxel.Y = bytes[cursor + 1];
					voxel.Z = bytes[cursor + 2];
					voxel.Color = bytes[cursor + 3];
					model.Voxels.Add(voxel);
					cursor += 4;
				}
			}
			else if (chunkId == "RGBA" && contentSize >= 1024)
			{
				for (int i = 0; i < 256; i++)
				{
					int cursor = index + i * 4;
					model.Palette[i] = new Color32(bytes[cursor], bytes[cursor + 1], bytes[cursor + 2], bytes[cursor + 3]);
				}

				ShiftPaletteForDirectColorIndex(model.Palette);
			}

			index += contentSize;
		}

		return model.Voxels.Count == 0 ? null : model;
	}

	private static VoxMeshData BuildMesh(VoxModel model, float voxelSize, string meshName)
	{
		Dictionary<int, bool> occupied = new Dictionary<int, bool>(model.Voxels.Count);
		for (int i = 0; i < model.Voxels.Count; i++)
		{
			Voxel voxel = model.Voxels[i];
			occupied[Pack(voxel.X, voxel.Y, voxel.Z)] = true;
		}

		List<Vector3> vertices = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		Dictionary<int, int> colorToSubmesh = new Dictionary<int, int>();
		List<List<int>> triangles = new List<List<int>>();
		List<Material> materials = new List<Material>();

		for (int i = 0; i < model.Voxels.Count; i++)
		{
			Voxel voxel = model.Voxels[i];
			int submesh = GetSubmesh(voxel.Color, model.Palette, meshName, colorToSubmesh, triangles, materials);
			AddVoxelFaces(voxel, model, voxelSize, occupied, submesh, vertices, normals, triangles);
		}

		Mesh mesh = new Mesh();
		mesh.name = meshName;
		mesh.SetVertices(vertices);
		mesh.SetNormals(normals);
		mesh.subMeshCount = triangles.Count;
		for (int i = 0; i < triangles.Count; i++)
		{
			mesh.SetTriangles(triangles[i], i);
		}

		mesh.RecalculateBounds();

		VoxMeshData data = new VoxMeshData();
		data.Mesh = mesh;
		data.Materials = materials.ToArray();
		data.Size = new Vector3(model.SizeX * voxelSize, model.SizeZ * voxelSize, model.SizeY * voxelSize);
		return data;
	}

	private static void AddVoxelFaces(
		Voxel voxel,
		VoxModel model,
		float voxelSize,
		Dictionary<int, bool> occupied,
		int submesh,
		List<Vector3> vertices,
		List<Vector3> normals,
		List<List<int>> triangles)
	{
		float x0 = (voxel.X - model.SizeX * 0.5f) * voxelSize;
		float x1 = x0 + voxelSize;
		float y0 = voxel.Z * voxelSize;
		float y1 = y0 + voxelSize;
		float z0 = (voxel.Y - model.SizeY * 0.5f) * voxelSize;
		float z1 = z0 + voxelSize;

		int x = voxel.X;
		int y = voxel.Y;
		int z = voxel.Z;

		if (!occupied.ContainsKey(Pack(x + 1, y, z)))
		{
			AddQuad(vertices, normals, triangles[submesh], Vector3.right,
				new Vector3(x1, y0, z1), new Vector3(x1, y1, z1), new Vector3(x1, y1, z0), new Vector3(x1, y0, z0));
		}

		if (!occupied.ContainsKey(Pack(x - 1, y, z)))
		{
			AddQuad(vertices, normals, triangles[submesh], Vector3.left,
				new Vector3(x0, y0, z0), new Vector3(x0, y1, z0), new Vector3(x0, y1, z1), new Vector3(x0, y0, z1));
		}

		if (!occupied.ContainsKey(Pack(x, y + 1, z)))
		{
			AddQuad(vertices, normals, triangles[submesh], Vector3.forward,
				new Vector3(x0, y0, z1), new Vector3(x0, y1, z1), new Vector3(x1, y1, z1), new Vector3(x1, y0, z1));
		}

		if (!occupied.ContainsKey(Pack(x, y - 1, z)))
		{
			AddQuad(vertices, normals, triangles[submesh], Vector3.back,
				new Vector3(x1, y0, z0), new Vector3(x1, y1, z0), new Vector3(x0, y1, z0), new Vector3(x0, y0, z0));
		}

		if (!occupied.ContainsKey(Pack(x, y, z + 1)))
		{
			AddQuad(vertices, normals, triangles[submesh], Vector3.up,
				new Vector3(x0, y1, z1), new Vector3(x0, y1, z0), new Vector3(x1, y1, z0), new Vector3(x1, y1, z1));
		}

		if (!occupied.ContainsKey(Pack(x, y, z - 1)))
		{
			AddQuad(vertices, normals, triangles[submesh], Vector3.down,
				new Vector3(x0, y0, z0), new Vector3(x0, y0, z1), new Vector3(x1, y0, z1), new Vector3(x1, y0, z0));
		}
	}

	private static int GetSubmesh(
		byte colorIndex,
		Color32[] palette,
		string resourcePath,
		Dictionary<int, int> colorToSubmesh,
		List<List<int>> triangles,
		List<Material> materials)
	{
		int paletteIndex = Mathf.Clamp(colorIndex, 0, 255);
		int submesh;
		if (colorToSubmesh.TryGetValue(paletteIndex, out submesh))
		{
			return submesh;
		}

		submesh = triangles.Count;
		colorToSubmesh[paletteIndex] = submesh;
		triangles.Add(new List<int>());
		Color materialColor = StylizePaletteColor(resourcePath, palette[paletteIndex]);
		Material material = CreateOpaqueColorMaterial("VoxColor_" + paletteIndex, materialColor, resourcePath);
		materials.Add(material);
		return submesh;
	}

	private static void AddQuad(
		List<Vector3> vertices,
		List<Vector3> normals,
		List<int> triangles,
		Vector3 normal,
		Vector3 a,
		Vector3 b,
		Vector3 c,
		Vector3 d)
	{
		int start = vertices.Count;
		vertices.Add(a);
		vertices.Add(b);
		vertices.Add(c);
		vertices.Add(d);

		normals.Add(normal);
		normals.Add(normal);
		normals.Add(normal);
		normals.Add(normal);

		triangles.Add(start);
		triangles.Add(start + 1);
		triangles.Add(start + 2);
		triangles.Add(start);
		triangles.Add(start + 2);
		triangles.Add(start + 3);
	}

	private static int Pack(int x, int y, int z)
	{
		return (x & 0x3ff) | ((y & 0x3ff) << 10) | ((z & 0x3ff) << 20);
	}

	private static int ReadInt(byte[] bytes, int index)
	{
		return bytes[index] | (bytes[index + 1] << 8) | (bytes[index + 2] << 16) | (bytes[index + 3] << 24);
	}

	private static string ReadString(byte[] bytes, int index, int length)
	{
		return Encoding.ASCII.GetString(bytes, index, length);
	}

	private static Color32[] CreateDefaultPalette()
	{
		Color32[] palette = new Color32[256];
		int index = 0;
		byte[] levels = {255, 204, 153, 102, 51, 0};

		palette[index++] = new Color32(255, 255, 255, 255);

		for (int r = 0; r < levels.Length; r++)
		{
			for (int g = 0; g < levels.Length; g++)
			{
				for (int b = 0; b < levels.Length; b++)
				{
					if (levels[r] == 0 && levels[g] == 0 && levels[b] == 0)
					{
						continue;
					}

					palette[index++] = new Color32(levels[r], levels[g], levels[b], 255);
				}
			}
		}

		byte[] ramps = {238, 221, 187, 170, 136, 119, 85, 68, 34, 17};
		for (int i = 0; i < ramps.Length; i++)
		{
			palette[index++] = new Color32(ramps[i], 0, 0, 255);
		}

		for (int i = 0; i < ramps.Length; i++)
		{
			palette[index++] = new Color32(0, ramps[i], 0, 255);
		}

		for (int i = 0; i < ramps.Length; i++)
		{
			palette[index++] = new Color32(0, 0, ramps[i], 255);
		}

		for (int i = 0; i < ramps.Length && index < palette.Length; i++)
		{
			palette[index++] = new Color32(ramps[i], ramps[i], ramps[i], 255);
		}

		return palette;
	}

	private static void ShiftPaletteForDirectColorIndex(Color32[] palette)
	{
		if (palette == null || palette.Length == 0)
		{
			return;
		}

		for (int i = palette.Length - 1; i > 0; i--)
		{
			palette[i] = palette[i - 1];
		}
	}
}
