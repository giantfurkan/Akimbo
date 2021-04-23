using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    Transform player;
    [SerializeField]
    float cameraCatchUpDistance = 2f;
    [SerializeField]
    float cameraSpeed = 0.01f;

    Vector3? cameraDestination;

    private void Update()
    {
        if (player == null)
            player = GameManager.Clone.transform;

        if (Mathf.Abs(player.transform.position.z - transform.position.z) > cameraCatchUpDistance)
            cameraDestination = new Vector3(transform.position.x, transform.position.y, player.transform.position.z);
        if (cameraDestination != null && Mathf.Abs(player.transform.position.z - transform.position.z) < cameraCatchUpDistance / 2)
            cameraDestination = null;
        if (cameraDestination != null)
            transform.position = Vector3.Lerp(transform.position, cameraDestination.Value, cameraSpeed);
    }

}
