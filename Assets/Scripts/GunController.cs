using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform weaponHold;

    public Gun startingWeapon;

    Gun equippedGun;


    private void Start()
    {
        if(startingWeapon != null)
        {
            EquipGun(startingWeapon);
        }    
    }

    public void EquipGun(Gun gunToEquip)
    {
        if(equippedGun != null)
        {
            Destroy(equippedGun.gameObject);
        }
        equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation);
        equippedGun.transform.parent = weaponHold;

    }

    public void OnTriggerHold()
    {
        if(equippedGun != null)
        {
            equippedGun.OnTriggerHold();
        }
    }

    public void OnTriggerRelease()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerReleased();
        }
    }

    public void Reload()
    {
        if(equippedGun != null)
        {
            equippedGun.Reload();
        }
    }

    public void Aim(Vector3 aimPoint)
    {
        if(equippedGun != null)
        {
            equippedGun.Aim(aimPoint);
        }
    }
    public float GunHeight
    {
        get
        {
            return weaponHold.position.y;
        }
    }
}
