using System;
using System.Collections.Generic;
using DebugTools;
using Silica.Keybinds;
using Silica.UI;
using Silica.UI.Menus;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Utilities;

namespace Silica;

public static class GameInput
{
	[Serializable]
	[Flags]
	public enum InputMap
	{
		None = 1,
		Global = 2,
		UI = 4,
		Unit = 8,
		Commander = 0x10,
		Gamepad = 0x20,
		Freecam = 0x40,
		Cheats = 0x80,
		Sandbox = 0x100,
		Voting = 0x200
	}

	public delegate bool ActionLabelProvider(InputAction action, out string mapLabel, out string actionLabel);

	public const string KBM_SCHEME = "KBM";

	public const string GAMEPAD_SCHEME = "Gamepad";

	public const string GENERATED_MAP_PREFIX = "Cmd/";

	public static ActionLabelProvider LabelProvider;

	public static Action<InputAction, InputBinding, int, bool> OnRebind;

	public static Action<string, EControlScheme> OnControlsUpdated;

	public static Action OnSwitchedInput;

	private static Dictionary<InputMap, ActionMapContainer> _actionMaps = new Dictionary<InputMap, ActionMapContainer>();

	private static Dictionary<InputActionMap, string[]> _concurrentMaps = new Dictionary<InputActionMap, string[]>();

	private static Dictionary<InputMap, Dictionary<string, ActionMapContainer.ToggleContainer>> _toggleActions = new Dictionary<InputMap, Dictionary<string, ActionMapContainer.ToggleContainer>>();

	private static GameInputAsset _currentAsset;

	private static PlayerInput _currentPlayerInput;

	private static ActionMapContainer.ActionContainer _pointerAction;

	private static ActionMapContainer.ActionContainer _aimAction;

	private static ActionMapContainer.ActionContainer _scrollAction;

	private static InputMap _currentInput = InputMap.None;

	private static InputActionRebindingExtensions.RebindingOperation _rebindingOperation;

	private static Vector2 _pointerPosition;

	private static InputBinding[] _rightStickPathMasks = new InputBinding[5]
	{
		new InputBinding
		{
			path = "<Gamepad>/rightStick"
		},
		new InputBinding
		{
			path = "<Gamepad>/rightStick/up"
		},
		new InputBinding
		{
			path = "<Gamepad>/rightStick/down"
		},
		new InputBinding
		{
			path = "<Gamepad>/rightStick/right"
		},
		new InputBinding
		{
			path = "<Gamepad>/rightStick/left"
		}
	};

	private static InputBinding[] _leftStickPathMasks = new InputBinding[5]
	{
		new InputBinding
		{
			path = "<Gamepad>/leftStick"
		},
		new InputBinding
		{
			path = "<Gamepad>/leftStick/up"
		},
		new InputBinding
		{
			path = "<Gamepad>/leftStick/down"
		},
		new InputBinding
		{
			path = "<Gamepad>/leftStick/right"
		},
		new InputBinding
		{
			path = "<Gamepad>/leftStick/left"
		}
	};

	private static InputBinding _mouseDeltaMask = new InputBinding
	{
		path = "<Mouse>/delta"
	};

	public static GameInputAsset CurrentAsset => _currentAsset;

	public static PlayerInput CurrentPlayerInput => _currentPlayerInput;

	public static EControlScheme CurrentControlScheme { get; private set; }

	public static InputMap CurrentInput => _currentInput;

	public static Vector2 AimDelta { get; private set; }

	public static float ScrollDelta { get; private set; }

	public static Vector2 PointerPosition
	{
		get
		{
			if (CurrentControlScheme != EControlScheme.KeyboardMouse)
			{
				return new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
			}
			return _pointerPosition;
		}
	}

	public static IReadOnlyDictionary<InputMap, ActionMapContainer> RegisteredActionMaps => _actionMaps;

	public static IReadOnlyDictionary<InputActionMap, string[]> ConcurrentMaps => _concurrentMaps;

