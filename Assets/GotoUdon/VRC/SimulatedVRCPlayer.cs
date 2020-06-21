#if GOTOUDON_SIMULATION_TEMP_DISABLED
using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;
using GotoUdon.Utils;

namespace GotoUdon.VRC
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class SimulatedVRCPlayer : MonoBehaviour
    {
        private static readonly Color defaultColor = new Color(138, 43, 226);

        private VRCPlayer _vrcPlayer;
        private Dictionary<string, string> _tags = new Dictionary<string, string>();

        public VRCPlayer VRCPlayer => _vrcPlayer;
        public int Id => _vrcPlayer.Id;
        public string Name => _vrcPlayer.displayName;

        public Animator animator;
        public new Rigidbody rigidbody;
        public new CapsuleCollider collider;

        public bool pickupsEnabled = true;
        public bool legacyLocomotion = false;
        public bool immobile = false;
        public int silencedLevel = 0;
        public bool visible = true;
        public float runSpeed = 4;
        public float walkSpeed = 2;
        public float jumpImpulse = 0;
        public float gravityStrength = 1;
        public VRC_Pickup rightHandPickup;
        public VRC_Pickup leftHandPickup;

        // unused by udon? hide for now.
        [NonSerialized] public Color nameplateColor = defaultColor;
        [NonSerialized] public bool nameplateVisibility = true;

        private List<RuntimeAnimatorController> _animatorControllers = new List<RuntimeAnimatorController>();
        private RuntimeAnimatorController _defaultRuntimeAnimatorController;
        private bool _emulatingVRUser = false;

        private Dictionary<string, object> _metadata = new Dictionary<string, object>();

        private void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            // TODO: more vrchat like?
            collider = GetComponent<CapsuleCollider>();
            collider.center = Vector3.up;
            collider.radius = 0.4F;
            collider.height = 2F;
            foreach (SimulatedVRCPlayer otherPlayer in FindObjectsOfType<SimulatedVRCPlayer>())
            {
                if (this != otherPlayer)
                {
                    Physics.IgnoreCollision(collider, otherPlayer.GetComponent<CapsuleCollider>());
                }
            }
        }

        public void OnBecameLocal()
        {
            // local player layer
            SetLayerRecursively(gameObject, 10);
        }

        public void OnBecameRemote()
        {
            // players layer
            SetLayerRecursively(gameObject, 9);
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return;
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                if (child == null) continue;
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        public T SetMetadata<T>(string key, Func<T, T> newValueFunc)
        {
            T newValue = newValueFunc(GetMetadata<T>(key));
            SetMetadata(key, newValue);
            return newValue;
        }

        public T GetMetadata<T>(string key)
        {
            if (!_metadata.ContainsKey(key)) return default;
            return (T) _metadata[key];
        }

        public void SetMetadata(string key, object value)
        {
            _metadata[key] = value;
        }

        public void PromoteToVRUser()
        {
            _emulatingVRUser = true;
            // TODO: somehow allow to move arms/legs around?
        }

        public void DemoteToDesktopUser()
        {
            _emulatingVRUser = false;
            // TODO: somehow allow to move arms/legs around?
        }

        public bool IsUsingVR()
        {
            return _emulatingVRUser;
        }

        public void PopAnimations()
        {
            _animatorControllers.RemoveAt(_animatorControllers.Count - 1);
            animator.runtimeAnimatorController = _animatorControllers.Count == 0
                ? _defaultRuntimeAnimatorController
                : _animatorControllers[_animatorControllers.Count - 1];
        }

        public void PushAnimations(RuntimeAnimatorController animations)
        {
            if (_animatorControllers.Count == 0)
            {
                _defaultRuntimeAnimatorController = this.animator.runtimeAnimatorController;
            }

            _animatorControllers.Add(animations);
            animator.runtimeAnimatorController = animations;
        }

        public bool IsGrounded()
        {
            // TODO
            return true;
        }

        public void RestoreNameplateColor()
        {
            nameplateColor = defaultColor;
        }

        public void RestoreNameplateVisibility()
        {
            // might not be valid, but whatever, does not seems to be important for udon
            nameplateVisibility = true;
        }

        public Dictionary<string, string> GetRawTags()
        {
            return _tags;
        }

        public void AddTag(string tag, string value)
        {
            _tags[tag] = value;
        }

        public string GetTagValue(string tag)
        {
            return _tags[tag];
        }

        public void ClearTags()
        {
            _tags.Clear();
        }

        public bool IsTagged(string tag, string value)
        {
            return _tags.ContainsKey(tag) && _tags[tag] == value;
        }


        public void TeleportToOrientation(
            Vector3 position,
            Quaternion rotation,
            VRC_SceneDescriptor.SpawnOrientation orientation
        )
        {
            // TODO: what to do with spawn orientation?
            TeleportTo(position, rotation);
        }

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        public void PlayHapticEventInHand(
            VRC_Pickup.PickupHand hand,
            float duration,
            float amplitude,
            float frequency
        )
        {
            GotoLog.Log(
                $"Playing haptic event to ${Name}: {hand} duration: {duration}, amplitude: {amplitude}, frequency: {frequency}");
        }

        public VRC_Pickup GetPickupInHand(VRC_Pickup.PickupHand hand)
        {
            // TODO: check default hand behaviour 
            switch (hand)
            {
                case VRC_Pickup.PickupHand.Left:
                    return leftHandPickup;
                case VRC_Pickup.PickupHand.Right:
                    return rightHandPickup;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hand), hand, null);
            }
        }

        public Quaternion GetBoneRotation(HumanBodyBones bone)
        {
            return animator.GetBoneTransform(bone).rotation;
        }

        public Vector3 GetBonePosition(HumanBodyBones bone)
        {
            return animator.GetBoneTransform(bone).position;
        }

        public Transform GetBoneTransform(HumanBodyBones bone)
        {
            return animator.GetBoneTransform(bone);
        }

        public VRCPlayerApi.TrackingData GetTrackingData(VRCPlayerApi.TrackingDataType trackingDataType)
        {
            // TODO: validate that it at least close to whatever game returns
            switch (trackingDataType)
            {
                case VRCPlayerApi.TrackingDataType.Head:
                    return new VRCPlayerApi.TrackingData(GetBonePosition(HumanBodyBones.Head),
                        GetBoneRotation(HumanBodyBones.Head));
                case VRCPlayerApi.TrackingDataType.LeftHand:
                    return new VRCPlayerApi.TrackingData(GetBonePosition(HumanBodyBones.LeftHand),
                        GetBoneRotation(HumanBodyBones.LeftHand));
                case VRCPlayerApi.TrackingDataType.RightHand:
                    return new VRCPlayerApi.TrackingData(GetBonePosition(HumanBodyBones.RightHand),
                        GetBoneRotation(HumanBodyBones.RightHand));
                default:
                    throw new ArgumentOutOfRangeException(nameof(trackingDataType), trackingDataType, null);
            }
        }

        public Ray GetLookRay()
        {
            // TODO
            return new Ray(Vector3.zero, Vector3.zero);
        }

        public void Initialize(VRCPlayer vrcPlayer, GameObject avatar)
        {
            _vrcPlayer = vrcPlayer;
            vrcPlayer.SimulatedVrcPlayer = this;
            ChangeAvatar(avatar);
        }

        public void ChangeAvatar(GameObject avatar)
        {
            AvatarMarker currentAvatar = GetComponentInChildren<AvatarMarker>();
            if (currentAvatar != null)
            {
                Destroy(currentAvatar.gameObject);
            }

            avatar = Instantiate(avatar, transform);
            avatar.name = "Avatar";
            avatar.AddComponent<AvatarMarker>();
            animator = avatar.GetComponent<Animator>();
            if (animator == null)
            {
                avatar.AddComponent<Animator>();
            }
        }
    }
}
#endif