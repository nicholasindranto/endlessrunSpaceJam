using System.Collections.Generic;
using UnityEngine;

public class TunnelManager : MonoBehaviour
{
    /*[Header("Tunnel Settings")]
    [Tooltip("Masukkan 4 GameObject Kotak (Children) ke sini secara berurutan")]
    public Transform[] segments; 
    [Tooltip("Panjang dari satu kotak/segmen pada sumbu Z")]
    public float segmentLength = 20f; 
    public float moveSpeed = 15f;
    
    [Header("Rotation Settings")]
    [Tooltip("Kecepatan transisi rotasi agar terasa smooth")]
    public float rotateSpeed = 10f; 

    private float targetZRotation = 0f;

    void Update()
    {
        HandleRotationInput();
        MoveAndLoopSegments();
    }

    private void HandleRotationInput()
    {
        // Deteksi input dari pemain
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            targetZRotation += 90f;
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            targetZRotation -= 90f;
        }

        // Terapkan rotasi secara mulus ke Parent GameObject ini
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZRotation);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
    }

    private void MoveAndLoopSegments()
    {
        for (int i = 0; i < segments.Length; i++)
        {
            // 1. Gerakkan segmen ke arah Z negatif (mendekati pemain)
            // Penting: Gunakan Space.World agar pergerakan selalu ke arah -Z global, 
            // tidak terpengaruh oleh rotasi parent-nya.
            segments[i].Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);

            // 2. Cek apakah segmen sudah melewati batas belakang pemain
            // Asumsi pemain ada di Z = 0. Jika segmen melewati -segmentLength, pindahkan ke depan.
            if (segments[i].position.z < -segmentLength)
            {
                // Cari posisi Z paling depan (furthest) saat ini
                float furthestZ = GetFurthestSegmentZ();
                
                // Pindahkan segmen ini tepat di depan segmen terjauh
                Vector3 newPosition = segments[i].position;
                newPosition.z = furthestZ + segmentLength;
                segments[i].position = newPosition;
            }
        }
    }

    private float GetFurthestSegmentZ()
    {
        float maxZ = float.MinValue;
        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i].position.z > maxZ)
            {
                maxZ = segments[i].position.z;
            }
        }
        return maxZ;
    }*/

    // masukin tunnel / world nya ke dalam array
    [SerializeField] private Transform[] tunnels;

    // kecepatan gerak tunnel / world nya
    [SerializeField] private float tunnelMoveSpeed;

    // panjang 1 tunnel / worldnya
    [SerializeField] float tunnelLength;

    // reference ke index tunnel paling belakang saat ini
    private int indexLastTunnel;

    private void Start() {
        // assign indexnya jadi lengt dari arraynya
        indexLastTunnel = tunnels.Length - 1;
    }

    private void Update() {
        HandleEndlessMovement();
    }

    private void HandleEndlessMovement()
    {
        for (int i = 0; i < tunnels.Length; i++)
        {
            // gerakin tunnelnya pakai transform.Translate
            // kenapa kok gak pakai vector3.lerp atau .smoothdamp?
            // karna gerakan tunnel kan stagnan kaya robot gitu gerakannya, alias konstan
            // kalau lerp itu di awal cepet di akhir melambat / ease out (kamera follow)
            // smoothdamp itu malah keduanya ease in dan ease out (cam 3rd person)

            // kenapa space.world? biar dia itu ngikutin arah belakangnya dunia bukan 
            // dirinya sendiri
            tunnels[i].Translate(Vector3.back * tunnelMoveSpeed * Time.deltaTime, Space.World);
        }

        // cek apakah udah di belakang player
        for (int i = 0; i < tunnels.Length; i++)
        {
            // kalau posisinya udah melebihi panjang dari tunnel itu sendiri, 
            // maka balik ke belakang
            // kenapa kok -tunnelLength? karna kan player di titil 0,0,0 jadi kalau dibelakang
            // ya minus
            if (tunnels[i].position.z < -tunnelLength)
            {
                // ambil posisi paling belakang saat ini
                Vector3 lastTunnelPos = tunnels[indexLastTunnel].position;

                // assign posisinya ke tunnel yang udah lewatin player yaitu tunnel ini
                tunnels[i].position = new Vector3(tunnels[i].position.x, tunnels[i].position.y, lastTunnelPos.z + tunnelLength);

                // update index tunnel paling belakang
                indexLastTunnel = i;
            }
        }
    }
}