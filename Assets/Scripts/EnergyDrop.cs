using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyDrop : MonoBehaviour
{
    private void Start()
    {
        transform.eulerAngles = Vector3.forward * Random.Range(0f, 360f);
    }

    private void Update()
    {
        if (Vector2.Distance(transform.position, PlayerController.Instance.transform.position) < 1f)
        {
            PlayerController.Instance.EnergyComp.RestoreEnergy(5);
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        transform.position = Vector2.Lerp(transform.position, PlayerController.Instance.transform.position, 5 * Time.fixedDeltaTime);
    }
}
