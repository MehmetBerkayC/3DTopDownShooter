using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] Transform muzzle;
    [SerializeField] Projectile projectile;

    [SerializeField] Transform shell;
    [SerializeField] Transform shellEjectionPoint;

    [SerializeField] float msBetweenShots = 100f; //in milliseconds
    [SerializeField] float muzzleVelocity = 35f;

    float nextShotTime;

    MuzzleFlash muzzleFlash;

    private void Start()
    {
        muzzleFlash = GetComponentInChildren<MuzzleFlash>();
    }

    public void Shoot()
    {
        if(Time.time > nextShotTime)
        {
            nextShotTime = Time.time + msBetweenShots / 1000;
            Projectile newProjectile = Instantiate(projectile, muzzle.position, muzzle.rotation) as Projectile;
            newProjectile.SetSpeed(muzzleVelocity);

            Instantiate(shell, shellEjectionPoint.position, shellEjectionPoint.rotation);
            muzzleFlash.Activate();
        }
    }
}
