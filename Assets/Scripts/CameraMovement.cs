using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private float smoothness, zoomOutCoef;
    [HideInInspector]
    public Vector3 targetPosition = Vector3.zero;

    private Vector3 velocity = Vector3.zero;

    public void Reset()
    {
        transform.position = targetPosition = Vector3.zero;
    }

    private void Update()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position, targetPosition, ref velocity, smoothness);
    }

    public void ShowTower(Vector3 topPosition)
    {
        float x, y, z;
        x = topPosition.x;
        y = topPosition.y / 2f;
        z = -y * zoomOutCoef;
        targetPosition = new Vector3(x, y, z);
    }
}
