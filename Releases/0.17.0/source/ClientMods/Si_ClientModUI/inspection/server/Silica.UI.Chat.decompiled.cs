using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Silica.UI.Menus;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Silica.UI;

public class Chat : MonoBehaviour
{
	private struct MessageInfo(Player Sender, string Text, bool TeamOnly)
	{
		public Player Sender = Sender;

		public string Text = Text;

		public bool TeamOnly = TeamOnly;
	}

	public const short MAX_DISPLAYED_MESSAGES = 30;

	public const short MAX_MESSAGE_LENGTH = 160;

	[SerializeField]
	private UIMode[] belongsTo;

	[SerializeField]
	private GameObject MessagePrefab;

	[SerializeField]
	private GameObject wrapper;

	[SerializeField]
	private RectTransform contentParent;

	[SerializeField]
	private TMP_InputField inputField;

	[SerializeField]
	private Button channelSwitch;

	[SerializeField]
	private TextMeshProUGUI channelSwitchLabel;

	[SerializeField]
	private LocalizedString LocalizedTeamChatLabelText = new LocalizedString();

	[SerializeField]
	private LocalizedString LocalizedAllChatLabelText = new LocalizedString();

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private ChatAnimator _chatAnimator;

	private readonly List<ChatMessageUI> messagePool = new List<ChatMessageUI>();

	private string message;

	private ChatChannel channel;

	private Queue<MessageInfo> _messageQueue = new Queue<MessageInfo>();

	private bool _isProcessingMessage;

	private bool _inputActive;

	private static List<MessageInfo> _history = new List<MessageInfo>();

	private CancellationTokenSource _cancellationTokenOnDestroy = new CancellationTokenSource();

	private bool _isLocked;

	protected void Start()
	{
		inputField.characterLimit = 160;
	}

	protected void OnEnable()
	{
		message = null;
		inputField.text = null;
		if ((bool)channelSwitch)
		{
			channelSwitch.onClick.AddListener(SwitchChannel);
		}
		if ((bool)inputField)
		{
			inputField.onValueChanged.AddListener(OnChange);
		}
		GameEvents.OnChatMessage = (Action<Player, string, bool>)Delegate.Combine(GameEvents.OnChatMessage, new Action<Player, string, bool>(MessageReceived));
		GameEvents.OnGameEnded = (Action<GameMode, Team>)Delegate.Combine(GameEvents.OnGameEnded, new Action<GameMode, Team>(OnGameEnded));
		UIManager.OnUISwitch += UISwitched;
		UISwitched(UIMode.None);
		Clear();
		LoadHistory();
		UpdateChannelText();
		MenuManager.OnMenuOpened += MenuOpened;
		MenuManager.OnMenuClosed += MenuClosed;
	}

	protected void OnDisable()
	{
		if ((bool)channelSwitch)
		{
			channelSwitch.onClick.RemoveAllListeners();
		}
		if ((bool)inputField)
		{
			inputField.onValueChanged.RemoveAllListeners();
		}
		GameEvents.OnChatMessage = (Action<Player, string, bool>)Delegate.Remove(GameEvents.OnChatMessage, new Action<Player, string, bool>(MessageReceived));
		GameEvents.OnGameEnded = (Action<GameMode, Team>)Delegate.Remove(GameEvents.OnGameEnded, new Action<GameMode, Team>(OnGameEnded));
		UIManager.OnUISwitch -= UISwitched;
		UIManager.ActiveTextInputField = null;
		MenuManager.OnMenuOpened -= MenuOpened;
		MenuManager.OnMenuClosed -= MenuClosed;
	}

	private void OnDestroy()
	{
		if (_cancellationTokenOnDestroy != null)
		{
			_cancellationTokenOnDestroy.Cancel();
		}
	}

	private void OnGameEnded(GameMode mode, Team team)
	{
		Clear();
	}

	private void Clear()
	{
		for (int i = 0; i < messagePool.Count; i++)
		{
			messagePool[i].gameObject.SetActive(value: false);
		}
	}

	private void LoadHistory()
	{
		foreach (MessageInfo item in _history)
		{
			AddMessage(item);
		}
	}

	private void UISwitched(UIMode mode)
	{
		bool flag = false;
		for (int i = 0; i < belongsTo.Length; i++)
		{
			if (belongsTo[i] == mode)
			{
				flag = true;
				break;
			}
		}
		if (flag && !wrapper.activeSelf)
		{
			wrapper.SetActive(value: true);
		}
		else if (!flag && wrapper.activeSelf)
		{
			wrapper.SetActive(value: false);
		}
	}

	private void OnChange(string value)
	{
		message = value;
	}

	private void SwitchChannel()
	{
		switch (channel)
		{
		case ChatChannel.Public:
			channel = ChatChannel.Team;
			break;
		case ChatChannel.Team:
			channel = ChatChannel.Public;
			break;
		}
		UpdateChannelText();
	}

	private void UpdateChannelText()
	{
		string text = string.Empty;
		switch (channel)
		{
		case ChatChannel.Public:
			text = LocalizedAllChatLabelText.GetLocalizedString();
			break;
		case ChatChannel.Team:
			text = LocalizedTeamChatLabelText.GetLocalizedString();
			break;
		}
		channelSwitchLabel.text = text;
	}

