using UnityEngine;

public class GlobeCamera : MonoBehaviour
{
    public float moveSpeed = 1000f;
    public float rotateSpeed = 100f;
    public float zoomSpeed = 5000f;

    void Update()
    {
        // WASD movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 movement =
            transform.right * x +
            transform.forward * z;

        transform.position += movement * moveSpeed * Time.deltaTime;

        // Mouse look
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            transform.Rotate(
                -mouseY * rotateSpeed * Time.deltaTime,
                mouseX * rotateSpeed * Time.deltaTime,
                0f,
                Space.Self
            );
        }

        // Scroll wheel zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        transform.position +=
            transform.forward *
            scroll *
            zoomSpeed;
    }
}