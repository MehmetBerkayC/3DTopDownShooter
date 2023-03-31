using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode { Auto, Burst, Single};
    [SerializeField] FireMode fireMode;

    [SerializeField] Transform[] projectileSpawns;
    [SerializeField] Projectile projectile;

    [SerializeField] int burstCount;

    [SerializeField] Transform shell;
    [SerializeField] Transform shellEjectionPoint;

    [SerializeField] float msBetweenShots = 100f; //in milliseconds
    [SerializeField] float muzzleVelocity = 35f;

    float nextShotTime;

    int remainingBurst;

    MuzzleFlash muzzleFlash;

    bool triggerReleased;

    private void Start()
    {
        muzzleFlash = GetComponentInChildren<MuzzleFlash>();

        if(fireMode == FireMode.Burst)
        {
            remainingBurst = burstCount;
        }
    }

    void Shoot()
    {
        if(Time.time > nextShotTime)
        {
            if (fireMode == FireMode.Burst)
            {
                if (remainingBurst == 0)
                {
                    return;
                }
                remainingBurst--;
            }
            else if (fireMode == FireMode.Single) 
            {
                if (!triggerReleased)
                {
                    return;
                }
            }

            for (int i = 0; i < projectileSpawns.Length; i++)
            {
                nextShotTime = Time.time + msBetweenShots / 1000;
                Projectile newProjectile = Instantiate(projectile, projectileSpawns[i].position, projectileSpawns[i].rotation) as Projectile;
                newProjectile.SetSpeed(muzzleVelocity);
            }
            
            Instantiate(shell, shellEjectionPoint.position, shellEjectionPoint.rotation);
            muzzleFlash.Activate();
        }
    }

    public void OnTriggerHold()
    {
        Shoot();
        triggerReleased = false;
    }

    public void OnTriggerReleased()
    {
        remainingBurst = burstCount;
        triggerReleased = true;
    }
}
