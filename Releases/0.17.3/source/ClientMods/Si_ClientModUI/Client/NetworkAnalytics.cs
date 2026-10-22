using HarmonyLib;
using Il2Cpp;

namespace SiSquadHud;
public sealed partial class SquadHud
{
    private readonly TrafficMeter traffic=new();
    // Observe native application packet sizes. Steam/UDP/encryption overhead is outside these hooks.
    [HarmonyPatch(typeof(NetworkLayer),"ProcessMessage")]
    private static class TrafficReceive
    {
        [HarmonyPriority(HarmonyLib.Priority.First)]
        private static void Prefix(uint __1,NetworkID __2,bool __3)
        {
            if(instance==null||__3||!HasSession||Game.GetIsSingleplayer())return;
            try{if(__2.SteamID.m_SteamID==NetworkGameClient.GetServerID().SteamID.m_SteamID)instance.traffic.Received(__1);}catch{}
        }
    }
    [HarmonyPatch(typeof(NetworkLayer),nameof(NetworkLayer.SendPacket),new[]{typeof(GameByteStreamWriter),typeof(ENetworkPacketSend)})]
    private static class TrafficSend
    {
        private static void Prefix(GameByteStreamWriter __0,out long __state)
        {__state=0;try{if(instance!=null&&HasSession&&!Game.GetIsSingleplayer()&&__0!=null)__state=__0.m_MemoryStream.Length;}catch{}}
        private static void Postfix(bool __result,long __state)
        {if(__state>0)instance?.traffic.Sent(__state,__result);}
    }
}
