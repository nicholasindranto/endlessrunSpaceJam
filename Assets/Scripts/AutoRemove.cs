using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRemove : MonoBehaviour
{
    // ini apa objectnya? obstacle atau energy atau weapon?
    [SerializeField] private ObjPool.KindOfObject whatIsThis;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("RemoveObj"))
        {
            Debug.Log("apakah masuk");
            // pastikan dulu dia balik ke tengah dan dia nggak gerak lagi
            gameObject.transform.localPosition = Vector3.zero;
            if (TryGetComponent(out AutoMoveObj script)) script.isMoving = false;

            switch (whatIsThis)
            {
                case ObjPool.KindOfObject.Obstacle:
                    ObjPool.instance.ReturnToPool(gameObject, ObjPool.KindOfObject.Obstacle);
                    break;
                case ObjPool.KindOfObject.Energy:
                    ObjPool.instance.ReturnToPool(gameObject, ObjPool.KindOfObject.Energy);
                    break;
                case ObjPool.KindOfObject.Weapon:
                    ObjPool.instance.ReturnToPool(gameObject, ObjPool.KindOfObject.Weapon);
                    break;
                default:
                    Debug.LogError("nothing");
                    break;
            }
        }
    }
}