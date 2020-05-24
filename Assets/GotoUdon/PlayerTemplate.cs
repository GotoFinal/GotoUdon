using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace GotoUdon.Editor
{
    [System.Serializable]
    public class PlayerTemplate
    {
        private static readonly Random _random = new System.Random();

        private static readonly string[] nameGeneratorWords =
        {
            "Goto", "Final", "The", "Ant", "Super", "Dog", "Cat", "Like", "Loli", "Hate", "Mark", "Eddie", "Steve", "Blue", "Black",
            "White", "Yellow", "Red", "Green", "Pro", "Cookie", "Lover", "I", "Love", "Fox", "Foxes", "Cats", "Dogs", "Cookies",
            ""
        };

        public string playerName;
        public GameObject avatarPrefab;
        public Transform spawnPoint;
        public bool hasVr;
        public bool joinByDefault;
        public int customId;


        public static PlayerTemplate CreateNewPlayer(bool firstOne)
        {
            HashSet<string> usedNamed = new HashSet<string>();
            foreach (PlayerTemplate instancePlayerTemplate in GotoUdonSettings.Instance.playerTemplates)
            {
                usedNamed.Add(instancePlayerTemplate.playerName);
            }

            string newName = GenerateName();
            while (usedNamed.Contains(newName))
            {
                newName += _random.Next(0, 10);
            }

            return new PlayerTemplate
            {
                playerName = newName,
                joinByDefault = firstOne,
                customId = -1
            };
        }

        private static string GenerateName()
        {
            int length = _random.Next(1, 6);
            string name = "";
            for (int i = 0; i < length; i++)
            {
                name += nameGeneratorWords[_random.Next(nameGeneratorWords.Length)];
            }

            return name;
        }
    }
}