	public static bool TryGetActionLabel(InputAction action, out string mapLabel, out string actionLabel)
	{
		mapLabel = null;
		actionLabel = null;
		if (action != null && LabelProvider != null)
		{
			return LabelProvider(action, out mapLabel, out actionLabel);
		}
		return false;
	}

	public static TValue ReadValue<TValue>(string action, InputMap actionMap) where TValue : struct
	{
		ActionMapContainer.ActionContainer actionContainer = ((actionMap == InputMap.None) ? FindActionInCurrentlyActiveMaps(action) : GetActionContainer(actionMap, action));
		if (actionContainer != null)
		{
			try
			{
				if (actionContainer.action.enabled || actionContainer.EnableOverride)
				{
					return actionContainer.action.ReadValue<TValue>();
				}
				return default;
			}
			catch (Exception arg)
			{
				DebugConsole.Log($"GameInput.ReadValue: {arg}", DebugConsole.LogLevel.Error);
			}
		}
		return default;
	}

	public static bool GetButtonUp(string action, InputMap actionMap)
	{
		ActionMapContainer.ActionContainer actionContainer = ((actionMap == InputMap.None) ? FindActionInCurrentlyActiveMaps(action) : GetActionContainer(actionMap, action));
		if (actionContainer != null && actionContainer.action.enabled)
		{
			return actionContainer.state == EInputState.ReleasedThisFrame;
		}
		return false;
	}

	public static bool GetButtonDown(string action, InputMap actionMap)
	{
		ActionMapContainer.ActionContainer actionContainer = ((actionMap == InputMap.None) ? FindActionInCurrentlyActiveMaps(action) : GetActionContainer(actionMap, action));
		if (actionContainer != null && actionContainer.action.enabled)
		{
			return actionContainer.state == EInputState.PressedThisFrame;
		}
		return false;
	}

	private static ActionMapContainer.ActionContainer FindActionInCurrentlyActiveMaps(string action)
	{
		foreach (KeyValuePair<InputMap, ActionMapContainer> actionMap in _actionMaps)
		{
			if (_currentInput.HasFlag(actionMap.Key))
			{
				ActionMapContainer.ActionContainer actionContainer = GetActionContainer(actionMap.Key, action);
				if (actionContainer != null)
				{
					return actionContainer;
				}
			}
		}
		return null;
	}

	public static bool GetButton(string action, InputMap actionMap)
	{
		ActionMapContainer.ActionContainer actionContainer = ((actionMap == InputMap.None) ? FindActionInCurrentlyActiveMaps(action) : GetActionContainer(actionMap, action));
		if (actionContainer != null && actionContainer.action.enabled)
		{
			return actionContainer.state != EInputState.Up;
		}
		return false;
	}

	public static bool UsingGamepad(string action, InputMap actionMap)
	{
		ActionMapContainer.ActionContainer actionContainer = ((actionMap == InputMap.None) ? FindActionInCurrentlyActiveMaps(action) : GetActionContainer(actionMap, action));
		if (actionContainer != null && actionContainer.action.enabled)
		{
			return actionContainer.action.IsGamepad();
		}
		return false;
	}

	public static bool GetToggle(string action, InputMap actionMap)
	{
		if (_toggleActions.TryGetValue(actionMap, out var value) && value.TryGetValue(action, out var value2))
		{
			return value2.State;
		}
		return false;
	}

	public static void InterruptToggle(string action, InputMap actionMap)
	{
		if (_toggleActions.TryGetValue(actionMap, out var value) && value.TryGetValue(action, out var value2))
		{
			value2.State = false;
		}
	}

	public static InputMap FindInputMapByName(string name)
	{
		foreach (KeyValuePair<InputMap, ActionMapContainer> actionMap in _actionMaps)
		{
			if (actionMap.Value.actionMap.name == name)
			{
				return actionMap.Key;
			}
		}
		return InputMap.None;
	}

