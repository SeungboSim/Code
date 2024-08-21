using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    InGameManager inGameManager;
    public GameObject[] prefabs;
    List<GameObject>[] pools;

    private void Awake()
    {        
        pools = new List<GameObject>[prefabs.Length];
        for (int i = 0; i < pools.Length; i++)
            pools[i] = new List<GameObject>();
    }
    private void Start()
    {
        inGameManager = InGameManager.instance;
        PreSpawn();
    }
    public void PreSpawn()
    {
        for (int i = 0; i < 800; i++)
        {
            GameObject enemy = Get(0);
            enemy.SetActive(false);
        }
        for (int i = 0; i < 400; i++)
        {
            GameObject exp = Get(2);
            exp.SetActive(false);
        }
    }

    public GameObject Get(int index)
    {
        GameObject select = null;

        if (index == 2 && inGameManager.expObjects.Count > 1000)
        {
            select = FindLowestExp().gameObject;
        }
        else
        {
            foreach (GameObject item in pools[index])
            {
                if (!item.activeSelf)
                {
                    select = item;
                    select.SetActive(true);
                    break;
                }
            }

            if (!select)
            {
                select = Instantiate(prefabs[index], transform);
                pools[index].Add(select);
            }
        }  
        return select;
    }
    Exp FindLowestExp()
    {
        List<Exp> exps = inGameManager.expObjects.OrderBy(x=>x.exp).ToList();
        return exps[0];
    }
}
