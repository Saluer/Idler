using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "Enemy level _", menuName = "Enemy level", order = 0)]
    public class EnemyLevelConfig : ScriptableObject
    {
        public List<EnemyCountWrapper> enemies;
        public GameObject enemyPreviewPrefab;
        public GameObject previewPosition;
    }

    [System.Serializable]
    public class EnemyCountWrapper
    {
        public int count;
        public GameObject enemyPrefab;
    }
}