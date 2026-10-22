using System.Collections.Generic;
using Baltarus.Blueprints;
using Common.Platform;
using DebugTools;
using Silica;
using Silica.AI;
using Silica.UI;
using Silica.UI.Loading;
using Silica.UI.Menus;
using UI.Windows.FlowGraphEditor;
using UnitRecorder;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[DefaultExecutionOrder(-1000)]
public class GameManager : MonoBehaviour
{
	public const float GC_COLLECT_CHECK_DELAY = 5f;

	public const long GC_COLLECT_MAX_SIZE = 15032385536L;

	public const string EDITOR_FALLBACK_DATABASE_PATH = "Assets/Baltarus/Configs/Silica_Database.asset";

	public AudioMixerGroup m_AudioMixer_Master;

	public AudioMixerGroup m_AudioMixer_Sound;

	public AudioMixerGroup m_AudioMixer_UI;

	public AudioMixerGroup m_AudioMixer_Voice;

	public AudioMixerGroup m_AudioMixer_VoiceChat;

	public AudioMixerGroup m_AudioMixer_Music;

	public AudioMixerGroup m_AudioMixer_RadioVoice;

	[Tooltip("Blur post-process volume")]
	public Volume BlurVolume;

	[Tooltip("Database to use for this game. LEAVE EMPTY except in the bootstrap scenes (Intro / MainMenu)! A reference here pulls the whole asset blob into the scene's build dependencies and costs ~500 s of build time for that scene. Everything reads the static GameDatabase.Database, which is set by the bootstrap scene and survives scene loads.")]
	public GameDatabase Database;

	[Tooltip("A completely set up loading screen prefab. Used for instantiating loading screen when the game did not start from the intro. Mainly for editor purposes")]
	public LoadingScreenController LoadingScreenControllerPrefab;

	[Tooltip("Country database to use for this game")]
	public CountryDatabase CountryDatabase;

	[Tooltip("How often physics are updated (fixed tick, in Hz)")]
	[Range(1f, 250f)]
	public int PhysicsTickHz = 25;

	[Tooltip("Gravitational acceleration in Y axis (in m/s^2")]
	[Range(-100f, 100f)]
	public float GravityY = -9.811f;

	[Tooltip("Multiplier of gravity forces to apply when falling on top of standard gravity to some units (such as characters and creatures), may also multiply jump force (%)")]
	[Range(-1f, 10f)]
	public float UnitFallGravityMult = 1f;

	[HideInInspector]
	public List<CollisionAvoidanceGraphMask> CollisionAvoidanceGraphMasks = new List<CollisionAvoidanceGraphMask>();

	[Tooltip("Whether the current level is the main menu")]
	public bool IsMainMenu;

	[Tooltip("The asset to use for input")]
	[SerializeField]
	private GameInputAsset InputAsset;

	[Tooltip("Cheat key that runs the playing unit path to its end. Held here rather than on the replayer, which is added to units at runtime and so has nothing to serialize a reference into.")]
	[SerializeField]
	private InputActionReference SkipUnitPathAction;

	private float m_NetTickMult = 1f;

	public static bool IsPauseMenuActive;

	public bool DebugSpawn_ShowList;

	public int DebugSpawn_Offset;

	public Team DebugSpawn_Team;

	private DepthOfField m_DepthOfField;

	private float m_BlurNearMax;

	private float m_BlurFarMax;

	private float m_Blur01;

	private float PhysicsTime;

	private bool m_ShouldCursorBeVisible;

	[Tooltip("Prefab to use for the strategy network manager")]
	public StrategyNetworkManager StrategyNetworkManagerPrefab;

	public static GameManager Instance { get; private set; }

	public float Net_TickMult
	{
		get
		{
			return m_NetTickMult;
		}
		set
		{
			m_NetTickMult = Mathf.Clamp(value, 0.1f, 30f);
			if (Game.GetIsServer())
			{
				NetworkLayer.SendTickMult(Net_TickMult);
			}
		}
	}

