using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Player : MonoBehaviour, IDamageable
{
    public Transform head;
    public Transform[] firePoints;
    public GameObject projectilePrefab;
    public AudioClip fireSound;
    public PostProcessProfile postProcess;
    private ChromaticAberration chromaticAberration;
    private GravityCharacterController controller;
    private AudioSource audioSource;

    public float health = 100;
    public float maxHealth = 100;

    public Vector3 firePos;

    public float damageTimer = 0;
    private Vector3 inputDir;

    public void ApplyDamage(float damages)
    {
        damageTimer = 0.75f;
    }

    private void Awake()
    {
        controller = GetComponent<GravityCharacterController>();
        audioSource = GetComponent<AudioSource>();

        postProcess.TryGetSettings(out chromaticAberration);
    }

    private void Update()
    {
        chromaticAberration.intensity.value = damageTimer*5 + 0.05f;
        if (damageTimer > 0)
        {
            damageTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        
        controller.Move(inputDir);

        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Plane groundplane = new Plane(transform.up, head.position);
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        groundplane.Raycast(mouseRay, out float hitDist);
        firePos = mouseRay.GetPoint(hitDist);

        head.LookAt(firePos, transform.up);

        inputDir = Vector3.ClampMagnitude(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")), 1);

        if (Input.GetMouseButtonDown(0))
        {
            foreach(Transform firePoint in firePoints)
            {
                GameObject projectile = Instantiate(projectilePrefab);
                projectile.transform.position = firePoint.position;
                projectile.transform.rotation = firePoint.rotation;
            }
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(fireSound, Random.Range(0.95f, 1.05f));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            float timing = MusicManager.beat - 0.15f;
            Debug.Log(timing);
            if(timing > 0.75f || timing < 0.25f)
                controller.AddImpulse(inputDir * 50);
        }
    }
}
