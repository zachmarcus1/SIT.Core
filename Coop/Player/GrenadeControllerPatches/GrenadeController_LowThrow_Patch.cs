﻿using SIT.Coop.Core.Player;
using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Core.Coop.Player.GrenadeControllerPatches
{
    internal class GrenadeController_LowThrow_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => null;
        public override string MethodName => "LowThrow";

        public GrenadeController_LowThrow_Patch()
        {
        }

        public GrenadeController_LowThrow_Patch(Type type)
        {
            OverrideInstanceType = type;
        }

        protected override MethodBase GetTargetMethod()
        {
            if (OverrideInstanceType == null)
            {
                var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName, false, true);
                return method;
            }
            else
            {
                var method = ReflectionHelpers.GetMethodForType(OverrideInstanceType, MethodName, false, true);
                return method;
            }
        }

        public static Dictionary<string, bool> CallLocally
            = new Dictionary<string, bool>();

        /// <summary>
        /// Disable patch from starting with the others automatically
        /// </summary>
        public override bool DisablePatch => true;


        [PatchPrefix]
        public static bool PrePatch(
            object __instance,
            EFT.Player ____player
            )
        {
            if (____player == null)
            {
                Logger.LogError("Player property is NULL!");
                return false;
            }

            var result = false;
            if (CallLocally.TryGetValue(____player.Profile.AccountId, out var expecting) && expecting)
                result = true;

            //Logger.LogDebug("GrenadeController_LowThrow_Patch:PrePatch");

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(object __instance,
            EFT.Player ____player
            )
        {
            if (____player == null)
            {
                Logger.LogError("Player property could not be found!");
                return;
            }

            if (CallLocally.TryGetValue(____player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(____player.Profile.AccountId);
                return;
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("t", DateTime.Now.Ticks);
            dictionary.Add("rX", ____player.Rotation.x);
            dictionary.Add("rY", ____player.Rotation.y);
            dictionary.Add("m", "LowThrow");
            ServerCommunication.PostLocalPlayerData(____player, dictionary);

            //Logger.LogDebug("GrenadeController_LowThrow_Patch:PostPatch");

        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            var playerHandsController = player.HandsController;
            //Logger.LogDebug("GrenadeController_LowThrow_Patch:Replicated");
            if (player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
            {
                if (prc.IsClientDrone)
                {
                    var rX = float.Parse(dict["rX"].ToString());
                    var rY = float.Parse(dict["rY"].ToString());
                    Vector2 rot = new Vector2(rX, rY);
                    player.Rotation = rot;
                }
            }

            CallLocally.Add(player.Profile.AccountId, true);
            ReflectionHelpers.GetMethodForType(player.HandsController.GetType(), MethodName).Invoke(playerHandsController, new object[] { });
        }
    }
}
