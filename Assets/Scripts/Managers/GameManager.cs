using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;

	[Header("Canvas")]
	public Canvas canvas;
	public float canvasScaleFactor;

	[Header("Game Parameters")]
	public GameGlobalParameters gameGlobalParameters;
	public GamePlayersParameters gamePlayersParameters;
	public GameSoundParameters gameSoundParameters;
	public GameInputParameters gameInputParameters;
	public GameObject fov;

	[Header("Minimap")]
	public Transform minimapAnchor;
	public Camera minimapCamera;
	public Minimap minimapScript;
	public BoxCollider mapWrapperCollider;
	public int terrainSize;
	private const float _TERRAIN_MID_HEIGHT = 30f;

	[HideInInspector]
	public bool gameIsPaused;

	[HideInInspector]
	public float producingRate = 3f; // in seconds

	[HideInInspector]
	public bool waitingForInput;
	[HideInInspector]
	public string pressedKey;

	private void Awake()
	{
		// Cache current UI scale for screen-to-world conversions
		canvasScaleFactor = canvas.scaleFactor;

		// Load saved player/settings data and enable optional systems
		DataHandler.LoadGameData();
		GetComponent<DayAndNightCycler>().enabled = gameGlobalParameters.enableDayAndNightCycle;

		// Initialize global resource slots and navigation
		Globals.InitializeGameResources(gamePlayersParameters.players.Length);

		// Expect a GameObject named "Terrain" with a NavMeshSurface
		Globals.NAV_MESH_SURFACE = GameObject.Find("Terrain").GetComponent<NavMeshSurface>();
		Globals.UpdateNavMeshSurface();

		// enable/disable FOV depending on game parameters
		fov.SetActive(gameGlobalParameters.enableFOV);

		// Minimap camera and wrapper collider sized to current terrain bounds
		_SetupMinimap();

		gameIsPaused = false;
	}

	public void Start()
	{
		instance = this;
	}

	// Positions the minimap, sets orthographic size, and synchronizes the wrapper collider
	private void _SetupMinimap()
	{
		Bounds b = GameObject.Find("Terrain").GetComponent<Terrain>().terrainData.bounds;

		terrainSize = (int) b.size.x;
		float p = terrainSize / 2;

		// Center the minimap camera over the terrain square
		minimapAnchor.position = new Vector3(p, 0, p);
		minimapCamera.orthographicSize = p;
		// Wrapper collider used for clamping and FOV computations
		mapWrapperCollider.center = new Vector3(0, _TERRAIN_MID_HEIGHT, 0);
		mapWrapperCollider.size = new Vector3(b.size.x, 1f, b.size.z);
		minimapScript.terrainSize = Vector2.one * terrainSize;
	}

	private void Update()
	{
		if (Input.anyKeyDown)
		{
			if (waitingForInput)
			{
				// Capture the physical input description when rebinding keys
				if (Input.GetMouseButtonDown(0))
					pressedKey = "mouse 0";
				else if (Input.GetMouseButtonDown(1))
					pressedKey = "mouse 1";
				else if (Input.GetMouseButtonDown(2))
					pressedKey = "mouse 2";
				else
					pressedKey = Input.inputString;
				waitingForInput = false;
			}
			else
				gameInputParameters.CheckForInput();
		}
	}

	private void OnEnable()
	{
		EventManager.AddListener("PausedGame", _OnPausedGame);
		EventManager.AddListener("ResumedGame", _OnResumedGame);

		EventManager.AddListener("UpdateGameParameter:enableDayAndNightCycle", _OnUpdateDayAndNightCycle);
		EventManager.AddListener("UpdateGameParameter:enableFOV", _OnUpdateFOV);
	}

	private void OnDisable()
	{
		EventManager.RemoveListener("PausedGame", _OnPausedGame);
		EventManager.RemoveListener("ResumedGame", _OnResumedGame);

		EventManager.RemoveListener("UpdateGameParameter:enableDayAndNightCycle", _OnUpdateDayAndNightCycle);
		EventManager.RemoveListener("UpdateGameParameter:enableFOV", _OnUpdateFOV);
	}

	private void _OnPausedGame()
	{
		gameIsPaused = true;
		Time.timeScale = 0;
	}

	private void _OnResumedGame()
	{
		gameIsPaused = false;
		Time.timeScale = 1;
	}

	/* game parameters update */
	private void _OnUpdateDayAndNightCycle(object data)
	{
		bool dayAndNightIsOn = (bool)data;
		GetComponent<DayAndNightCycler>().enabled = dayAndNightIsOn;
	}
	private void _OnUpdateFOV(object data)
	{
		bool fovIsOn = (bool)data;
		fov.SetActive(fovIsOn);
	}

	private void OnApplicationQuit()
	{
#if !UNITY_EDITOR
		DataHandler.SaveGameData();
#endif
	}
}
