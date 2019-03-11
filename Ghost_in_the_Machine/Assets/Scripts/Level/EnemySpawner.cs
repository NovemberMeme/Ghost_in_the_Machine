using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> monsters = new List<GameObject>();
    public int maxMonsterCount;

    public void SpawnMonsters()
    {
        int spawnCount = Random.Range(0, maxMonsterCount);

        for (int i = 0; i < spawnCount; i++)
        {
            int monsterSelection = Random.Range(0, monsters.Count);
            Instantiate(monsters[monsterSelection], transform.position, Quaternion.identity);
        }
    }
}
