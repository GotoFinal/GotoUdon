using UnityEngine;

namespace GotoUdon.Editor
{
    [System.Serializable]
    public class PlayerTemplate
    {
        public string playerName;
        public GameObject avatarPrefab;
        public Transform spawnPoint;
        public bool hasVr;
        public bool joinByDefault;


        public static PlayerTemplate CreateNewPlayer(bool firstOne)
        {
            return new PlayerTemplate
            {
                playerName = "GotoFinal",
                joinByDefault = firstOne
            };
        }
    }
}