	private void MessageReceived(Player sender, string text, bool teamOnly)
	{
		if (!sender || !sender.IsMuted)
		{
			MessageInfo item = new MessageInfo(sender, text, teamOnly);
			_messageQueue.Enqueue(item);
			_history.Add(item);
		}
	}

	protected void LateUpdate()
	{
		if (_messageQueue.Count > 0 && !_isProcessingMessage)
		{
			ProcessMessage(_messageQueue.Dequeue()).AttachExternalCancellation(_cancellationTokenOnDestroy.Token).Forget();
		}
		if (_isLocked)
		{
			return;
		}
		if (UIManager.TextInputActive && GameInput.GetButtonDown("Chat Channel", GameInput.InputMap.Global))
		{
			SwitchChannel();
		}
		if (GameInput.GetButtonDown("Chat", GameInput.InputMap.Global))
		{
			if (_inputActive)
			{
				_inputActive = false;
				UIManager.ActiveTextInputField = null;
				if (!string.IsNullOrWhiteSpace(message))
				{
					Player.CurrentPlayer.SendChatMessage(message, channel == ChatChannel.Team);
				}
				message = null;
				inputField.text = null;
				inputField.DeactivateInputField();
				if ((bool)EventSystem.current)
				{
					EventSystem.current.SetSelectedGameObject(null);
				}
				if (_chatAnimator != null)
				{
					_chatAnimator.SetChatDisabled();
				}
				LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
				scrollRect.verticalNormalizedPosition = 0f;
				LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
			}
			else
			{
				if (_chatAnimator != null)
				{
					_chatAnimator.SetChatEnabled();
				}
				_inputActive = true;
				UIManager.ActiveTextInputField = inputField;
				inputField.ActivateInputField();
			}
		}
		if (GameInput.GetButtonDown("Chat Hide", GameInput.InputMap.Global) && (bool)_chatAnimator)
		{
			_chatAnimator.ChatToggled();
		}
		float scrollDelta = GameInput.ScrollDelta;
		if (UIManager.TextInputActive && scrollDelta != 0f)
		{
			scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + scrollDelta * 6f * Time.deltaTime);
		}
	}

	private async UniTask ProcessMessage(MessageInfo messageInfo)
	{
		_isProcessingMessage = true;
		LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
		scrollRect.verticalNormalizedPosition = 0f;
		LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
		ChatMessageUI chatMessageUI = AddMessage(messageInfo);
		if ((bool)_chatAnimator && (bool)chatMessageUI)
		{
			await _chatAnimator.OnMessage(chatMessageUI);
		}
		_isProcessingMessage = false;
	}

	private ChatMessageUI AddMessage(MessageInfo messageInfo)
	{
		Player sender = messageInfo.Sender;
		string text = messageInfo.Text;
		bool teamOnly = messageInfo.TeamOnly;
		if (!MessagePrefab)
		{
			Debug.LogWarning("No message prefab found.");
			return null;
		}
		GameObject gameObject = null;
		ChatMessageUI chatMessageUI = null;
		for (int i = 0; i < messagePool.Count; i++)
		{
			GameObject gameObject2 = messagePool[i].gameObject;
			messagePool[i].Alpha01 = 1f;
			if (!gameObject && !gameObject2.activeSelf)
			{
				chatMessageUI = messagePool[i];
				gameObject = gameObject2;
				gameObject.SetActive(value: true);
				messagePool.RemoveAt(i);
				messagePool.Add(chatMessageUI);
				(gameObject.transform as RectTransform).SetAsLastSibling();
				i--;
			}
		}
		if (!gameObject && messagePool.Count == 30)
		{
			chatMessageUI = messagePool[0];
			gameObject = chatMessageUI.gameObject;
			gameObject.SetActive(value: true);
			messagePool.RemoveAt(0);
			messagePool.Add(chatMessageUI);
			(gameObject.transform as RectTransform).SetAsLastSibling();
		}
		if (!gameObject)
		{
			gameObject = UnityEngine.Object.Instantiate(MessagePrefab, contentParent);
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.position = Vector3.zero;
			chatMessageUI = gameObject.GetComponent<ChatMessageUI>();
			messagePool.Add(chatMessageUI);
		}
		if ((bool)chatMessageUI)
		{
			chatMessageUI.SetText(sender, text, teamOnly);
		}
		return chatMessageUI;
	}

	public static void ClearHistory()
	{
		if (_history != null)
		{
			_history.Clear();
		}
	}

	private void MenuClosed(Menu menu)
	{
		if (menu is InGameMenu)
		{
			_isLocked = false;
		}
	}

	private void MenuOpened(Menu menu)
	{
		if (menu is InGameMenu)
		{
			if (_inputActive)
			{
				inputField.DeactivateInputField();
				UIManager.ActiveTextInputField = null;
			}
			_isLocked = true;
			_inputActive = false;
			message = string.Empty;
			if (_chatAnimator != null)
			{
				_chatAnimator.SetChatDisabled();
			}
		}
	}
}
