using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode { Auto, Burst, Single};
    [SerializeField] FireMode fireMode;

    [SerializeField] Transform[] projectileSpawns;
    [SerializeField] Projectile projectile;

    [SerializeField] float msBetweenShots = 100f; //in milliseconds
    [SerializeField] float muzzleVelocity = 35f;
    [SerializeField] int burstCount;

    [SerializeField] int magazineSize;
    [SerializeField] float reloadTime = 0.3f; // in seconds

    [Header("Effects")]  // Effects
    [SerializeField] Transform shell;
    [SerializeField] Vector3 shellScale;
    [SerializeField] Transform shellEjectionPoint;

    [SerializeField] AudioClip audioShoot;
    [SerializeField] AudioClip audioReload;

    [Header("Recoil")]  // Recoil
    [SerializeField] float recoilMovementSettleTime = 0.1f;
    [SerializeField] float recoilRotationSettleTime = 0.1f;

    [SerializeField] Vector2 recoilKickMinMax = new Vector2(0.05f, 0.2f);
    [SerializeField] Vector2 recoilAngleMinMax = new Vector2(3,5);

    float nextShotTime;
    int remainingBurst;
    int projectilesInMagazine;

    MuzzleFlash muzzleFlash;

    bool triggerReleased;
    bool isReloading;

    float recoilAngle;

    Vector3 recoilSmoothDampVelocity;
    float recoilRotationSmoothDampVelocity;


    private void Start()
    {
        muzzleFlash = GetComponentInChildren<MuzzleFlash>();

        if(fireMode == FireMode.Burst)
        {
            remainingBurst = burstCount;
        }

        projectilesInMagazine = magazineSize;

        shell.transform.localScale = shellScale; // check if working right
    }

    private void Update()
    {   // animating recoil
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilMovementSettleTime);
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotationSmoothDampVelocity, recoilRotationSettleTime);
        transform.localEulerAngles += Vector3.left * recoilAngle;

        if (!isReloading && projectilesInMagazine == 0)
        {
            Reload();
        }
    }

    void Shoot()
    {
        if(!isReloading && Time.time > nextShotTime && projectilesInMagazine > 0)
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
                if(projectilesInMagazine == 0)
                {
                    Reload();
                    break;
                }
                projectilesInMagazine--;

                nextShotTime = Time.time + msBetweenShots / 1000;
                Projectile newProjectile = Instantiate(projectile, projectileSpawns[i].position, projectileSpawns[i].rotation) as Projectile;
                newProjectile.SetSpeed(muzzleVelocity);
            }
            
            Instantiate(shell, shellEjectionPoint.position, shellEjectionPoint.rotation);
            muzzleFlash.Activate();

            transform.localPosition -= Vector3.forward * Random.Range(recoilKickMinMax.x, recoilKickMinMax.y);
            recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

            AudioManager.instance.PlaySound(audioShoot, transform.position);
        }
    }

    public void Reload()
    {
        if(!isReloading && projectilesInMagazine != magazineSize)
        {
            StartCoroutine(AnimationReload());
            AudioManager.instance.PlaySound(audioReload, transform.position);
        }
    }

    IEnumerator AnimationReload()
    {   
        isReloading = true;
        yield return new WaitForSeconds(0.2f);

        float reloadSpeed = 1 / reloadTime;
        float percentage = 0;

        Vector3 initialRotation = transform.localEulerAngles;
        float maxReloadAngle = 30f;

        while (percentage < 1f)
        {
            percentage += Time.deltaTime * reloadSpeed;

            float interpolation = (-Mathf.Pow(percentage, 2) + percentage) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRotation + Vector3.left * reloadAngle;

            yield return null;
        }

        isReloading = false;
        projectilesInMagazine = magazineSize;
    }

    public void Aim(Vector3 aimPoint)
    {
        if (!isReloading)
        {
            transform.LookAt(aimPoint);
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
