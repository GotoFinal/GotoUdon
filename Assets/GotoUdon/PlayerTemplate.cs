using UnityEngine;

namespace GotoUdon.Editor
{
    [System.Serializable]
    public class PlayerTemplate
    {
        private static int _nextNumber = 1;

        public string playerName;
        public GameObject avatarPrefab;
        public Transform spawnPoint;
        public bool hasVr;
        public bool joinByDefault;
        public int customId;


        public static PlayerTemplate CreateNewPlayer(bool firstOne)
        {
            return new PlayerTemplate
            {
                playerName = "GotoFinal_" + _nextNumber++,
                joinByDefault = firstOne,
                customId = -1
            };
        }
    }
}