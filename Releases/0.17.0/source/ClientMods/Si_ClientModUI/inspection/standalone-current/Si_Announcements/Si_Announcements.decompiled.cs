using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using MelonLoader;
using MelonLoader.Preferences;
using MelonLoader.Utils;
using Microsoft.CodeAnalysis;
using Si_Announcements;
using SilicaAdminMod;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: MelonInfo(typeof(Announcements), "Server Announcements", "1.2.3", "databomb", "https://github.com/data-bomb/Silica")]
[assembly: MelonGame("Bohemia Interactive", "Silica")]
[assembly: MelonOptionalDependencies(new string[] { "Admin Mod" })]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("Si_Announcements")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+6462b97383020630aa9e15ee52fdd9f3e112abe0")]
[assembly: AssemblyProduct("Si_Announcements")]
[assembly: AssemblyTitle("Si_Announcements")]
[assembly: AssemblyVersion("1.0.0.0")]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableAttribute : Attribute
	{
		public readonly byte[] NullableFlags;

		public NullableAttribute(byte P_0)
		{
			NullableFlags = new byte[1] { P_0 };
		}

		public NullableAttribute(byte[] P_0)
		{
			NullableFlags = P_0;
		}
	}
	[CompilerGenerated]
	[Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableContextAttribute : Attribute
	{
		public readonly byte Flag;

		public NullableContextAttribute(byte P_0)
		{
			Flag = P_0;
		}
	}
}
namespace Si_Announcements
{
	public class Announcements : MelonMod
	{
		private static MelonPreferences_Category _modCategory = null;

		private static MelonPreferences_Entry<int> _Announcements_SecondsBetweenMessages = null;

		private static MelonPreferences_Entry<bool> _Announcements_ShowIfLastChatWasAnnouncement = null;

		private static float Timer_Announcement = HelperMethods.Timer_Inactive;

		private static int announcementCount;

		private static string[]? announcementsText;

		private static string? lastChatMessage;

		public override void OnInitializeMelon()
		{
			if (_modCategory == null)
			{
				_modCategory = MelonPreferences.CreateCategory("Silica");
			}
			if (_Announcements_SecondsBetweenMessages == null)
			{
				_Announcements_SecondsBetweenMessages = _modCategory.CreateEntry<int>("Announcements_SecondsBetweenMessages", 420, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			if (_Announcements_ShowIfLastChatWasAnnouncement == null)
			{
				_Announcements_ShowIfLastChatWasAnnouncement = _modCategory.CreateEntry<bool>("Announcements_ShowAnnouncementWhenLastChatWasAnnouncement", true, (string)null, (string)null, false, false, (ValueValidator)null, (string)null);
			}
			string text = MelonEnvironment.UserDataDirectory + "\\announcements.txt";
			try
			{
				if (!File.Exists(text))
				{
					CreateStarterAnnouncementsFile(text);
				}
				using (StreamReader streamReader = File.OpenText(text))
				{
					List<string> list = new List<string>();
					string text2 = "";
					while ((text2 = streamReader.ReadLine()) != null)
					{
						if (IsValidAnnouncement(text2))
						{
							list.Add(text2);
						}
					}
					announcementsText = list.ToArray();
				}
				MelonLogger.Msg("Loaded announcements.txt file with " + announcementsText.Length + " announcements.");
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed in OnInitializeMelon");
			}
		}

		[MethodImpl(MethodImplOptions.NoOptimization)]
		public override void OnLateInitializeMelon()
		{
			HelperMethods.StartTimer(ref Timer_Announcement);
			Event_Chat.OnRequestPlayerChat += OnRequestPlayerChat;
		}

		public override void OnUpdate()
		{
			try
			{
				if (_Announcements_SecondsBetweenMessages == null)
				{
					return;
				}
				Timer_Announcement += Time.deltaTime;
				if (!(Timer_Announcement >= (float)_Announcements_SecondsBetweenMessages.Value))
				{
					return;
				}
				Timer_Announcement = 0f;
				if (announcementsText != null && !(GameMode.CurrentGameMode == null) && GameMode.CurrentGameMode.GameOngoing)
				{
					if (!_Announcements_ShowIfLastChatWasAnnouncement.Value && IsPreviousChatMessageAnnouncement(lastChatMessage))
					{
						MelonLogger.Msg("Skipping Announcement - Repeated Message");
						return;
					}
					string nextAnnouncement = GetNextAnnouncement();
					HelperMethods.SendChatMessageToAll(nextAnnouncement);
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed in OnUpdate");
			}
		}

		public void OnRequestPlayerChat(object? sender, OnRequestPlayerChatArgs args)
		{
			if (args.Player == null)
			{
				return;
			}
			try
			{
				if ((_Announcements_ShowIfLastChatWasAnnouncement == null || !_Announcements_ShowIfLastChatWasAnnouncement.Value) && !args.TeamOnly)
				{
					lastChatMessage = args.Text;
				}
			}
			catch (Exception exception)
			{
				HelperMethods.PrintError(exception, "Failed to run OnRequestPlayerChat");
			}
		}

		private static bool IsPreviousChatMessageAnnouncement(string? previousChat)
		{
			if (previousChat == null || announcementsText == null)
			{
				return false;
			}
			return announcementsText.Any((string m) => m == previousChat);
		}

		private static void CreateStarterAnnouncementsFile(string filePath)
		{
			File.WriteAllText(filePath, "<color=#cc33ff>Server mods by databomb. Report issues in Discord or on GitHub.</color>\n");
		}

		private static string GetNextAnnouncement()
		{
			if (announcementsText == null)
			{
				return "";
			}
			announcementCount++;
			string text = announcementsText[announcementCount % announcementsText.Length];
			MelonLogger.Msg("Announcement: " + text);
			return text;
		}

		private static bool IsValidAnnouncement(string announcementText)
		{
			if (announcementText.Length <= 2)
			{
				return false;
			}
			return true;
		}
	}
}