	public static InputMap GetPreferredInput()
	{
		InputMap inputMap = InputMap.None;
		if (DebugConsole.IsOpen)
		{
			return inputMap | InputMap.UI;
		}
		if (Game.InMainMenu || ((bool)MenuManager.Instance && MenuManager.Instance.IsAnyMenuOpened(includeClosingMenus: true)))
		{
			return inputMap | InputMap.UI;
		}
		if (ConfirmationPopup.IsOpen)
		{
			return inputMap | InputMap.UI;
		}
		if ((bool)GlobeHandler.Instance)
		{
			return inputMap | InputMap.Commander;
		}
		if (Cursor.visible && !StrategyMode.InStrategyMode)
		{
			return inputMap | InputMap.UI;
		}
		if ((bool)Unit.CurrentUnit && !Unit.CurrentUnit.IsDestroyed)
		{
			inputMap |= InputMap.Unit;
			return inputMap | InputMap.Voting;
		}
		if (StrategyMode.InStrategyMode)
		{
			inputMap |= InputMap.Commander;
			return inputMap | InputMap.Voting;
		}
		return InputMap.Unit | InputMap.Freecam;
	}

	public static void UpdateInput()
	{
		if ((bool)Player.CurrentPlayer)
		{
			InputMap inputMap = GetPreferredInput();
			if (GameMode.CurrentGameMode != null)
			{
				inputMap = GameMode.CurrentGameMode.OverrideInput(inputMap);
			}
			if (Game.CheatsEnabled)
			{
				inputMap |= InputMap.Cheats;
			}
			inputMap |= InputMap.Global;
			inputMap |= InputMap.Gamepad;
			if (DebugConsole.IsOpen)
			{
				inputMap = InputMap.None;
			}
			if (CurrentInput != inputMap)
			{
				SwitchInput(inputMap);
			}
		}
	}

	public static void SetToggleMode(string action, bool toggle, InputMap actionMap = InputMap.None)
	{
		if (actionMap == InputMap.None)
		{
			return;
		}
		Dictionary<string, ActionMapContainer.ToggleContainer> value2;
		if (!_actionMaps.TryGetValue(actionMap, out var value))
		{
			DebugConsole.Log($"GameInput.SetToggleMode: Action map '{actionMap}' not registered", DebugConsole.LogLevel.Warning);
		}
		else if (!value.actionContainers.ContainsKey(action))
		{
			DebugConsole.Log("GameInput.SetToggleMode: Action '" + action + "' not present in action map", DebugConsole.LogLevel.Warning);
		}
		else if (toggle)
		{
			_toggleActions.TryAdd(actionMap, new Dictionary<string, ActionMapContainer.ToggleContainer>());
			if (!_toggleActions[actionMap].TryAdd(action, new ActionMapContainer.ToggleContainer()))
			{
				DebugConsole.Log("GameInput.SetToggleMode: Action '" + action + "' already set to toggle mode", DebugConsole.LogLevel.Warning);
			}
		}
		else if (_toggleActions.TryGetValue(actionMap, out value2) && value2.ContainsKey(action))
		{
			_toggleActions[actionMap].Remove(action);
		}
	}

	public static bool GetToggleMode(string action, InputMap actionMap)
	{
		if (_toggleActions.ContainsKey(actionMap) && _toggleActions[actionMap].ContainsKey(action))
		{
			return true;
		}
		return false;
	}

	public static void SetPlayerInput(PlayerInput playerInput)
	{
		if (playerInput == null)
		{
			DebugConsole.Log("GameInput.SetPlayerInput: The given PlayerInput is null", DebugConsole.LogLevel.Error);
			return;
		}
		if ((bool)_currentPlayerInput)
		{
			_currentPlayerInput.onControlsChanged -= OnControlsChanged;
			_currentPlayerInput.enabled = false;
		}
		_currentPlayerInput = playerInput;
		_currentPlayerInput.enabled = true;
		ForceDisableAll();
		EnableAllWithOverride();
		_currentPlayerInput.onControlsChanged += OnControlsChanged;
		OnControlsChanged(_currentPlayerInput);
	}

