using System;
using System.Collections.Generic;
using Baltarus.Sandbox;
using Baltarus.UI;
using DebugTools;
using Silica.UI.Menus;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Silica.UI;

public class UIManager : MonoBehaviour
{
	public static UIManager Instance;

	private float m_CinematicPct;

	private int m_LastUIHideLevel;

	private int m_UIHideLevel;

	public InputRepository InputRepository;

	[Tooltip("All UI collections containing related UI elements")]
	public UICollection[] UICollections;

	[Tooltip("Canvas containing world space UI elements, such as targetting UI")]
	public Canvas WorldSpaceCanvas;

	[Tooltip("Canvas containing FPS minimap")]
	public GameObject MinimapCanvas;

	[Tooltip("Special objects that hide when the game pauses")]
	public List<GameObject> HideDuringPause = new List<GameObject>();

	[Tooltip("Canvas containing RTS world space UI elements, such as selection rings")]
	public Canvas RTSWorldCanvas;

	[Tooltip("Unity's event system component")]
	public EventSystem EventSystem;

	[Tooltip("Special raycasts applicable only for UI elements")]
	public List<GraphicRaycaster> GraphicRaycasters;

	[Tooltip("All elements raycasted during last raycast")]
	public List<RaycastResult> MouseOverElements = new List<RaycastResult>();

	public bool MouseOverButton;

	[Tooltip("Cursor data")]
	public PointerEventData PointerEventData;

	[Tooltip("UI hide levels")]
	public UIHideElement[] HideLevels;

	public UIHighlightables UIHighlightables;

	private UIMode previousMode;

	private UISubset previousSubset;

	public UIDatabase UIDatabase;

	public static bool TextInputActive => (object)ActiveTextInputField != null;

	public static TMP_InputField ActiveTextInputField { get; set; }

	public int UIHideLevel
	{
		get
		{
			if (!GameMode.CurrentGameMode || !GameMode.InCinematicMoment)
			{
				return m_UIHideLevel;
			}
			return UIHideLevelMax;
		}
		set
		{
			m_UIHideLevel = value;
			if (UIHideLevel > HideLevels.Length)
			{
				UIHideLevel = 0;
			}
		}
	}

	public int UIHideLevelMax => HideLevels.Length;

	[field: SerializeField]
	public bool SimplifiedUIModes { get; private set; }

	[Tooltip("UI Mode - infantry, vehicle, alien or strategy")]
	public UIMode CurrentMode { get; private set; }

	[Tooltip("UI Mode subset - generic or harvester")]
	public UISubset CurrentSubset { get; private set; }

	public static UIMode TargetMode { get; private set; }

	public static UISubset TargetSubset { get; private set; }

	public static bool ForceUpdateUI { get; private set; }

	public bool MouseOverScrollView { get; private set; }

	public bool IsInOverlayMode { get; private set; }

	public static bool MouseOverAnyElement
	{
		get
		{
			if ((bool)Instance)
			{
				return Instance.MouseOverElements.Count > 0;
			}
			return false;
		}
	}

	public static event OnUISwitch OnUISwitch;

	public static void ClearPreviousUIMode()
	{
		if ((bool)Instance)
		{
			Instance.previousMode = UIMode.None;
			Instance.previousSubset = UISubset.Generic;
		}
	}

	public void UpdateUIHideLevel()
	{
		if (UIHideLevel == m_LastUIHideLevel)
		{
			return;
		}
		m_LastUIHideLevel = UIHideLevel;
		for (int i = 0; i < HideLevels.Length; i++)
		{
			GameObject[] hideObjects = HideLevels[i].HideObjects;
			for (int j = 0; j < hideObjects.Length; j++)
			{
				hideObjects[j].SetActive(UIHideLevel < i + 1);
			}
		}
	}

	protected void Awake()
	{
		Instance = this;
		EventSystem = EventSystem.current;
		PointerEventData = new PointerEventData(EventSystem);
	}

	protected void Start()
	{
		InputSystemUIInputModule component = EventSystem.gameObject.GetComponent<InputSystemUIInputModule>();
		if (!(component == null))
		{
			GameInput.SetInputModule(component);
		}
	}

	protected void OnEnable()
	{
		Instance = this;
		GameEvents.OnPlayerJoined = (Action<Player>)Delegate.Combine(GameEvents.OnPlayerJoined, new Action<Player>(OnPlayerJoined));
		GameEvents.OnPlayerLeft = (Action<Player>)Delegate.Combine(GameEvents.OnPlayerLeft, new Action<Player>(OnPlayerLeft));
		GameEvents.OnPlayerChangedTeam = (Action<Player, Team, Team>)Delegate.Combine(GameEvents.OnPlayerChangedTeam, new Action<Player, Team, Team>(OnPlayerChangedTeam));
		Player.OnCurrentPlayerChangedUnit = (Action<Unit, Unit>)Delegate.Combine(Player.OnCurrentPlayerChangedUnit, new Action<Unit, Unit>(OnCurrentPlayerChangedUnit));
		UpdateUIHideLevel();
		ForceUpdateUI = true;
	}

