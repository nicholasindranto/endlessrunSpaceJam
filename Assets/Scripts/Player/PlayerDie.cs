using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDie : MonoBehaviour
{
    // nama tag obstaclenya
    public readonly string obstacleTagName = "Obstacle";

    // event nya
    public static event Action OnHitObstacle;

    // audionya
    [SerializeField] private AudioClip hitSFX;
    [SerializeField] private AudioSource source;

    // kalau nabrak obstacle maka game over
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(obstacleTagName))
        {
            // play sfx nya juga
            source.PlayOneShot(hitSFX);

            OnHitObstacle?.Invoke();
        }
    }
}