	public static void SetInputModule(InputSystemUIInputModule inputModule)
	{
		if (CurrentPlayerInput == null)
		{
			DebugConsole.Log("GameInput.SetInputModule: CurrentPlayerInput is null. The PlayerInput component must be set before InputSystemUIInputModule", DebugConsole.LogLevel.Error);
			return;
		}
		if (CurrentAsset == null)
		{
			DebugConsole.Log("GameInput.SetInputModule: CurrentAsset is null. The GameInputAsset must be set before InputSystemUIInputModule", DebugConsole.LogLevel.Error);
			return;
		}
		if (CurrentPlayerInput.uiInputModule != null)
		{
			CurrentPlayerInput.uiInputModule.enabled = false;
		}
		CurrentPlayerInput.uiInputModule = inputModule;
		inputModule.actionsAsset = CurrentAsset.currentActions.asset;
		inputModule.enabled = true;
		ForceDisableAll();
		EnableAllWithOverride();
	}

	public static void SetAsset(GameInputAsset asset)
	{
		if (asset == null)
		{
			DebugConsole.Log("GameInput.SetAsset: Asset is null", DebugConsole.LogLevel.Warning);
		}
		else
		{
			_currentAsset = asset;
		}
	}

	public static void LoadSettings(InputSettings settings)
	{
		if (_currentAsset == null)
		{
			DebugConsole.Log("GameInput.LoadSettings: Asset is null", DebugConsole.LogLevel.Warning);
			return;
		}
		if (settings == null)
		{
			DebugConsole.Log("GameInput.LoadSettings: Settings is null", DebugConsole.LogLevel.Warning);
			return;
		}
		KeybindPersistence.Load(_currentAsset.currentActions.asset, settings.m_Keybinds);
		SetToggleMode("Crouch", settings.m_toggleCrouch, InputMap.Unit);
		SetToggleMode("Speed Boost", settings.m_toggleSprint, InputMap.Unit);
		SetToggleMode("Sights", settings.m_toggleZoom, InputMap.Unit);
		ApplyAimSensitivity(settings);
		ApplyGamepadDeadzone(settings);
	}

	private static void ApplyAimSensitivity(InputSettings settings)
	{
		if (settings != null)
		{
			bool invertAimHorizontal = settings.m_InvertAimHorizontal;
			bool invertAimVertical = settings.m_InvertAimVertical;
			float num = _currentAsset.mouseSensitivityMax * settings.m_MouseAimSensitivityX;
			float num2 = _currentAsset.mouseSensitivityMax * settings.m_MouseAimSensitivityY;
			float num3 = _currentAsset.gamepadSentivityMax * settings.m_GamepadAimSensitivityX;
			float num4 = _currentAsset.gamepadSentivityMax * settings.m_GamepadAimSensitivityY;
			if (_aimAction != null)
			{
				_aimAction.action.ApplyParameterOverride("invertX", invertAimHorizontal, _rightStickPathMasks[0]);
				_aimAction.action.ApplyParameterOverride("invertY", !invertAimVertical, _rightStickPathMasks[0]);
				_aimAction.action.ApplyParameterOverride("invertX", invertAimHorizontal, _mouseDeltaMask);
				_aimAction.action.ApplyParameterOverride("invertY", !invertAimVertical, _mouseDeltaMask);
				_aimAction.action.ApplyParameterOverride("x", num, _mouseDeltaMask);
				_aimAction.action.ApplyParameterOverride("y", num2, _mouseDeltaMask);
				_aimAction.action.ApplyParameterOverride("x", num3, _rightStickPathMasks[0]);
				_aimAction.action.ApplyParameterOverride("y", num4, _rightStickPathMasks[0]);
			}
		}
	}