	protected void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		GameEvents.OnPlayerJoined = (Action<Player>)Delegate.Remove(GameEvents.OnPlayerJoined, new Action<Player>(OnPlayerJoined));
		GameEvents.OnPlayerLeft = (Action<Player>)Delegate.Remove(GameEvents.OnPlayerLeft, new Action<Player>(OnPlayerLeft));
		GameEvents.OnPlayerChangedTeam = (Action<Player, Team, Team>)Delegate.Remove(GameEvents.OnPlayerChangedTeam, new Action<Player, Team, Team>(OnPlayerChangedTeam));
		Player.OnCurrentPlayerChangedUnit = (Action<Unit, Unit>)Delegate.Remove(Player.OnCurrentPlayerChangedUnit, new Action<Unit, Unit>(OnCurrentPlayerChangedUnit));
	}

	protected void LateUpdate()
	{
		if (Game.CheatsEnabled && !TextInputActive && !DebugConsole.IsOpen && GameInput.GetButtonDown("Hide UI", GameInput.InputMap.Cheats))
		{
			UIHideLevel++;
		}
		if (!Game.CheatsEnabled && UIHideLevel != 0)
		{
			UIHideLevel = 0;
		}
		UpdateUIHideLevel();
		if (PointerEventData != null && PointerEventData.position != GameInput.PointerPosition)
		{
			RaycastUI();
		}
		IsInOverlayMode = (bool)MenuManager.Instance && MenuManager.Instance.IsMenuOpened<InGameMenu>();
		bool flag = Game.IsPaused && !GameVideo.IsPlaying && UIHideLevel == 0 && !MenuManager.Instance.IsMenuOpened<InGameMenu>() && !MP_Sandbox.CurrentPlayerIsGameMaster();
		bool waitingForServer = NetworkGameClient.GetWaitingForServer();
		bool flag2 = IsInOverlayMode | waitingForServer | flag;
		if (flag2 && TargetMode != UIMode.None)
		{
			previousMode = TargetMode;
			previousSubset = TargetSubset;
			RequestSwitchUIMode(UIMode.None);
			foreach (GameObject item in HideDuringPause)
			{
				if ((bool)item)
				{
					item.SetActive(value: false);
				}
			}
		}
		else if (!flag2 && previousMode != UIMode.None)
		{
			RequestSwitchUIMode(previousMode, previousSubset);
			previousMode = UIMode.None;
			previousSubset = UISubset.Generic;
			foreach (GameObject item2 in HideDuringPause)
			{
				if ((bool)item2)
				{
					item2.SetActive(value: true);
				}
			}
		}
		UpdateUIMode();
	}

	public void OnPlayerJoined(Player player)
	{
		BaseGameModeHandler.StaticUpdatePlayersList();
	}

	public void OnPlayerLeft(Player player)
	{
		BaseGameModeHandler.StaticUpdatePlayersList();
	}

	public void OnPlayerChangedTeam(Player player, Team oldTeam, Team newTeam)
	{
		BaseGameModeHandler.StaticUpdatePlayersList();
	}

	private void OnCurrentPlayerChangedUnit(Unit oldUnit, Unit newUnit)
	{
		if ((bool)newUnit)
		{
			RequestSwitchUIMode(newUnit.GetUIMode(), newUnit.GetUISubsetMode());
		}
	}

	public static void RequestSwitchUIMode(UIMode mode, UISubset subset = UISubset.Generic, bool forceUpdate = false)
	{
		if ((bool)Instance)
		{
			Instance.RequestSwitchMode(mode, subset, forceUpdate);
		}
	}

	public void RequestSwitchMode(UIMode mode, UISubset subset = UISubset.Generic, bool forceUpdate = false)
	{
		if (SimplifiedUIModes)
		{
			UIMode uIMode;
			switch (mode)
			{
			case UIMode.Infantry:
			case UIMode.Vehicle:
			case UIMode.Alien:
				uIMode = UIMode.FPS;
				break;
			case UIMode.RTSHuman:
			case UIMode.RTSAlien:
				uIMode = UIMode.RTS;
				break;
			default:
				uIMode = mode;
				break;
			}
			mode = uIMode;
		}
		TargetMode = mode;
		TargetSubset = subset;
		ForceUpdateUI = forceUpdate;
	}

	private void UpdateUIMode()
	{
		if (CurrentMode != TargetMode || CurrentSubset != TargetSubset || ForceUpdateUI)
		{
			SwitchUIMode(TargetMode, TargetSubset);
			ForceUpdateUI = false;
		}
	}

	private void SwitchUIMode(UIMode mode, UISubset subset = UISubset.Generic)
	{
		CurrentMode = mode;
		CurrentSubset = subset;
		if (CurrentMode == UIMode.None)
		{
			Crosshair.CurrentTarget = null;
			if ((bool)Minimap.Instance)
			{
				Minimap.Instance.Clear();
			}
			if ((bool)WorldSpaceCanvas)
			{
				WorldSpaceCanvas.gameObject.SetActive(value: false);
			}
			if ((bool)RTSWorldCanvas)
			{
				RTSWorldCanvas.gameObject.SetActive(value: false);
			}
			if ((bool)MinimapCanvas)
			{
				MinimapCanvas.gameObject.SetActive(value: false);
			}
		}
		else
		{
			UIMode currentMode = CurrentMode;
			if (currentMode == UIMode.Infantry || currentMode == UIMode.Vehicle || currentMode == UIMode.FPS)
			{
				if ((bool)RTSWorldCanvas)
				{
					RTSWorldCanvas.gameObject.SetActive(value: false);
				}
				if ((bool)MinimapCanvas)
				{
					MinimapCanvas.SetActive(value: true);
				}
			}
			else if (CurrentMode == UIMode.Alien)
			{
				if ((bool)Minimap.Instance)
				{
					Minimap.Instance.Clear();
				}
				if ((bool)RTSWorldCanvas)
				{
					RTSWorldCanvas.gameObject.SetActive(value: false);
				}
				if ((bool)MinimapCanvas)
				{
					MinimapCanvas.gameObject.SetActive(value: false);
				}
			}
			else
			{
				currentMode = CurrentMode;
				if (currentMode == UIMode.RTSHuman || currentMode == UIMode.RTSAlien || currentMode == UIMode.RTS)
				{
					if ((bool)Minimap.Instance)
					{
						Minimap.Instance.Clear();
					}
					if ((bool)RTSWorldCanvas)
					{
						RTSWorldCanvas.gameObject.SetActive(value: true);
					}
					if ((bool)MinimapCanvas)
					{
						MinimapCanvas.gameObject.SetActive(value: false);
					}
				}
				else if (CurrentMode == UIMode.RTSSandbox)
				{
					if ((bool)Minimap.Instance)
					{
						Minimap.Instance.Clear();
					}
					if ((bool)RTSWorldCanvas)
					{
						RTSWorldCanvas.gameObject.SetActive(value: true);
					}
					if ((bool)MinimapCanvas)
					{
						MinimapCanvas.gameObject.SetActive(value: false);
					}
				}
			}
		}
		UICollection[] uICollections = UICollections;
		foreach (UICollection uICollection in uICollections)
		{
			if (CurrentMode == UIMode.None || uICollection.Mode != CurrentMode)
			{
				uICollection.gameObject.SetActive(value: false);
			}
			else if (uICollection.gameObject.activeSelf)
			{
				uICollection.Refresh();
			}
			else
			{
				uICollection.gameObject.SetActive(value: true);
			}
		}
		OnUISwitch?.Invoke(CurrentMode);
	}

	private void RaycastUI()
	{
		if (GraphicRaycasters.Count == 0 || PointerEventData == null)
		{
			return;
		}
		MouseOverElements.Clear();
		PointerEventData.position = GameInput.PointerPosition;
		for (int i = 0; i < GraphicRaycasters.Count; i++)
		{
			GraphicRaycasters[i].Raycast(PointerEventData, MouseOverElements);
		}
		MouseOverButton = false;
		for (int j = 0; j < MouseOverElements.Count; j++)
		{
			if ((bool)MouseOverElements[j].gameObject.GetComponent<Button>())
			{
				MouseOverButton = true;
				break;
			}
		}
		MouseOverScrollView = false;
		for (int k = 0; k < MouseOverElements.Count; k++)
		{
			if ((bool)MouseOverElements[k].gameObject.transform.parent.GetComponent<ScrollRect>())
			{
				MouseOverScrollView = true;
				break;
			}
		}
	}

	public void RegisterRaycaster(GraphicRaycaster raycaster)
	{
		GraphicRaycasters.Add(raycaster);
	}

	public void UnregisterRaycaster(GraphicRaycaster raycaster)
	{
		GraphicRaycasters.Remove(raycaster);
	}
}