	public static bool IsAnyMenuOpened
	{
		get
		{
			if (!IsPauseMenuActive)
			{
				if ((bool)MenuManager.Instance)
				{
					return MenuManager.Instance.IsAnyMenuOpened();
				}
				return false;
			}
			return true;
		}
	}

	public float Blur01
	{
		get
		{
			return m_Blur01;
		}
		set
		{
			m_Blur01 = Mathf.Clamp01(value);
			if ((bool)BlurVolume)
			{
				if (m_Blur01 <= 0f)
				{
					BlurVolume.enabled = false;
					return;
				}
				BlurVolume.enabled = true;
				float num = 1f - Mathf.Pow(1f - m_Blur01, 3f);
				m_DepthOfField.nearMaxBlur = m_BlurNearMax * num;
				m_DepthOfField.farMaxBlur = m_BlurFarMax * num;
			}
		}
	}

	public void OnEscape()
	{
		if (!GlobeMissionEditor.OnEscape() && !GlobeEditor.OnEscape() && !GameVideo.OnEscape() && !ConfirmationPopup.OnEscape() && !BlueprintSpawner.OnEscape() && !BlueprintEditor.OnEscape() && !FlowGraph.OnEscape() && !StrategyMode.OnEscape() && !Credits.OnEscape() && !IntroDialogueUI.OnEscape() && (!(GameMode.CurrentGameMode != null) || !GameMode.CurrentGameMode.OnEscape()) && (bool)MenuManager.Instance && !MenuManager.Instance.OnEscape())
		{
			if (!MenuManager.Instance.IsMenuOpened<InGameMenu>())
			{
				MenuManager.Instance.OpenMenu(out InGameMenu _);
			}
			else
			{
				MenuManager.Instance.CloseMenu<InGameMenu>();
			}
		}
	}

	private void ToggleUnit()
	{
		Player currentPlayer = Player.CurrentPlayer;
		if ((bool)GameMode.CurrentGameMode && !GameMode.CurrentGameMode.GetCanPlayerChangeUnits(currentPlayer))
		{
			return;
		}
		if ((bool)Unit.CurrentUnit)
		{
			if (currentPlayer.GetCanSwitchToUnit(null))
			{
				if (Game.GetIsMultiplayer())
				{
					NetworkLayer.SendPlayerSelectUnit(currentPlayer, null);
				}
				if (!Game.GetIsClient())
				{
					currentPlayer.ControlledUnit = null;
				}
			}
			return;
		}
		Unit nearestUnit = Unit.GetNearestUnit(Game.CurrentCamera.transform.position, mustBeUncontrolled: true);
		if ((bool)nearestUnit)
		{
			if (Game.GetIsMultiplayer())
			{
				NetworkLayer.SendPlayerSelectUnit(currentPlayer, nearestUnit);
			}
			if (!Game.GetIsClient())
			{
				currentPlayer.ControlledUnit = nearestUnit;
			}
		}
	}

	private void TeleportUnit()
	{
		if ((bool)Unit.LastControlledUnit && !Game.GetIsClient())
		{
			Unit.LastControlledUnit.QueueTeleport(Game.CurrentCamera.transform.position, Quaternion.Euler(0f, Game.CurrentCamera.transform.rotation.eulerAngles.y, 0f));
		}
	}

	private void CycleUnit(bool onlyAlive = true)
	{
		Player currentPlayer = Player.CurrentPlayer;
		if (((bool)GameMode.CurrentGameMode && !GameMode.CurrentGameMode.GetCanPlayerChangeUnits(currentPlayer)) || ((bool)Unit.CurrentUnit && !currentPlayer.GetCanSwitchToUnit(null)))
		{
			return;
		}
		Unit nextUnit = Unit.GetNextUnit(Unit.CurrentUnit, mustBeUncontrolled: true, onlyAlive);
		if ((bool)nextUnit && currentPlayer.GetCanSwitchToUnit(nextUnit))
		{
			if (Game.GetIsMultiplayer())
			{
				NetworkLayer.SendPlayerSelectUnit(currentPlayer, nextUnit);
			}
			if (!Game.GetIsClient())
			{
				currentPlayer.ControlledUnit = nextUnit;
			}
		}
	}

