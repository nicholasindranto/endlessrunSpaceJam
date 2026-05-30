using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FTUEManager : MonoBehaviour
{
    // singleton
    public static FTUEManager instance { get; private set; }

    // reference ini udah di sequence ke berapa
    public int currentSequence;

    // apakah udah done FTUE nya
    public bool isFTUEFinished;

    public GameObject ftueUI;

    private void OnEnable() {
        MainMenuManager.OnFTUEStart += () => currentSequence = 1;
        FTUESeq1.OnEnterSequence2 += () => currentSequence = 2;
        FTUESeq2.OnEnterSequence3 += () => currentSequence = 3;
        FTUESeq3.OnEnterSequence4 += () => currentSequence = 4;
        FTUESeq4.OnEnterSequence5 += () => currentSequence = 5;
    }

    private void OnDisable() {
        MainMenuManager.OnFTUEStart -= () => currentSequence = 1;
        FTUESeq1.OnEnterSequence2 -= () => currentSequence = 2;
        FTUESeq2.OnEnterSequence3 -= () => currentSequence = 3;
        FTUESeq3.OnEnterSequence4 -= () => currentSequence = 4;
        FTUESeq4.OnEnterSequence5 -= () => currentSequence = 5;
    }

    private void Awake() {
        instance = this;
    }
}