	private static void ApplyGamepadDeadzone(InputSettings settings)
	{
		if (settings == null)
		{
			return;
		}
		foreach (KeyValuePair<InputMap, ActionMapContainer> actionMap in _actionMaps)
		{
			foreach (InputAction action in actionMap.Value.actionMap.actions)
			{
				for (int i = 0; i < _rightStickPathMasks.Length; i++)
				{
					action.ApplyParameterOverride("min", settings.m_rightAxisDeadzoneMin, _rightStickPathMasks[i]);
					action.ApplyParameterOverride("max", settings.m_rightAxisDeadzoneMax, _rightStickPathMasks[i]);
					action.ApplyParameterOverride("min", settings.m_leftAxisDeadzoneMin, _leftStickPathMasks[i]);
					action.ApplyParameterOverride("max", settings.m_leftAxisDeadzoneMax, _leftStickPathMasks[i]);
					action.ApplyParameterOverride("min", settings.m_rightAxisDeadzoneMin, _rightStickPathMasks[i]);
					action.ApplyParameterOverride("max", settings.m_rightAxisDeadzoneMax, _rightStickPathMasks[i]);
					action.ApplyParameterOverride("min", settings.m_leftAxisDeadzoneMin, _leftStickPathMasks[i]);
					action.ApplyParameterOverride("max", settings.m_leftAxisDeadzoneMax, _leftStickPathMasks[i]);
				}
			}
		}
	}

	public static void ProcessInput()
	{
		AimDelta = Vector2.zero;
		ScrollDelta = 0f;
		if (_rebindingOperation != null)
		{
			return;
		}
		foreach (KeyValuePair<InputMap, ActionMapContainer> actionMap in _actionMaps)
		{
			actionMap.Value.ProcessInput();
		}
		ProcessInputToggle();
		bool flag = (bool)MenuManager.Instance && MenuManager.Instance.IsAnyMenuOpened();
		if (_aimAction != null)
		{
			AimDelta = (flag ? Vector2.zero : _aimAction.action.ReadValue<Vector2>());
		}
		if (_pointerAction != null)
		{
			_pointerPosition = _pointerAction.action.ReadValue<Vector2>();
		}
		if (_scrollAction != null)
		{
			ScrollDelta = _scrollAction.action.ReadValue<Vector2>().y / 120f;
		}
	}

	public static void SetPointerAction(InputMap actionMap, string action)
	{
		ActionMapContainer.ActionContainer actionContainer = GetActionContainer(actionMap, action);
		if (actionContainer != null)
		{
			_pointerAction = actionContainer;
		}
	}

	public static void SetAimAction(InputMap actionMap, string action)
	{
		ActionMapContainer.ActionContainer actionContainer = GetActionContainer(actionMap, action);
		if (actionContainer != null)
		{
			_aimAction = actionContainer;
		}
	}

	public static void SetScrollAction(InputMap actionMap, string action)
	{
		ActionMapContainer.ActionContainer actionContainer = GetActionContainer(actionMap, action);
		if (actionContainer != null)
		{
			_scrollAction = actionContainer;
		}
	}

	public static void RegisterActionMap(InputActionMap actionMap, InputMap inputMap)
	{
		if (actionMap == null)
		{
			DebugConsole.Log("GameInput.RegisterActionMap: Action map is null", DebugConsole.LogLevel.Warning);
		}
		else if (_actionMaps.ContainsKey(inputMap))
		{
			DebugConsole.Log("GameInput.RegisterActionMap: Action map with name '" + actionMap.name + "' already registered", DebugConsole.LogLevel.Warning);
		}
		else
		{
			_actionMaps.Add(inputMap, new ActionMapContainer(actionMap));
		}
	}

	public static void RegisterConcurrentMap(InputActionMap map, params string[] peerMapNames)
	{
		if (map != null && peerMapNames != null && peerMapNames.Length != 0)
		{
			_concurrentMaps[map] = peerMapNames;
		}
	}

