using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMoveObj : MonoBehaviour
{
    // lagi gerak kaga
    public bool isMoving = false;

    // kecepatan gerak objnya
    [SerializeField] private float objMoveSpeed;

    // Update is called once per frame
    void Update()
    {
        if (!isMoving) return;

        gameObject.transform.Translate(Vector3.back * objMoveSpeed * Time.deltaTime, Space.World); // bikin biar obj nya gerak ke belakang
    }
}
