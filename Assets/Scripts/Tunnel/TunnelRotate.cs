using System;
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

    public bool CanUseInputOnSequence1
    {
        private set;
        get;
    }
    public bool CanUseInputOnSequence2
    {
        private set;
        get;
    }
    public bool CanUseInputOnSequence3
    {
        private set;
        get;
    }
    public bool CanUseInputOnSequence4
    {
        private set;
        get;
    }

    // lagi di sequence 1 kah?
    public bool IsInSequence1 { get; private set; }

    // event end of sequence 1
    public static event Action EndOfSequence1;

    // Track input mode untuk disable rotation saat sequence 2
    // private FTUEGameplaySequence.InputMode currentInputMode = FTUEGameplaySequence.InputMode.AllEnabled;
    
    // Event untuk FTUE: ketika D/Right Arrow di-tekan
    // public static event Action OnKeyboardRightInput;

    private void Start() {
        // set can use inputnya true
        CanUseInputOnSequence1 = true;
        IsInSequence1 = false;

        // set can use inputnya true
        CanUseInputOnSequence2 = true;

        // set can use inputnya true
        CanUseInputOnSequence3 = true;

        // set can use inputnya true
        CanUseInputOnSequence4 = true;
    }

    private void OnEnable() {
        SwipeController.OnSwipeLeft += HandleSwipeLeft;
        SwipeController.OnSwipeRight += HandleSwipeRight;

        // subscribe ke FTUE nya
        // FTUEGameplaySequence.OnDisableAllInput += () => CanUseInput = false;
        // FTUEGameplaySequence.OnEnableAllInput += () => CanUseInput = true;
        
        // subscribe ke input mode change untuk tracking
        // FTUEGameplaySequence.OnInputModeChange += HandleInputModeChange;

        FTUESeq1.OnIgnoreAllInputExceptRight += HandleOnSequence1;
        FTUESeq1.OnStopIgnoringInput += () => CanUseInputOnSequence1 = true;

        FTUESeq2.OnIgnoreAllInput += () => CanUseInputOnSequence2 = false;
        FTUESeq2.OnStopIgnoringInput += () => CanUseInputOnSequence2 = true;

        FTUESeq3.OnIgnoreAllInputExceptDown += () => CanUseInputOnSequence3 = false;
        FTUESeq3.OnStopIgnoringInput += () => CanUseInputOnSequence3 = true;

        FTUESeq4.OnIgnoreAllInputExceptUp += () => CanUseInputOnSequence4 = false;
        FTUESeq4.OnStopIgnoringInput += () => CanUseInputOnSequence4 = true;
    }

    private void OnDisable() {
        SwipeController.OnSwipeLeft -= HandleSwipeLeft;
        SwipeController.OnSwipeRight -= HandleSwipeRight;

        // unsubscribe dari FTUE nya
        // FTUEGameplaySequence.OnDisableAllInput -= () => CanUseInput = false;
        // FTUEGameplaySequence.OnEnableAllInput -= () => CanUseInput = true;
        
        // unsubscribe dari input mode change
        // FTUEGameplaySequence.OnInputModeChange -= HandleInputModeChange;

        FTUESeq1.OnIgnoreAllInputExceptRight -= HandleOnSequence1;
        FTUESeq1.OnStopIgnoringInput -= () => CanUseInputOnSequence1 = true;

        FTUESeq2.OnIgnoreAllInput -= () => CanUseInputOnSequence2 = false;
        FTUESeq2.OnStopIgnoringInput -= () => CanUseInputOnSequence2 = true;

        FTUESeq3.OnIgnoreAllInputExceptDown -= () => CanUseInputOnSequence3 = false;
        FTUESeq3.OnStopIgnoringInput -= () => CanUseInputOnSequence3 = true;

        FTUESeq4.OnIgnoreAllInputExceptUp -= () => CanUseInputOnSequence4 = false;
        FTUESeq4.OnStopIgnoringInput -= () => CanUseInputOnSequence4 = true;
    }

    private void HandleOnSequence1()
    {
        CanUseInputOnSequence1 = false;
        IsInSequence1 = true;
    }

    // private void HandleInputModeChange(FTUEGameplaySequence.InputMode mode)
    // {
    //     currentInputMode = mode;
    // }

    private void HandleSwipeLeft()
    {
        // kalau belum start gamenya maka skip
        if (!MainMenuManager.instance.isAlreadyStarted) return;

        // kalau lagi sprint maka skip
        if (PlayerSprint.IsSprinting) return;

        // apakah bisa menggunakan input?
        if (!CanUseInputOnSequence1 || !CanUseInputOnSequence2 || !CanUseInputOnSequence3 || !CanUseInputOnSequence4) return;

        // kalau diteken maka langsung rotate kekiri
        ChangeTargetRotation(rotationDegree);
    }

    private void HandleSwipeRight()
    {
        // kalau belum start gamenya maka skip
        if (!MainMenuManager.instance.isAlreadyStarted) return;

        // kalau lagi sprint maka skip
        if (PlayerSprint.IsSprinting) return;

        // apakah bisa menggunakan input?
        if (!CanUseInputOnSequence2 || !CanUseInputOnSequence3 || !CanUseInputOnSequence4) return;

        // kalau lagi di sequence 1 maka invoke event nya
        if (IsInSequence1)
        {
            EndOfSequence1?.Invoke();

            // sudah tidak di sequence 1
            IsInSequence1 = false;
        }

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
        if (!CanUseInputOnSequence1 || !CanUseInputOnSequence2 || !CanUseInputOnSequence3 || !CanUseInputOnSequence4) return;

        // kalau diteken maka langsung rotate kekiri
        if (context.started) ChangeTargetRotation(rotationDegree);
    }

    public void RotateRight(InputAction.CallbackContext context)
    {
        // kalau belum start gamenya maka skip
        if (!MainMenuManager.instance.isAlreadyStarted) return;

        // kalau lagi sprint maka skip
        if (PlayerSprint.IsSprinting) return;

        // apakah bisa menggunakan input?
        if (!CanUseInputOnSequence2 || !CanUseInputOnSequence3 || !CanUseInputOnSequence4) return;

        if (context.started)
        {
            // Trigger event untuk FTUE
            // OnKeyboardRightInput?.Invoke();

            // kalau lagi di sequence 1 maka invoke event nya
            if (IsInSequence1)
            {
                EndOfSequence1?.Invoke();

                // sudah tidak di sequence 1
                IsInSequence1 = false;
            }
            
            // kalau diteken maka langsung rotate kekanan
            ChangeTargetRotation(-rotationDegree);
        }
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