	public static bool AreConcurrent(InputActionMap a, InputActionMap b)
	{
		if (a == null || b == null || a == b)
		{
			return false;
		}
		if (_concurrentMaps.TryGetValue(a, out var value) && Array.IndexOf(value, b.name) >= 0)
		{
			return true;
		}
		if (_concurrentMaps.TryGetValue(b, out var value2))
		{
			return Array.IndexOf(value2, a.name) >= 0;
		}
		return false;
	}

	private static void SwitchInput(InputMap actionMap)
	{
		if (_currentInput == actionMap)
		{
			ClearAllInput();
			return;
		}
		foreach (KeyValuePair<InputMap, ActionMapContainer> actionMap2 in _actionMaps)
		{
			bool enabled = actionMap.HasFlag(actionMap2.Key);
			actionMap2.Value.SetEnabled(enabled);
		}
		SetCurrentInputValue(actionMap);
		ClearAllInput();
		OnSwitchedInput?.Invoke();
	}

	public static void SetActionEnableOverride(InputMap actionMap, string action, bool state)
	{
		if (actionMap != InputMap.None)
		{
			ActionMapContainer.ActionContainer value2;
			if (!_actionMaps.TryGetValue(actionMap, out var value))
			{
				DebugConsole.Log($"GameInput.SetActionEnableOverride: Action map '{actionMap}' not registered", DebugConsole.LogLevel.Warning);
			}
			else if (!value.actionContainers.TryGetValue(action, out value2))
			{
				DebugConsole.Log("GameInput.SetActionEnableOverride: Action '" + action + "' not present in action map", DebugConsole.LogLevel.Warning);
			}
			else
			{
				value2.EnableOverride = state;
			}
		}
	}

	private static void SetCurrentInputValue(InputMap newCurrentInput)
	{
		_currentInput = newCurrentInput;
	}

	public static void DebugPrintInputMode()
	{
		Debug.Log($"Current action map = '{CurrentInput}'");
	}

	private static void ProcessInputToggle()
	{
		bool flag = false;
		foreach (InputMap key in _toggleActions.Keys)
		{
			foreach (string key2 in _toggleActions[key].Keys)
			{
				bool state = _toggleActions[key][key2].State;
				flag = GetButtonDown(key2, key);
				state = ((!state) ? flag : (!flag));
				_toggleActions[key][key2].State = state;
			}
		}
	}

	public static void ClearAllInput()
	{
		_pointerPosition = (AimDelta = Vector2.zero);
		ScrollDelta = 0f;
		foreach (KeyValuePair<InputMap, ActionMapContainer> actionMap in _actionMaps)
		{
			actionMap.Value.ClearAllInput();
		}
		foreach (InputMap key in _toggleActions.Keys)
		{
			foreach (string key2 in _toggleActions[key].Keys)
			{
				_toggleActions[key][key2].State = false;
			}
		}
	}

	public static void ForceDisableAll()
	{
		if (_currentAsset == null)
		{
			DebugConsole.Log("GameInput.ForceDisableAll: Asset is null", DebugConsole.LogLevel.Warning);
			return;
		}
		foreach (KeyValuePair<InputMap, ActionMapContainer> actionMap in _actionMaps)
		{
			actionMap.Value.ForceDisableAll();
		}
	}

	public static void EnableAllWithOverride()
	{
		foreach (KeyValuePair<InputMap, ActionMapContainer> actionMap in _actionMaps)
		{
			if (_currentInput.HasFlag(actionMap.Key))
			{
				actionMap.Value.SetEnabled(state: true);
			}
			else
			{
				actionMap.Value.SetEnabled(state: false);
			}
		}
	}

	public static InputAction GetAction(InputMap actionMap, string action)
	{
		return GetActionContainer(actionMap, action)?.action;
	}

	public static InputAction GetAction(InputActionReference inputActionReference)
	{
		if (!inputActionReference)
		{
			return null;
		}
		return GetActionContainer(FindInputMapByName(inputActionReference.action.actionMap.name), inputActionReference.action.name)?.action;
	}

