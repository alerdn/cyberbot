using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Gun : MonoBehaviour
{
    [SerializeField] private Transform _crossHair;
    [SerializeField] private Transform _shootPosition;
    [SerializeField][Tooltip("Tiros por segundo")] private float _fireRate = 2;
    [SerializeField] private Projectile _projectile;

    private IObjectPool<Projectile> _projectilePool;
    private Coroutine _shootRoutine;

    private void Start()
    {
        _projectilePool = new ObjectPool<Projectile>(
            createFunc: () =>
            {
                Projectile projectile = Instantiate(_projectile, transform);
                projectile.Init(onRelease: (Projectile projectile) => _projectilePool.Release(projectile));

                return projectile;
            }, actionOnGet: (Projectile projectile) =>
            {
                projectile.gameObject.SetActive(true);
                projectile.Launch(_shootPosition.position, transform.rotation);

            }, actionOnRelease: (Projectile projectile) =>
            {
                projectile.gameObject.SetActive(false);
            }, actionOnDestroy: (Projectile projectile) =>
            {
                Destroy(projectile.gameObject);
            }, collectionCheck: true, defaultCapacity: 50, maxSize: 100
        );
    }

    public void Shoot(bool isShooting, EnergyComponent energyComp)
    {
        if (isShooting)
        {
            _shootRoutine ??= StartCoroutine(ShootRoutine(energyComp));
        }
        else
        {
            if (_shootRoutine == null) return;

            StopCoroutine(_shootRoutine);
            _shootRoutine = null;
        }
    }

    private IEnumerator ShootRoutine(EnergyComponent energyComp)
    {
        while (energyComp.UseEnergy())
        {
            _projectilePool.Get();
            yield return new WaitForSeconds(1 / _fireRate);
        }
    }

    public void Aim(Vector2 aimPosition, Vector3 eulerAngles)
    {
        Vector3 crosshairPosition = Camera.main.ScreenToWorldPoint(new Vector3(aimPosition.x, aimPosition.y, -Camera.main.transform.position.z));
        _crossHair.position = crosshairPosition;

        transform.localEulerAngles = eulerAngles;
    }
}