	private void SimulatePhysics()
	{
		if (Physics.simulationMode != SimulationMode.Script)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		if (deltaTime <= 0f)
		{
			return;
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		PhysicsTime += deltaTime;
		while (PhysicsTime >= fixedDeltaTime)
		{
			if ((bool)OrderedUpdateManager.Instance)
			{
				OrderedUpdateManager.Instance.CallFixedUpdate(fixedDeltaTime);
			}
			PhysicsTime -= fixedDeltaTime;
			Physics.Simulate(fixedDeltaTime);
			if ((bool)OrderedUpdateManager.Instance)
			{
				OrderedUpdateManager.Instance.CallFixedLateUpdate(fixedDeltaTime);
			}
		}
	}

	private void Update()
	{
		JobHandle.ScheduleBatchedJobs();
		Game.UpdateFPS();
		NetworkLayer.Update();
		if (!Game.IsLoading && IsMainMenu && !NetworkGameServer.Instance)
		{
			if (!NetworkGameServer.CreateServerFromSettings())
			{
				Debug.LogError("Cannot start server from settings!");
			}
			return;
		}
		SimulatePhysics();
		GameInput.ProcessInput();
		if (!GameLevelLoader.IsLoading && GameInput.GetButtonDown("Menu", GameInput.InputMap.Global))
		{
			if (OrderTargetingController.IsTargetingActive)
			{
				OrderTargetingController.Current.Cancel();
			}
			else
			{
				OnEscape();
			}
		}
		if (GameVideo.IsPlaying && (GameInput.GetButtonDown("Cancel", GameInput.InputMap.UI) || GameInput.GetButtonDown("Submit", GameInput.InputMap.UI) || GameInput.GetButtonDown("Click", GameInput.InputMap.UI)))
		{
			GameVideo.OnAnyKey();
		}
		Game.UpdateFading(Mathf.Clamp(Time.unscaledDeltaTime, 0f, 0.04f), forceUpdateSound: false);
		DebugConsole.HandleToggling();
		DebugConsole.CanToggle = true;
		GameInput.UpdateInput();
		if (!Game.CheatsEnabled)
		{
			Debug.developerConsoleVisible = false;
		}
		Game.UpdateCameraFOV();
		NetworkComponent.TickNetworkObjects(Time.unscaledDeltaTime);
		VisionModeObject.UpdateNewlyAddedObjects();
		CinematicCamera.Update();
		BaseGameObject.UpdateDestroyedDisposal();
		Unit.UpdateFallThroughGround();
		Unit.PrepareUpdateConcealmentAndNavmeshCutAll();
		Unit.UpdateRigidBodyInterpolationAll();
		Team.TickTeams();
		Target.TickTargetsWithEvent();
		VehicleParticleFX.TickVehicleParticleFX();
		DisableBasedOnDistance.TickDisableBasedOnDistance();
		UsePoint.UpdateUse();
		Unit.CompleteUpdateConcealmentAll();
		AIManager.TickAI();
		AIManager.UpdateSyncAI();
		AudioAmbientSource.UpdateAmbients();
		CurrentObjective.Update();
		if ((bool)FogOfWar.Instance)
		{
			FogOfWar.Instance.OnUpdate();
		}
		DamageManager.UpdateSynchHealth(Time.unscaledDeltaTime);
		GameAI.UpdateNavigation();
		CursorWaiting.UpdateShow();
		LightGrid.Update();
		if (Debug.isDebugBuild && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.U))
		{
			CameraOperations.CopyCameraToClipboard();
			Debug.Log("Copied current camera view to clipboard");
		}
		bool buttonDown = GameInput.GetButtonDown("Toggle Strategy Mode", GameInput.InputMap.Cheats);
		if (((!UIManager.TextInputActive && Game.CheatsEnabled) & buttonDown) && (!GameMode.CurrentGameMode || GameMode.CurrentGameMode.GetCanFreelyOpenStrategyMode()) && (!StrategyMode.Instance || !StrategyMode.Instance.DirectControlUnit))
		{
			StrategyMode.ToggleStrategyMode(!StrategyMode.InStrategyMode);
		}
		IsPauseMenuActive = (bool)MenuManager.Instance && MenuManager.Instance.IsMenuOpened<InGameMenu>();
		if (!UIManager.TextInputActive && !Benchmark.IsRunning && Game.CheatsEnabled)
		{
			if (GameInput.GetButtonDown("Debug Health", GameInput.InputMap.Cheats))
			{
				DamageManager.ShowDebugHealth++;
				if (DamageManager.ShowDebugHealth > 2)
				{
					DamageManager.ShowDebugHealth = 0;
				}
			}
			if (GameInput.GetButtonDown("Debug Damage", GameInput.InputMap.Cheats))
			{
				DamageManager.ShowDebug = !DamageManager.ShowDebug;
			}
			if (GameInput.GetButtonDown("Debug AI", GameInput.InputMap.Cheats))
			{
				AIManager.ShowDebug = !AIManager.ShowDebug;
			}
			if ((bool)Player.CurrentPlayer && !StrategyMode.InStrategyMode && !StrategyMode.InDirectControl)
			{
				if (GameInput.GetButtonDown("Toggle Unit", GameInput.InputMap.Cheats))
				{
					ToggleUnit();
				}
				if (!Unit.CurrentUnit && GameInput.GetButtonDown("Teleport Unit", GameInput.InputMap.Cheats))
				{
					TeleportUnit();
				}
				if (GameInput.GetButtonDown("Select Next Unit", GameInput.InputMap.Cheats))
				{
					CycleUnit(!Input.GetKey(KeyCode.LeftShift));
				}
			}
			if (!Game.GetIsClient())
			{
				if (!StrategyMode.InStrategyMode)
				{
					if (GameInput.GetButtonDown("Delete Structures", GameInput.InputMap.Cheats))
					{
						if (!Unit.CurrentUnit)
						{
							Debug.Log("Deleting ALL structures...");
							Structure.DestroyAllStructures();
							ConstructionSite.ClearAllConstructionSites();
						}
					}
					else if (GameInput.GetButtonDown("Delete Units", GameInput.InputMap.Cheats))
					{
						if ((bool)Unit.CurrentUnit)
						{
							Debug.Log("Deleting current unit...");
							Object.Destroy(Unit.CurrentUnit.gameObject);
						}
						else
						{
							Debug.Log("Deleting ALL units...");
							Unit.DestroyAllUnits();
						}
					}
				}
				if (!Unit.CurrentUnit && !StrategyMode.InStrategyMode)
				{
					if (GameInput.GetButtonDown("Spawn Menu", GameInput.InputMap.Cheats))
					{
						DebugSpawn_ShowList = !DebugSpawn_ShowList;
					}
					if (DebugSpawn_ShowList)
					{
						if (GameInput.GetButtonDown("Spawn Next Unit", GameInput.InputMap.Cheats) && DebugSpawnList.GetPrefabSpawnableMaxIndex() >= DebugSpawn_Offset + 10)
						{
							DebugSpawn_Offset += 10;
						}
						else if (GameInput.GetButtonDown("Spawn Previous Unit", GameInput.InputMap.Cheats) && DebugSpawn_Offset > 0)
						{
							DebugSpawn_Offset -= 10;
						}
						List<Team> list = (GameDatabase.Database ? GameDatabase.Database.AllTeams : null);
						if (list != null && list.Count > 0)
						{
							if (GameInput.GetButtonDown("Team Shift Left", GameInput.InputMap.Cheats))
							{
								if (DebugSpawn_Team == null)
								{
									DebugSpawn_Team = list[list.Count - 1];
								}
								else if (list.IndexOf(DebugSpawn_Team) <= 0)
								{
									DebugSpawn_Team = null;
								}
								else
								{
									DebugSpawn_Team = list[list.IndexOf(DebugSpawn_Team) - 1];
								}
							}
							if (GameInput.GetButtonDown("Team Shift Right", GameInput.InputMap.Cheats))
							{
								if (DebugSpawn_Team == null)
								{
									DebugSpawn_Team = list[0];
								}
								else if (list.IndexOf(DebugSpawn_Team) >= list.Count - 1)
								{
									DebugSpawn_Team = null;
								}
								else
								{
									DebugSpawn_Team = list[list.IndexOf(DebugSpawn_Team) + 1];
								}
							}
						}
						int count = ((!GameInput.GetButton("Spawn Group Modifier", GameInput.InputMap.Cheats)) ? 1 : 50);
						if (GameInput.GetButtonDown("Spawn Unit 1", GameInput.InputMap.Cheats))
						{
							DebugSpawnList.DebugSpawnIndexedPrefab(DebugSpawn_Offset, count, DebugSpawn_Team);
						}
						if (GameInput.GetButtonDown("Spawn Unit 2", GameInput.InputMap.Cheats))
						{
							DebugSpawnList.DebugSpawnIndexedPrefab(DebugSpawn_Offset + 1, count, DebugSpawn_Team);
						}
						if (GameInput.GetButtonDown("Spawn Unit 3", GameInput.InputMap.Cheats))
						{
							DebugSpawnList.DebugSpawnIndexedPrefab(DebugSpawn_Offset + 2, count, DebugSpawn_Team);
						}
						if (GameInput.GetButtonDown("Spawn Unit 4", GameInput.InputMap.Cheats))
						{
							DebugSpawnList.DebugSpawnIndexedPrefab(DebugSpawn_Offset + 3, count, DebugSpawn_Team);
						}
						if (GameInput.GetButtonDown("Spawn Unit 5", GameInput.InputMap.Cheats))
						{
							DebugSpawnList.DebugSpawnIndexedPrefab(DebugSpawn_Offset + 4, count, DebugSpawn_Team);
						}
						if (GameInput.GetButtonDown("Spawn Unit 6", GameInput.InputMap.Cheats))
						{
							DebugSpawnList.DebugSpawnIndexedPrefab(DebugSpawn_Offset + 5, count, DebugSpawn_Team);
						}
						if (GameInput.GetButtonDown("Spawn Unit 7", GameInput.InputMap.Cheats))
						{
							DebugSpawnList.DebugSpawnIndexedPrefab(DebugSpawn_Offset + 6, count, DebugSpawn_Team);
						}
						if (GameInput.GetButtonDown("Spawn Unit 8", GameInput.InputMap.Cheats))
						{
							DebugSpawnList.DebugSpawnIndexedPrefab(DebugSpawn_Offset + 7, count, DebugSpawn_Team);
						}
						if (GameInput.GetButtonDown("Spawn Unit 9", GameInput.InputMap.Cheats))
						{
							DebugSpawnList.DebugSpawnIndexedPrefab(DebugSpawn_Offset + 8, count, DebugSpawn_Team);
						}
						if (GameInput.GetButtonDown("Spawn Unit 10", GameInput.InputMap.Cheats))
						{
							DebugSpawnList.DebugSpawnIndexedPrefab(DebugSpawn_Offset + 9, count, DebugSpawn_Team);
						}
					}
				}
			}
		}
		Game.CurrentCameraRotation = Game.CurrentCamera.transform.rotation;
		Game.CurrentCameraDirection = Game.CurrentCamera.transform.forward;
		Game.CurrentCameraPosition = Game.CurrentCamera.transform.position;
		Game.CurrentGamePosition = (Unit.CurrentUnit ? Unit.CurrentUnit.transform.position : Game.CurrentCameraPosition);
		Game.CurrentListenerPosition = Game.CurrentCameraPosition;
		Game.CurrentCameraFOVV = Game.CurrentCamera.fieldOfView;
		Game.CurrentCameraFOVH = (float)Screen.width / (float)Screen.height * Game.CurrentCameraFOVV;
	}

