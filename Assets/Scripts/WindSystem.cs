using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WindSystem : MonoBehaviour
{
    public static event Action<Vector2> OnWindChanged;

    private Vector2[] direccionesValidas  = new Vector2[]
    {
        Vector2.up,    // Norte
        Vector2.down,  // Sur
        Vector2.left,  // Oeste
        Vector2.right,  // Este
        new Vector2(1, 1).normalized,   // Noreste
        new Vector2(-1, 1).normalized,  // Noroeste
        new Vector2(1, -1).normalized,  // Sureste
        new Vector2(-1, -1).normalized // Suroeste
    }; 

    void Start()
    {
        Vector2 nuevaDireccion = direccionesValidas[UnityEngine.Random.Range(0, direccionesValidas.Length)];
        OnWindChanged?.Invoke(nuevaDireccion);
        StartCoroutine(ChangeWind());
    }

   IEnumerator ChangeWind()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(10f, 20f));
            Vector2 nuevaDireccion = direccionesValidas[UnityEngine.Random.Range(0, direccionesValidas.Length)];
            OnWindChanged?.Invoke(nuevaDireccion);
        }
    }

}
