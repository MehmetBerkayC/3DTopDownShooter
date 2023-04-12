using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    [SerializeField] float moveSpeed = 5f;
    
    [SerializeField] Crosshairs crosshair;

    Camera viewCamera;
    PlayerController controller;
    GunController gunController;


    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    protected override void Start()
    {
        base.Start();
    }

    void Update()
    {
        // Movement Input
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput * moveSpeed;
        controller.Move(moveVelocity);

        // Look Input
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.up);
        float rayDistance;

        // if the ray intersects with the ground plane
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            Debug.DrawLine(ray.origin, point, Color.green);

            controller.LookAt(point);
            
            //Crosshair
            crosshair.transform.position = point;
            crosshair.DetectTargets(ray, rayDistance);

            if ((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1f)
            {
                gunController.Aim(point);
            }
        }


        // Reload
        if (Input.GetKeyDown(KeyCode.R))
        {
            gunController.Reload();
        }

        // Weapon Input
        if (Input.GetMouseButton(0))
        {
            gunController.OnTriggerHold();
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            gunController.OnTriggerRelease();
        }

    }

    void OnNewWave(int waveNumber)
    {
        health = startingHealth;
        gunController.EquipGun(waveNumber - 1);
    }

    public override void Die()
    {
        AudioManager.instance.PlaySound("Player Death", transform.position);
        base.Die();
    }
}