	private void Awake()
	{
		Instance = this;
		Game.GravityY = GravityY;
		Game.UnitFallGravityMult = UnitFallGravityMult;
		Physics.gravity = Vector3.up * GravityY;
		OrderedUpdateManager.RegisterUpdate(SensorTickPart1, 1000);
		OrderedUpdateManager.RegisterUpdate(SensorTickPart2, -1);
		OrderedUpdateManager.RegisterUpdate(SensorTickPart3, -3000);
		if (InputAsset != null)
		{
			InputAsset.Initialize();
		}
		GameInput.SetPlayerInput(GetComponent<PlayerInput>());
		OrderedUpdateManager.RegisterUpdate(OverrideManager.Update, int.MinValue);
	}

	private void SensorTickPart1(float dt)
	{
		Sensor.EnsureSensorJobsCompleted();
		TargetGrid.Update();
		Sensor.TickSensors();
	}

	private void SensorTickPart2(float dt)
	{
		Sensor.TickSensorsPart2();
	}

	private void SensorTickPart3(float dt)
	{
		Sensor.TickSensorsPart3();
	}

	private static void EnsureDatabaseFromBootstrap()
	{
		if (!(GameDatabase.Database != null))
		{
			Debug.LogError("GameManager: GameDatabase.Database is null outside of a bootstrap scene. The game must be started from the Intro/MainMenu scene.");
		}
	}

