using UnityEditor;
using UnityEngine;

// Editor utilities to set up Terrain layers and paint textures using optional masks.
// Menu:
// - Tools/Terrain/Assign Default TerrainLayers
// - Tools/Terrain/Apply Splat From Masks
// Place mask textures under Resources/Terrain/Textures and ensure they are Read/Write enabled.
public static class TerrainSetupEditor
{
	[MenuItem("Tools/Terrain/Assign Default TerrainLayers")] 
	public static void AssignDefaultTerrainLayers()
	{
		Terrain terrain = Object.FindObjectOfType<Terrain>();
		if (terrain == null)
		{
			Debug.LogError("No Terrain found in the scene. Create or select a Terrain first.");
			return;
		}

		// Load all TerrainLayer assets from Resources/Terrain/Layers
		TerrainLayer[] layers = Resources.LoadAll<TerrainLayer>("Terrain/Layers");
		if (layers == null || layers.Length == 0)
		{
			Debug.LogError("No TerrainLayer assets found under Resources/Terrain/Layers.");
			return;
		}

		Undo.RecordObject(terrain.terrainData, "Assign Terrain Layers");
		terrain.terrainData.terrainLayers = layers;
		EditorUtility.SetDirty(terrain.terrainData);
		Debug.Log($"Assigned {layers.Length} TerrainLayers to '{terrain.name}'.");
	}

	[MenuItem("Tools/Terrain/Apply Splat From Masks")] 
	public static void ApplySplatFromMasks()
	{
		Terrain terrain = Object.FindObjectOfType<Terrain>();
		if (terrain == null)
		{
			Debug.LogError("No Terrain found in the scene. Create or select a Terrain first.");
			return;
		}

		TerrainData td = terrain.terrainData;
		int w = td.alphamapWidth;
		int h = td.alphamapHeight;
		int numLayers = td.alphamapLayers;
		if (numLayers == 0)
		{
			Debug.LogError("Terrain has no layers. Run Tools/Terrain/Assign Default TerrainLayers first.");
			return;
		}

		// Try to load masks from Resources. Missing masks are treated as zero weight.
		Texture2D grassMask = Resources.Load<Texture2D>("Terrain/Textures/grass-heightmap");
		Texture2D dirtMask = Resources.Load<Texture2D>("Terrain/Textures/dirt");
		Texture2D sandMask = Resources.Load<Texture2D>("Terrain/Textures/sand");
		Texture2D soilMask = Resources.Load<Texture2D>("Terrain/Textures/soil");

		bool anyMask = grassMask || dirtMask || sandMask || soilMask;
		if (!anyMask)
		{
			Debug.LogWarning("No mask textures found in Resources/Terrain/Textures. Painting will default to layer 0.");
		}

		if (!IsReadable(grassMask) || !IsReadable(dirtMask) || !IsReadable(sandMask) || !IsReadable(soilMask))
		{
			Debug.LogWarning("One or more masks are not readable. Enable Read/Write on their import settings.");
		}

		float[,,] splat = new float[h, w, numLayers];

		for (int y = 0; y < h; y++)
		{
			for (int x = 0; x < w; x++)
			{
				float u = (float)x / (w - 1);
				float v = (float)y / (h - 1);

				// Sample masks as grayscale in [0..1]
				float g = SampleGray(grassMask, u, v);
				float d = SampleGray(dirtMask, u, v);
				float s = SampleGray(sandMask, u, v);
				float o = SampleGray(soilMask, u, v);

				// Collect up to first 4 layers. If there are more layers, extra ones will be zeroed.
				float[] weights = new float[numLayers];
				if (numLayers > 0) weights[0] = g;
				if (numLayers > 1) weights[1] = d;
				if (numLayers > 2) weights[2] = s;
				if (numLayers > 3) weights[3] = o;

				float sum = 0f;
				for (int i = 0; i < numLayers; i++) sum += weights[i];
				if (sum <= 0.0001f)
				{
					// Default all weight to first layer when no mask signal
					weights[0] = 1f;
					sum = 1f;
				}

				for (int i = 0; i < numLayers; i++)
					splat[y, x, i] = weights[i] / sum;
			}
		}

		Undo.RecordObject(td, "Apply Splat From Masks");
		td.SetAlphamaps(0, 0, splat);
		EditorUtility.SetDirty(td);
		Debug.Log($"Applied splatmaps ({w}x{h}) using available masks to '{terrain.name}'.");
	}

	private static bool IsReadable(Texture2D tex)
	{
		if (tex == null) return true; // treat missing as readable to keep flow
		try
		{
			tex.GetPixel(0, 0);
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static float SampleGray(Texture2D tex, float u, float v)
	{
		if (tex == null) return 0f;
		Color c = tex.GetPixelBilinear(u, v);
		return Mathf.Clamp01(c.grayscale);
	}
}