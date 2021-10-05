using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{

    public float damages = 10;
    public LayerMask entityLayers;

    private void OnTriggerEnter(Collider other)
    {
        if (entityLayers.value == (entityLayers.value | 1 << other.gameObject.layer))
        {
            other.gameObject.GetComponent<IDamageable>().ApplyDamage(damages);
        }
    }
}
