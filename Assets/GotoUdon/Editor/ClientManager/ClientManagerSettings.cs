using System.Collections.Generic;
using GotoUdon.Utils;
using UnityEngine;
using VRC.Core;

namespace GotoUdon.Editor.ClientManager
{
    public class ClientManagerSettings : ScriptableObject
    {
        public string worldId;

        public string userId;

        // public bool sendInvitesOnUpdate = true;
        public ApiWorldInstance.AccessType accessType = ApiWorldInstance.AccessType.InviteOnly;
        public List<ClientSettings> clients;
        public int sameInstanceRestartDelay = 10;

        public string WorldId
        {
            get => !string.IsNullOrWhiteSpace(worldId) ? worldId : worldId = VRCUtils.FindWorldID();
            set => worldId = value;
        }

        public string UserId
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(userId) && APIUser.CurrentUser?.id != null && userId != APIUser.CurrentUser.id)
                {
                    userId = APIUser.CurrentUser.id;
                }

                return !string.IsNullOrWhiteSpace(userId) ? userId : userId = VRCUtils.FindWorldID();
            }
            set => userId = value;
        }

        public void Init()
        {
            if (worldId == null) worldId = VRCUtils.FindWorldID();
            if (userId == null) userId = APIUser.CurrentUser?.id;
            if (clients == null) clients = new List<ClientSettings>();
            if (clients.Count == 0)
            {
                clients.Add(new ClientSettings()
                {
                    name = "Default profile",
                    profile = 0,
                    enabled = true,
                    vr = true
                });
            }
        }
    }
}