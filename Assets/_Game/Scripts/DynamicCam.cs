using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCam : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private GameObject camTopDown;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CamTrigger"))
        {
            camTopDown.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CamTrigger"))
        {
            camTopDown.SetActive(false);
        }
    }
}
