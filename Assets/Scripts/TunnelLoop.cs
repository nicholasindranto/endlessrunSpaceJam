using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TunnelLoop : MonoBehaviour
{
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
