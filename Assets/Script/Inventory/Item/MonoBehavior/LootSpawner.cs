using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    [System.Serializable]
    public class LootItem
    {
        public GameObject itemPrefab;
        [Range(0f, 1f)]
        public float dropChance; // 掉落概率，范围为0到1
        //public int dropAmount; // 掉落数量，可以是固定值或者范围，根据需要调整
    }

    public List<LootItem> lootTable; // 掉落表，包含所有可能掉落的物品和它们的掉落概率

    public void SpawnLoot()
    {
        float randomValue = Random.value; // 生成一个0到1之间的随机数
        foreach(var item in lootTable)
        {
            if(randomValue < item.dropChance)
            {
                var spawnItem = Instantiate(item.itemPrefab);
                spawnItem.transform.position = transform.position + Vector3.up*2f ; // 在生成点上方生成物品，可以根据需要调整位置
            }
        }
    }
}