	private static ActionMapContainer.ActionContainer GetActionContainer(InputMap actionMap, string action)
	{
		if (!_actionMaps.TryGetValue(actionMap, out var value))
		{
			return null;
		}
		return value.actionContainers.GetValueOrDefault(action);
	}

	public static bool TryGetValidBinding(InputAction inputAction, out InputBinding validbinding, out int bindingIndex)
	{
		validbinding = default;
		bindingIndex = -1;
		if (inputAction == null)
		{
			return false;
		}
		string currentControlScheme = CurrentPlayerInput.currentControlScheme;
		int count = inputAction.bindings.Count;
		for (int i = 0; i < count; i++)
		{
			InputBinding inputBinding = inputAction.bindings[i];
			if (InputBinding.MaskByGroup(currentControlScheme).Matches(inputBinding) && !inputBinding.isComposite && !inputBinding.isPartOfComposite)
			{
				validbinding = inputBinding;
				bindingIndex = i;
				return true;
			}
			if (i + 1 < count && inputBinding.isComposite)
			{
				InputBinding binding = inputAction.bindings[i + 1];
				if (InputBinding.MaskByGroup(currentControlScheme).Matches(binding))
				{
					validbinding = inputBinding;
					bindingIndex = i;
					return true;
				}
			}
		}
		return false;
	}

	public static string GetDisplayString(InputAction inputAction, bool plusInComposites = true)
	{
		if (inputAction == null)
		{
			return string.Empty;
		}
		if (!TryGetValidBinding(inputAction, out var _, out var bindingIndex))
		{
			return string.Empty;
		}
		string result = string.Empty;
		if (bindingIndex >= 0 && bindingIndex < inputAction.bindings.Count)
		{
			result = inputAction.bindings[bindingIndex].ToDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
			if (inputAction.bindings[bindingIndex].isComposite)
			{
				ReadOnlyArray<InputBinding> bindings = inputAction.bindings;
				int count = bindings.Count;
				_ = NameAndParameters.Parse(bindings[bindingIndex].effectivePath).name;
				int num = bindingIndex + 1;
				int i;
				for (i = num; i < count && bindings[i].isPartOfComposite; i++)
				{
				}
				int num2 = i - num;
				string text = string.Empty;
				for (int j = 0; j < num2; j++)
				{
					string text2 = bindings[num + j].ToDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions);
					if (string.IsNullOrEmpty(text2))
					{
						text2 = " ";
					}
					if ((j > 0) & plusInComposites)
					{
						text += "+";
					}
					text += text2;
				}
				result = text;
			}
		}
		return result;
	}

	public static string GetDisplayString(InputMap actionMap, string action)
	{
		ActionMapContainer.ActionContainer actionContainer = GetActionContainer(actionMap, action);
		if (actionContainer == null)
		{
			return string.Empty;
		}
		return GetDisplayString(actionContainer.action);
	}

	private static void OnControlsChanged(PlayerInput playerInput)
	{
		string currentControlScheme = playerInput.currentControlScheme;
		EControlScheme eControlScheme = ((currentControlScheme == "KBM") ? EControlScheme.KeyboardMouse : ((currentControlScheme == "Gamepad") ? EControlScheme.Gamepad : EControlScheme.None));
		EControlScheme eControlScheme2 = eControlScheme;
		if (CurrentControlScheme != eControlScheme2)
		{
			CurrentControlScheme = eControlScheme2;
			OnControlsUpdated?.Invoke(playerInput.currentControlScheme, eControlScheme2);
		}
	}

	public static void OnDeviceChange(InputDevice device, InputDeviceChange change)
	{
		if (change == InputDeviceChange.ConfigurationChanged && device is Keyboard && GameSettings.Instance != null)
		{
			ApplyAimSensitivity(GameSettings.Instance.m_InputSettings);
		}
	}

	public static void OnFocusChanged(bool state)
	{
		ClearAllInput();
	}
}
