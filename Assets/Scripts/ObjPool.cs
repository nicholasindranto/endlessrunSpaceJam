using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjPool : MonoBehaviour
{
    public enum KindOfObject
    {
        Obstacle,
        Energy,
        Weapon
    }

    public static ObjPool instance;

    public GameObject obstaclePrefab;
    public GameObject energyPrefab;
    public GameObject weaponPrefab;

    private Queue<GameObject> obstaclePool = new Queue<GameObject>();
    private Queue<GameObject> energyPool = new Queue<GameObject>();
    private Queue<GameObject> weaponPool = new Queue<GameObject>();

    private void Awake()
    {
        instance = this;
    }

    private IEnumerator Start()
    {
        yield return StartCoroutine(CreatePoolCoroutine());
    }

    private IEnumerator CreatePoolCoroutine()
    {
        // bikin 10 saja tapi pelan pelan biar CPU bisa napas
        yield return StartCoroutine(WhatToCreate(obstaclePrefab, 10, obstaclePool)); // obstacle

        yield return StartCoroutine(WhatToCreate(energyPrefab, 5, energyPool)); // energy

        yield return StartCoroutine(WhatToCreate(weaponPrefab, 5, weaponPool)); // weapon
    }

    private IEnumerator WhatToCreate(GameObject obj, int howMany, Queue<GameObject> whichOne)
    {
        for (int i = 0; i < howMany; i++)
        {
            GameObject instantiateObj = Instantiate(obj, transform);

            // pas di instantiate cari dulu automovenya dan pastikan nggak gerak di awal
            if (instantiateObj.TryGetComponent(out AutoMoveObj script)) script.isMoving = false;

            instantiateObj.SetActive(false);
            whichOne.Enqueue(instantiateObj);
            yield return new WaitForSeconds(0.1f);
        }
    }

    // buat ngambil
    public GameObject GetFromPool(KindOfObject what)
    {
        switch (what)
        {
            case KindOfObject.Obstacle:
                // kalau ada di return, kalau nggak ada maka di instantiate
                if (obstaclePool.Count > 0) return obstaclePool.Dequeue();
                else return Instantiate(obstaclePrefab, transform);
            case KindOfObject.Energy:
                // kalau ada di return, kalau nggak ada maka di instantiate
                if (energyPool.Count > 0) return energyPool.Dequeue();
                else return Instantiate(energyPrefab, transform);
            case KindOfObject.Weapon:
                // kalau ada di return, kalau nggak ada maka di instantiate
                if (weaponPool.Count > 0) return weaponPool.Dequeue();
                else return Instantiate(weaponPrefab, transform);
            default:
                Debug.LogError("nothing");
                return null;
        }
    }

    public void ReturnToPool(GameObject obj, KindOfObject toWhat)
    {
        obj.SetActive(false); // matiin
        switch (toWhat)
        {
            case KindOfObject.Obstacle:
                obstaclePool.Enqueue(obj);
                break;
            case KindOfObject.Energy:
                energyPool.Enqueue(obj);
                break;
            case KindOfObject.Weapon:
                weaponPool.Enqueue(obj);
                break;
            default:
                Debug.LogError("nothing");
                break;
        }
    }
}