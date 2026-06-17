using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SectionTrigger : MonoBehaviour
{
   public GameObject prefabSection;
   
   private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Trigger"))
        {
            Instantiate(prefabSection, new Vector3(0, 0, -57), Quaternion.identity);
        }
    }
}
