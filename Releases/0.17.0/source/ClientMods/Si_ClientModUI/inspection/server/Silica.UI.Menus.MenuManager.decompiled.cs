using System;
using System.Collections.Generic;
using UnityEngine;

namespace Silica.UI.Menus;

[DefaultExecutionOrder(-1000)]
public class MenuManager : MonoBehaviour
{
	private Dictionary<Type, Menu> _menus = new Dictionary<Type, Menu>();

	private List<Menu> _openMenus = new List<Menu>();

	public static MenuManager Instance { get; private set; }

	public static event Action<Menu> OnMenuOpened;

	public static event Action<Menu> OnMenuClosed;

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		if (GameInput.GetButtonDown("Button East", GameInput.InputMap.Gamepad))
		{
			OnEscape();
		}
		if (_openMenus.Count > 0)
		{
			Menu menu = _openMenus[_openMenus.Count - 1];
			if (menu == null || !menu.IsOpen)
			{
				_openMenus.RemoveAt(_openMenus.Count - 1);
				OnMenuClosed?.Invoke(menu);
			}
		}
	}

	public bool OnEscape()
	{
		if (_openMenus.Count <= 0)
		{
			return false;
		}
		Menu menu = _openMenus[_openMenus.Count - 1];
		if (menu.Back())
		{
			return true;
		}
		if (menu.ShouldCloseOnBack())
		{
			menu.Close();
			_openMenus.RemoveAt(_openMenus.Count - 1);
			OnMenuClosed?.Invoke(menu);
			HandleCovering();
			return true;
		}
		return false;
	}

	public void AddMenu<T>(T menu) where T : Menu
	{
		Type type = menu.GetType();
		if (_menus.ContainsKey(type))
		{
			if (menu == _menus[type])
			{
				return;
			}
			_menus.Remove(type);
		}
		_menus.Add(type, menu);
		if (menu.IsOpen)
		{
			_openMenus.Add(menu);
			OnMenuOpened?.Invoke(menu);
		}
	}

	public void RemoveMenu<T>(T menu) where T : Menu
	{
		if (_menus.TryGetValue(menu.GetType(), out var value) && !(value != menu))
		{
			_menus.Remove(menu.GetType());
		}
	}

	public bool IsMenuOpened<T>(bool includeClosingMenus = false)
	{
		if (_menus.TryGetValue(typeof(T), out var value))
		{
			if (includeClosingMenus && value.OpenClosehandler.State == UIOpenCloseHandler.EState.Closing)
			{
				return true;
			}
			if (value.IsOpen)
			{
				return true;
			}
		}
		return false;
	}

	public bool OpenMenu<T>(out T screen) where T : Menu
	{
		screen = null;
		if (!_menus.TryGetValue(typeof(T), out var value))
		{
			return false;
		}
		screen = (T)value;
		if (value.IsOpen)
		{
			return false;
		}
		if (_openMenus.Contains(value))
		{
			return false;
		}
		value.Open();
		OnMenuOpened?.Invoke(value);
		_openMenus.Add(value);
		HandleCovering();
		return true;
	}

	public bool OpenMenu(Menu menu)
	{
		if (!_menus.TryGetValue(menu.GetType(), out var value) || value != menu)
		{
			return false;
		}
		if (menu.IsOpen)
		{
			return false;
		}
		if (_openMenus.Contains(menu))
		{
			return false;
		}
		menu.Open();
		OnMenuOpened?.Invoke(menu);
		_openMenus.Add(menu);
		HandleCovering();
		return true;
	}

	private void HandleCovering()
	{
		if (_openMenus.Count == 0)
		{
			return;
		}
		bool flag = false;
		for (int num = _openMenus.Count - 1; num >= 0; num--)
		{
			if (flag && _openMenus[num].HideWhenCovered)
			{
				if (_openMenus[num].IsOpen)
				{
					_openMenus[num].Close();
				}
			}
			else
			{
				if (!_openMenus[num].IsOpen)
				{
					_openMenus[num].Open();
				}
				if (_openMenus[num].CoversWholeScreen)
				{
					flag = true;
				}
			}
		}
	}

	public bool CloseMenu<T>() where T : Menu
	{
		if (!_menus.TryGetValue(typeof(T), out var value))
		{
			return false;
		}
		if (!value.IsOpen)
		{
			return false;
		}
		value.Close();
		_openMenus.Remove(value);
		OnMenuClosed?.Invoke(value);
		HandleCovering();
		return true;
	}

	public bool CloseMenu(Menu menu)
	{
		if (!_menus.TryGetValue(menu.GetType(), out var value) || value != menu)
		{
			return false;
		}
		if (!menu.IsOpen)
		{
			return false;
		}
		menu.Close();
		_openMenus.Remove(menu);
		OnMenuClosed?.Invoke(menu);
		HandleCovering();
		return true;
	}

	public Menu GetCurrent()
	{
		if (_openMenus.Count == 0)
		{
			return null;
		}
		return _openMenus[_openMenus.Count - 1];
	}

	public static bool IsMenuOpen<T>() where T : Menu
	{
		if (Instance == null)
		{
			return false;
		}
		if (!Instance._menus.TryGetValue(typeof(T), out var value))
		{
			return false;
		}
		return value.IsOpen;
	}

	public bool IsAnyMenuOpened(bool includeClosingMenus = false)
	{
		foreach (KeyValuePair<Type, Menu> menu in _menus)
		{
			if (includeClosingMenus && menu.Value.OpenClosehandler.State == UIOpenCloseHandler.EState.Closing)
			{
				return true;
			}
			if (menu.Value.IsOpen)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyActiveMenuCoveringWholeScreen()
	{
		foreach (KeyValuePair<Type, Menu> menu in _menus)
		{
			if (menu.Value.IsOpen && menu.Value.CoversWholeScreen)
			{
				return true;
			}
		}
		return false;
	}
}
