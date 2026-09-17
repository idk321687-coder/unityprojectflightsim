using UnityEngine;

public class Spinner : MonoBehaviour
{
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 100f, 0f);
    [SerializeField] private Space rotationSpace = Space.Self;

    void Update()
    {
        // Smoothly rotate the object over time
        transform.Rotate(rotationSpeed * Time.deltaTime, rotationSpace);
    }
}
