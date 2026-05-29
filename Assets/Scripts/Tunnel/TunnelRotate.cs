using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TunnelRotate : MonoBehaviour
{
    // kecepatan rotasinya
    [SerializeField] private float rotationSpeed;

    // rotasinya mau ke berapa derajat
    private float targetRotation;

    // rotatenya berapa derajat
    [SerializeField] private float rotationDegree;

    public bool CanUseInput
    {
        private set;
        get;
    }

    private void Start() {
        // set can use inputnya true
        CanUseInput = true;
    }

    private void OnEnable() {
        SwipeController.OnSwipeLeft += HandleSwipeLeft;
        SwipeController.OnSwipeRight += HandleSwipeRight;

        // subscribe ke FTUE nya
        FTUEGameplaySequence.OnDisableAllInput += () => CanUseInput = false;
        FTUEGameplaySequence.OnEnableAllInput += () => CanUseInput = true;
    }

    private void OnDisable() {
        SwipeController.OnSwipeLeft -= HandleSwipeLeft;
        SwipeController.OnSwipeRight -= HandleSwipeRight;

        // unsubscribe dari FTUE nya
        FTUEGameplaySequence.OnDisableAllInput -= () => CanUseInput = false;
        FTUEGameplaySequence.OnEnableAllInput -= () => CanUseInput = true;
    }

    private void HandleSwipeLeft()
    {
        // kalau belum start gamenya maka skip
        if (!MainMenuManager.instance.isAlreadyStarted) return;

        // kalau lagi sprint maka skip
        if (PlayerSprint.IsSprinting) return;

        // apakah bisa menggunakan input?
        if (!CanUseInput) return;

        // kalau diteken maka langsung rotate kekiri
        ChangeTargetRotation(rotationDegree);
    }

    private void HandleSwipeRight()
    {
        // kalau belum start gamenya maka skip
        if (!MainMenuManager.instance.isAlreadyStarted) return;

        // kalau lagi sprint maka skip
        if (PlayerSprint.IsSprinting) return;

        // kalau diteken maka langsung rotate kekanan
        ChangeTargetRotation(-rotationDegree);
    }

    public void RotateLeft(InputAction.CallbackContext context)
    {
        // kalau belum start gamenya maka skip
        if (!MainMenuManager.instance.isAlreadyStarted) return;

        // kalau lagi sprint maka skip
        if (PlayerSprint.IsSprinting) return;

        // apakah bisa menggunakan input?
        if (!CanUseInput) return;

        // kalau diteken maka langsung rotate kekiri
        if (context.started) ChangeTargetRotation(rotationDegree);
    }

    public void RotateRight(InputAction.CallbackContext context)
    {
        // kalau belum start gamenya maka skip
        if (!MainMenuManager.instance.isAlreadyStarted) return;

        // kalau lagi sprint maka skip
        if (PlayerSprint.IsSprinting) return;

        // kalau diteken maka langsung rotate kekanan
        if (context.started) ChangeTargetRotation(-rotationDegree);
    }

    // Update is called once per frame
    void Update()
    {
        HandleTunnelRotation();
    }

    private void ChangeTargetRotation(float amount)
    {
        targetRotation += amount;

        // kalau dapat angka lebih dari 360 maka -360, kalau kurang dari 0 maka +360
        if (targetRotation >= 360) targetRotation -= 360;
        else if (targetRotation <= 0) targetRotation += 360;
    }

    private void HandleTunnelRotation()
    {
        // bikin dulu target rotasinya dalam quaternion
        Quaternion targetRotate = Quaternion.Euler(0, 0, targetRotation);

        // baru di totate pakai lerp aja biar ada ease out nya
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotate, rotationSpeed * Time.deltaTime);
    }
}
