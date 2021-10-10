using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BeatStomper : Enemy
{
    public GameObject explosionPrefab;
    public Transform damageZone;
    public AudioClip deathClip;
    public float health = 40;


    public override void ApplyDamage(float damages)
    {
        health -= damages;
        if (health <= 0)
            Explode();
    }

    // Start is called before the first frame update
    void Start()
    {
        MusicManager.Instance.OnBeat.AddListener(OnBeat);
        damageZone.localScale = Vector3.zero;
    }

    private void OnDestroy()
    {
        MusicManager.Instance.OnBeat.RemoveListener(OnBeat);
    }

    private void OnBeat()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float size = (1-(MusicManager.beat* MusicManager.beat))*3;
        damageZone.localScale = new Vector3(size, size, 0.1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            other.GetComponent<IDamageable>()?.ApplyDamage(5);
            Explode();
        }
    }

    private void Explode()
    {
        Instantiate(explosionPrefab, transform.position, transform.rotation);
        AudioSource.PlayClipAtPoint(deathClip, transform.position);
        Destroy(gameObject);
    }
}
