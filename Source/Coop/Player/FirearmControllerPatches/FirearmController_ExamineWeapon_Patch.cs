﻿using SIT.Core.Coop.NetworkPacket;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Core.Coop.Player.FirearmControllerPatches
{
    internal class FirearmController_ExamineWeapon_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "ExamineWeapon";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public static List<string> CallLocally = new();

        [PatchPrefix]
        public static bool PrePatch(EFT.Player.FirearmController __instance, EFT.Player ____player)
        {
            var player = ____player;
            if (player == null)
                return false;

            if (CallLocally.Contains(player.ProfileId))
                return true;

            return false;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player.FirearmController __instance)
        {
            var player = ReflectionHelpers.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
            if (player == null)
                return;

            if (CallLocally.Contains(player.ProfileId))
            {
                CallLocally.Remove(player.ProfileId);
                return;
            }

            AkiBackendCommunication.Instance.SendDataToPool(new BasePlayerPacket(player.ProfileId, "ExamineWeapon").Serialize());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            Logger.LogInfo("FirearmController_ExamineWeapon_Patch:Replicated");

            BasePlayerPacket examineWeaponPacket = new();

            if (dict.ContainsKey("data"))
                examineWeaponPacket = examineWeaponPacket.DeserializePacketSIT(dict["data"].ToString());

            if (HasProcessed(GetType(), player, examineWeaponPacket))
                return;

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                CallLocally.Add(player.ProfileId);
                firearmCont.ExamineWeapon();
            }
        }
    }
}
