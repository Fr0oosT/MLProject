using UnityEngine;
using UnityEngine.InputSystem;

public class Shooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;

    private InputAction attackAction;

    void Start()
    {
        var playerInput = FindFirstObjectByType<PlayerInput>();
        attackAction = playerInput.actions["Attack"];
    }

    void Update()
    {
        if(attackAction.triggered)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position + firePoint.forward * 0.5f, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = firePoint.forward * bulletSpeed;
    }
}