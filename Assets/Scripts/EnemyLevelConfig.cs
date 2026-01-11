using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "Enemy level _", menuName = "Enemy level", order = 0)]
    public class EnemyLevelConfig : ScriptableObject
    {
        public GameObject prefab;
        public int enemyCount;
    }
}