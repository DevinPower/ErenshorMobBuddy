using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace erenshor_mob_buddy.Patches
{
    [HarmonyPatch(typeof(TypeText))]
    [HarmonyPatch("CheckCommands")]
    class CommandText
    {
        static bool Prefix()
        {
            string fullText = GameData.TextInput.typed.text;
            if (fullText.Length >= 6)
            {
                string command = fullText.Substring(0, 6).ToLower();
                if (command == "/track")
                {
                    try
                    {
                        string MobName = fullText.Replace("/track ", "");

                        if (MobName == "clear")
                        {
                            Mod.ClearTracking();
                            UpdateSocialLog.LogAdd($"<color=#ffba66>MobBuddy: </color><color=#66a1ff>Cleared all tracked mobs.</color>");

                            return false;
                        }

                        bool IsTracking = Mod.ToggleTracking(MobName);
                        if (IsTracking)
                            UpdateSocialLog.LogAdd($"<color=#ffba66>MobBuddy: </color><color=#66a1ff>Now tracking <b>{MobName}</b></color>");
                        else
                            UpdateSocialLog.LogAdd($"<color=#ffba66>MobBuddy: </color><color=#66a1ff>No longer tracking <b>{MobName}</b></color>");
                    }
                    catch { return true; }

                    return false;
                }
            }

            return true;
        }
    }
}
