using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMoveObj : MonoBehaviour
{
    // lagi gerak kaga
    public bool isMoving = false;

    // Update is called once per frame
    void Update()
    {
        if (!isMoving) return;

        if (GameManager.instance.player.TryGetComponent(out PlayerSpeed script))
            gameObject.transform.Translate(Vector3.back * script.currentSpeed * Time.deltaTime, Space.World); // bikin biar obj nya gerak ke belakang
    }
}