	private void Start()
	{
		Debug.Log("Initializing Game Manager");
		UnitPathReplayer.SubscribeKeybinds(SkipUnitPathAction);
		if (Database != null)
		{
			GameDatabase.Database = Database;
		}
		else
		{
			EnsureDatabaseFromBootstrap();
		}
		CountryDatabase.Database = CountryDatabase;
		Game.GravityY = GravityY;
		Game.UnitFallGravityMult = UnitFallGravityMult;
		Physics.gravity = Vector3.up * GravityY;
		Time.fixedDeltaTime = 1f / (float)PhysicsTickHz;
		if ((bool)BlurVolume)
		{
			BlurVolume.profile.TryGet<DepthOfField>(out m_DepthOfField);
			if ((bool)m_DepthOfField)
			{
				m_BlurNearMax = m_DepthOfField.nearMaxBlur;
				m_BlurFarMax = m_DepthOfField.farMaxBlur;
			}
		}
		if (GameLevelLoader.IsInLevelScene())
		{
			GameSettings.Load();
			GameSettings.Apply();
		}
		Game.InitGameLevel();
		if (Game.GetIsSingleplayer())
		{
			if (!Player.CurrentPlayer)
			{
				if (PlatformManager.IsPlatformInitialized())
				{
					Player.CurrentPlayer = Player.AddPlayer(NetworkID.CurrentUserID, PlatformManager.GetPersonaName());
				}
				else
				{
					Player.CurrentPlayer = Player.AddPlayer(NetworkID.Nil, "Player");
				}
			}
			GameEvents.OnPlayerJoined?.Invoke(Player.CurrentPlayer);
		}
		if (LoadingScreenController.Instance == null && LoadingScreenControllerPrefab != null)
		{
			Object.Instantiate(LoadingScreenControllerPrefab);
		}
	}

	private void OnDestroy()
	{
		UnitPathReplayer.UnsubscribeKeybinds();
		Game.ClearAll(deleting: true);
		if ((bool)Camera.main && (bool)Camera.main.gameObject)
		{
			Object.DestroyImmediate(Camera.main.gameObject);
		}
		if (!Game.GetIsMultiplayer())
		{
			Player.ClearPlayers();
		}
		OrderedUpdateManager.UnregisterUpdate(SensorTickPart1);
		OrderedUpdateManager.UnregisterUpdate(SensorTickPart2);
		OrderedUpdateManager.UnregisterUpdate(SensorTickPart3);
		OrderedUpdateManager.UnregisterUpdate(OverrideManager.Update);
	}

	private void OnApplicationQuit()
	{
		Debug.Log("Application ending after " + Time.time + " seconds");
	}
}
