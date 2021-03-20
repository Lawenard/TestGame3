using UnityEngine;
using System.Collections.Generic;

public class TowerPool : MonoBehaviour
{
    private GameObject prefab;
    private TowerController controller;
    private readonly List<GameObject> objPool = new List<GameObject>();

    public GameObject GetTowerPart(int id)
    {
        if (id >= objPool.Count)
            return InstantinateTowerPart(id);
        else
            return objPool[id];
    }

    private GameObject InstantinateTowerPart(int id)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.transform.localPosition = controller.GetTowerPartPosition(id);
        objPool.Add(obj);
        return obj;
    }

    public void SetPrefab(GameObject prefab)
    {
        if (!this.prefab)
            this.prefab = prefab;
    }

    public void SetController(TowerController controller)
    {
        if (!this.controller)
            this.controller = controller;
    }

    public void DisableAll()
    {
        foreach (GameObject obj in objPool)
            obj.SetActive(false);
    }
}
