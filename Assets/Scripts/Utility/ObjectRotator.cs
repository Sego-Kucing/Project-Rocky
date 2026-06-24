using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    public Vector3 rotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(rotation * 1 * Time.deltaTime);
    }
}
