using System.Collections.Generic;
using GotoUdon.Editor;
using UnityEngine;
using VRC.SDK3.Components;

namespace GotoUdon
{
    public class GotoUdonSettings : ScriptableObject
    {
        // singleton because can't delay udon and we need to be ready to create emulator at any point
        public static GotoUdonSettings Instance;

        public bool enableSimulation = true;
        public GameObject avatarPrefab;
        public Transform spawnPoint;
        public List<PlayerTemplate> playerTemplates = new List<PlayerTemplate>();

        public void Init()
        {
            // lazy handle / ignore incompatible changes
            if (playerTemplates == null)
            {
                playerTemplates = new List<PlayerTemplate>();
            }

            if (playerTemplates.Count == 0)
            {
                playerTemplates.Add(PlayerTemplate.CreateNewPlayer(true));
            }

            playerTemplates.RemoveAll(obj => obj == null);

            // try to use vrc component and get first spawn location
            if (spawnPoint == null)
            {
                VRCSceneDescriptor descriptor = FindObjectOfType<VRCSceneDescriptor>();
                if (descriptor != null && descriptor.spawns != null && descriptor.spawns.Length > 0)
                {
                    spawnPoint = descriptor.spawns[0];
                }
            }
        }
    }
}