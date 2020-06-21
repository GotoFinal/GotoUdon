#if GOTOUDON_SIMULATION_TEMP_DISABLED
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace GotoUdon.VRC
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class VRCPlayer : VRCPlayerApi
    {
        private static int _idCounter;
        private static HashSet<int> usedIds = new HashSet<int>();

        public readonly int Id;
        private SimulatedVRCPlayer _simulatedVrcPlayer;

        public SimulatedVRCPlayer SimulatedVrcPlayer
        {
            get => _simulatedVrcPlayer;
            set
            {
                if (_simulatedVrcPlayer != null)
                {
                    throw new InvalidOperationException(
                        "This player instance is already assigned to simulated component");
                }

                _simulatedVrcPlayer = value;
                gameObject = value.gameObject;
            }
        }

        public VRCPlayer(string name)
        {
            isLocal = false;
            displayName = name;
            while (usedIds.Contains(_idCounter))
            {
                _idCounter += 1;
            }
            Id = _idCounter;
            usedIds.Add(Id);
        }
        
        public VRCPlayer(string name, int id)
        {
            isLocal = false;
            displayName = name;
            Id = id;
            usedIds.Add(id);
        }

        #region vrchatDelegates

#if UNITY_EDITOR
        static VRCPlayer()
        {
            _isMasterDelegate = player => player == VRCEmulator.Instance.Master;
            _isModeratorDelegate = _ => false;
            _isSuperDelegate = _ => false;
            _GetPlayerId = player => GetPlayerId(player as VRCPlayer);
            _GetPlayerByGameObject = GetPlayerByGameObject;
            _GetPlayerById = GetPlayerById;
            _IsOwner = (player, o) => IsOwner(player as VRCPlayer, o);
            _TakeOwnership = (player, gameObject) => TakeOwnership(player as VRCPlayer, gameObject);
            _GetTrackingData = (player, type) => GetTrackingData(player as VRCPlayer, type);
            _GetBoneTransform = (player, bone) => GetBoneTransform(player as VRCPlayer, bone);
            _GetBonePosition = (player, bone) => GetBonePosition(player as VRCPlayer, bone);
            _GetBoneRotation = (player, bone) => GetBoneRotation(player as VRCPlayer, bone);
            _GetPickupInHand = (player, hand) => GetPickupInHand(player as VRCPlayer, hand);
            _PlayHapticEventInHand = (player, pickupHand, duration, amplitude, frequency) =>
                PlayHapticEventInHand(player as VRCPlayer, pickupHand, duration, amplitude, frequency);
            _TeleportTo = (player, position, rotation) => TeleportTo(player as VRCPlayer, position, rotation);
            _TeleportToOrientation = (player, position, rotation, spawnOrientation) =>
                TeleportToOrientation(player as VRCPlayer, position, rotation, spawnOrientation);
            // Can we emulate that better?
            _TeleportToOrientationLerp = (player, position, rotation, spawnOrientation, lerp) =>
                TeleportToOrientation(player as VRCPlayer, position, rotation, spawnOrientation);
            _EnablePickups = (player, enabled) => EnablePickups(player as VRCPlayer, enabled);
            _SetNamePlateColor = (player, color) => SetNamePlateColor(player as VRCPlayer, color);
            _RestoreNamePlateColor = player => RestoreNamePlateColor(player as VRCPlayer);
            _SetNamePlateVisibility = (player, visibility) => SetNamePlateVisibility(player as VRCPlayer, visibility);
            _RestoreNamePlateVisibility = player => RestoreNamePlateVisibility(player as VRCPlayer);
            _SetPlayerTag = (player, tag, value) => SetPlayerTag(player as VRCPlayer, tag, value);
            _GetPlayerTag = (player, tag) => GetPlayerTag(player as VRCPlayer, tag);
            _GetPlayersWithTag = GetPlayersWithTag;
            _ClearPlayerTags = player => ClearPlayerTags(player as VRCPlayer);
            _SetInvisibleToTagged = (player, invisible, tagName, tagValue) =>
                SetInvisibleToTagged(player as VRCPlayer, invisible, tagName, tagValue);
            _SetInvisibleToUntagged = (player, invisible, tagName, tagValue) =>
                SetInvisibleToUntagged(player as VRCPlayer, invisible, tagName, tagValue);
            _SetSilencedToTagged = (player, level, tagName, tagValue) =>
                SetSilencedToTagged(player as VRCPlayer, level, tagName, tagValue);
            _SetSilencedToUntagged = (player, level, tagName, tagValue) =>
                SetSilencedToUntagged(player as VRCPlayer, level, tagName, tagValue);
            _ClearInvisible = player => ClearInvisible(player as VRCPlayer);
            _ClearSilence = player => ClearSilence(player as VRCPlayer);
            _SetRunSpeed = (player, f) => SetRunSpeed(player as VRCPlayer, f);
            _SetWalkSpeed = (player, f) => SetWalkSpeed(player as VRCPlayer, f);
            _SetJumpImpulse = (player, f) => SetJumpImpulse(player as VRCPlayer, f);
            _SetGravityStrength = (player, f) => SetGravityStrength(player as VRCPlayer, f);
            _GetRunSpeed = player => GetRunSpeed(player as VRCPlayer);
            _GetWalkSpeed = player => GetWalkSpeed(player as VRCPlayer);
            _GetJumpImpulse = player => GetJumpImpulse(player as VRCPlayer);
            _GetGravityStrength = player => GetGravityStrength(player as VRCPlayer);
            _CombatSetup = player => CombatSetup(player as VRCPlayer);
            _CombatSetMaxHitpoints = (player, maxHitpoints) => CombatSetMaxHitpoints(player as VRCPlayer, maxHitpoints);
            _CombatSetCurrentHitpoints = (player, currentHitpoints) =>
                CombatSetCurrentHitpoints(player as VRCPlayer, currentHitpoints);
            _CombatGetCurrentHitpoints = player => CombatGetCurrentHitpoints(player as VRCPlayer);
            _CombatSetRespawn = (player, respawnOnDeath, respawnTimer, respawnLocation) =>
                CombatSetRespawn(player as VRCPlayer, respawnOnDeath, respawnTimer, respawnLocation);
            _CombatSetDamageGraphic = (player, prefab) => CombatSetDamageGraphic(player as VRCPlayer, prefab);
            _CombatGetDestructible = player => CombatGetDestructible(player as VRCPlayer);
            _IsUserInVR = player => IsUserInVr(player as VRCPlayer);
            _UseLegacyLocomotion = player => UseLegacyLocomotion(player as VRCPlayer);
            _UseAttachedStation = player => UseAttachedStation(player as VRCPlayer);
            _PushAnimations = (player, controller) => PushAnimations(player as VRCPlayer, controller);
            _PopAnimations = player => PopAnimations(player as VRCPlayer);
            _Immobilize = (player, immobile) => Immobilize(player as VRCPlayer, immobile);
            _SetVelocity = (player, velocity) => SetVelocity(player as VRCPlayer, velocity);
            _GetVelocity = player => GetVelocity(player as VRCPlayer);
            _GetPosition = player => GetPosition(player as VRCPlayer);
            _GetRotation = player => GetRotation(player as VRCPlayer);
            VRCPlayerApi.SetAnimatorBool = (player, name, value) => SetAnimatorBool(player as VRCPlayer, name, value);
            ClaimNetworkControl = (player, obj) => VRCEmulator.ActionNotImplemented("ClaimNetworkControl");
            VRCPlayerApi.GetLookRay = player => GetLookRay(player as VRCPlayer);
            VRCPlayerApi.IsGrounded = player => IsGrounded(player as VRCPlayer);
        }

        private static string GetGameObjectPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }

        private static void RestoreNamePlateVisibility(VRCPlayer player)
        {
            player.SimulatedVrcPlayer.RestoreNameplateVisibility();
        }

        private static void SetNamePlateVisibility(VRCPlayer player, bool visibility)
        {
            player.SimulatedVrcPlayer.nameplateVisibility = visibility;
        }

        private static void RestoreNamePlateColor(VRCPlayer player)
        {
            player.SimulatedVrcPlayer.RestoreNameplateColor();
        }

        private static void SetNamePlateColor(VRCPlayer player, Color color)
        {
            player.SimulatedVrcPlayer.nameplateColor = color;
        }

        private static bool IsGrounded(VRCPlayer player)
        {
            return player.SimulatedVrcPlayer.IsGrounded();
        }

        private static Ray GetLookRay(VRCPlayer player)
        {
            throw VRCEmulator.ActionNotImplemented("GetLookRay");
        }

        private static void SetAnimatorBool(VRCPlayer player, string name, bool value)
        {
            player.SimulatedVrcPlayer.animator.SetBool(name, value);
        }

        private static Quaternion GetRotation(VRCPlayer player)
        {
            return player.SimulatedVrcPlayer.transform.rotation;
        }

        private static Vector3 GetPosition(VRCPlayer player)
        {
            return player.SimulatedVrcPlayer.transform.position;
        }

        private static Vector3 GetVelocity(VRCPlayer player)
        {
            return player.SimulatedVrcPlayer.rigidbody.velocity;
        }

        private static void SetVelocity(VRCPlayer player, Vector3 velocity)
        {
            player.SimulatedVrcPlayer.rigidbody.velocity = velocity;
        }

        private static void Immobilize(VRCPlayer player, bool immobile)
        {
            player.SimulatedVrcPlayer.immobile = immobile;
        }

        private static void PopAnimations(VRCPlayer player)
        {
            player.SimulatedVrcPlayer.PopAnimations();
        }

        private static void PushAnimations(VRCPlayer player, RuntimeAnimatorController animations)
        {
            player.SimulatedVrcPlayer.PushAnimations(animations);
        }

        private static void UseAttachedStation(VRCPlayer player)
        {
            throw VRCEmulator.ActionNotImplemented("UseAttachedStation");
        }

        private static void UseLegacyLocomotion(VRCPlayer player)
        {
            player.SimulatedVrcPlayer.legacyLocomotion = true;
        }

        private static bool IsUserInVr(VRCPlayer player)
        {
            return player.SimulatedVrcPlayer.IsUsingVR();
        }

        private static IVRC_Destructible CombatGetDestructible(VRCPlayer player)
        {
            return player.gameObject.GetComponent<VRCDestructible>();
        }

        private static void CombatSetDamageGraphic(VRCPlayer player, GameObject prefab)
        {
            throw VRCEmulator.ActionNotImplemented("CombatSetDamageGraphic");
        }

        private static void CombatSetRespawn(
            VRCPlayer player,
            bool respawnOnDeath,
            float respawnTimer,
            Transform respawnLocation
        )
        {
            throw VRCEmulator.ActionNotImplemented("CombatSetRespawn");
        }

        private static float CombatGetCurrentHitpoints(VRCPlayer player)
        {
            return player.gameObject.GetComponent<VRCDestructible>().currentHealth;
        }

        private static void CombatSetCurrentHitpoints(VRCPlayer player, float currentHitpoints)
        {
            player.gameObject.GetComponent<VRCDestructible>().currentHealth = currentHitpoints;
        }

        private static void CombatSetMaxHitpoints(VRCPlayer player, float maxHitpoints)
        {
            player.gameObject.GetComponent<VRCDestructible>().maxHealth = maxHitpoints;
        }

        private static void CombatSetup(VRCPlayer player)
        {
            VRCDestructible vrcDestructible = player.gameObject.GetComponent<VRCDestructible>();
            if (vrcDestructible == null)
            {
                vrcDestructible = player.gameObject.AddComponent<VRCDestructible>();
            }
        }

        private static float GetGravityStrength(VRCPlayer player)
        {
            ThrowIfNotLocal(player);
            return player.SimulatedVrcPlayer.gravityStrength;
        }

        private static float GetJumpImpulse(VRCPlayer player)
        {
            ThrowIfNotLocal(player);
            return player.SimulatedVrcPlayer.jumpImpulse;
        }

        private static float GetWalkSpeed(VRCPlayer player)
        {
            ThrowIfNotLocal(player);
            return player.SimulatedVrcPlayer.walkSpeed;
        }

        private static float GetRunSpeed(VRCPlayer player)
        {
            ThrowIfNotLocal(player);
            return player.SimulatedVrcPlayer.runSpeed;
        }

        private static void SetGravityStrength(VRCPlayer player, float newGravityStrength)
        {
            ThrowIfNotLocal(player);
            player.SimulatedVrcPlayer.gravityStrength = newGravityStrength;
        }

        private static void SetJumpImpulse(VRCPlayer player, float newJumpImpulse)
        {
            ThrowIfNotLocal(player);
            player.SimulatedVrcPlayer.jumpImpulse = newJumpImpulse;
        }

        private static void SetWalkSpeed(VRCPlayer player, float newWalkSpeed)
        {
            ThrowIfNotLocal(player);
            player.SimulatedVrcPlayer.walkSpeed = newWalkSpeed;
        }

        private static void SetRunSpeed(VRCPlayer player, float newRunSpeed)
        {
            ThrowIfNotLocal(player);
            player.SimulatedVrcPlayer.runSpeed = newRunSpeed;
        }

        private static void ClearSilence(VRCPlayer player)
        {
            player.SimulatedVrcPlayer.silencedLevel = 0;
        }

        private static void ClearInvisible(VRCPlayer player)
        {
            player.SimulatedVrcPlayer.visible = true;
        }

        private static void SetSilencedToUntagged(VRCPlayer player, int level, string tagName, string tagValue)
        {
            if (!player.SimulatedVrcPlayer.IsTagged(tagName, tagValue))
            {
                player.SimulatedVrcPlayer.silencedLevel = level;
            }
        }

        private static void SetSilencedToTagged(VRCPlayer player, int level, string tagName, string tagValue)
        {
            if (player.SimulatedVrcPlayer.IsTagged(tagName, tagValue))
            {
                player.SimulatedVrcPlayer.silencedLevel = level;
            }
        }

        private static void SetInvisibleToUntagged(VRCPlayer player, bool invisible, string tagName, string tagValue)
        {
            if (!player.SimulatedVrcPlayer.IsTagged(tagName, tagValue))
            {
                player.SimulatedVrcPlayer.visible = !invisible;
            }
        }

        private static void SetInvisibleToTagged(VRCPlayer player, bool invisible, string tagName, string tagValue)
        {
            if (player.SimulatedVrcPlayer.IsTagged(tagName, tagValue))
            {
                player.SimulatedVrcPlayer.visible = !invisible;
            }
        }

        private static void ClearPlayerTags(VRCPlayer player)
        {
            player.SimulatedVrcPlayer.ClearTags();
        }

        private static List<int> GetPlayersWithTag(string tag, string value)
        {
            List<int> players = new List<int>();
            foreach (var vrcPlayerApi in sPlayers)
            {
                if (vrcPlayerApi is VRCPlayer player && player.SimulatedVrcPlayer.IsTagged(tag, value))
                {
                    players.Add(player.Id);
                }
            }

            return players;
        }

        private static string GetPlayerTag(VRCPlayer player, string tag)
        {
            return player.SimulatedVrcPlayer.GetTagValue(tag);
        }

        private static void SetPlayerTag(VRCPlayer player, string tag, string value)
        {
            player.SimulatedVrcPlayer.AddTag(tag, value);
        }

        private static void EnablePickups(VRCPlayer player, bool enable)
        {
            player.SimulatedVrcPlayer.pickupsEnabled = enable;
        }

        private static void TeleportToOrientation(
            VRCPlayer player,
            Vector3 position,
            Quaternion rotation,
            VRC_SceneDescriptor.SpawnOrientation orientation
        )
        {
            player.SimulatedVrcPlayer.TeleportToOrientation(position, rotation, orientation);
        }

        private static void TeleportTo(VRCPlayer player, Vector3 position, Quaternion rotation)
        {
            player.SimulatedVrcPlayer.TeleportTo(position, rotation);
        }

        private static void PlayHapticEventInHand(
            VRCPlayer player,
            VRC_Pickup.PickupHand hand,
            float duration,
            float amplitude,
            float frequency
        )
        {
            player.SimulatedVrcPlayer.PlayHapticEventInHand(hand, duration, amplitude, frequency);
        }

        private static VRC_Pickup GetPickupInHand(VRCPlayer player, VRC_Pickup.PickupHand hand)
        {
            return player.SimulatedVrcPlayer.GetPickupInHand(hand);
        }

        private static Quaternion GetBoneRotation(VRCPlayer player, HumanBodyBones bone)
        {
            return player.SimulatedVrcPlayer.GetBoneRotation(bone);
        }

        private static Vector3 GetBonePosition(VRCPlayer player, HumanBodyBones bone)
        {
            return player.SimulatedVrcPlayer.GetBonePosition(bone);
        }

        private static Transform GetBoneTransform(VRCPlayer player, HumanBodyBones bone)
        {
            return player.SimulatedVrcPlayer.GetBoneTransform(bone);
        }

        private static TrackingData GetTrackingData(VRCPlayer player,
            TrackingDataType trackingDataType)
        {
            return player.SimulatedVrcPlayer.GetTrackingData(trackingDataType);
        }

        private static void TakeOwnership(VRCPlayer player, GameObject obj)
        {
            VRCObject.AsVrcObject(obj).owner = player.SimulatedVrcPlayer;
        }

        private static bool IsOwner(VRCPlayer player, GameObject obj)
        {
            return VRCObject.AsVrcObject(obj).VRCPlayer == player;
        }

        private static VRCPlayerApi GetPlayerById(int id)
        {
            // any reason to make dictionary for these? I don't think it will be a performance issue here?
            foreach (var vrcPlayer in sPlayers)
            {
                if (vrcPlayer.playerId == id)
                {
                    return vrcPlayer;
                }
            }

            return null;
        }

        private static VRCPlayerApi GetPlayerByGameObject(GameObject gameObject)
        {
            foreach (var vrcPlayer in sPlayers)
            {
                if (vrcPlayer.gameObject == gameObject)
                {
                    return vrcPlayer;
                }
            }

            return null;
        }

        private static int GetPlayerId(VRCPlayer player)
        {
            return player.Id;
        }

        private static void ThrowIfNotLocal(VRCPlayer player)
        {
            if (!player.isLocal) throw new ApplicationException("This method will not work on non local player in VRChat!");
        }
#endif

        #endregion
    }
}
#endif