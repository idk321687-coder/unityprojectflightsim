using UnityEngine;

public class SpinCube : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(60f, 50f, 60f);
    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
