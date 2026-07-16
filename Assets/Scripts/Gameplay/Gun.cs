using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float speed;
    public GameObject bullet;
    public Transform Barrel;
    //public AudioSource audioSource;
    //public AudioClip audioClip;

    public void Fire()
    {
        GameObject spawnedBullet = Instantiate(bullet, Barrel.position, Barrel.rotation);
        spawnedBullet.GetComponent<Rigidbody>().linearVelocity = speed * Barrel.forward;
        //audioSource.PlayOneShot(audioClip);
        Destroy(spawnedBullet, 2);
    }
}
