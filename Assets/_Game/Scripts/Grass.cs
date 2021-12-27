using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    [SerializeField] private ParticleSystem hitFx;

    private bool isCut;
    void GetHit(int amount)
    {
        if (!isCut)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
            hitFx.Emit(10);
            isCut = true;
        }

    }
}
