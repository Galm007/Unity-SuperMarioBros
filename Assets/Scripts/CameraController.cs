using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public GameObject player;
    public Flagpole flagpole;
    public float followSoftThreshold = 0.34f;
    public float followHardThreshold = 0.45f;
    public float followSoftSpeed = 5.0f;
    public float followHardThresholdAtFlagpole = 0.55f;

    Camera cam;
    Rigidbody2D playerRb;
    
    float targetX;

    void Start()
    {
        cam = GetComponent<Camera>();
        playerRb = player.GetComponent<Rigidbody2D>();

        targetX = transform.position.x;
        flagpole.OnFlagpoleTouched += _ => {
            followSoftThreshold = 1.0f;
            followHardThreshold = followHardThresholdAtFlagpole;
        };
    }

    void FixedUpdate()
    {
        float cameraWidth = 2.0f * cam.orthographicSize * cam.aspect;
        float leftBound = transform.position.x - cameraWidth / 2.0f;
        float softThresholdX = leftBound + followSoftThreshold * cameraWidth;
        float hardThresholdX = leftBound + followHardThreshold * cameraWidth;

        if (player.transform.position.x > hardThresholdX)
        {
            targetX = transform.position.x + (player.transform.position.x - hardThresholdX);
        }
        else if (player.transform.position.x > softThresholdX && playerRb.velocity.x > 0.0f)
        {
            targetX += followSoftSpeed * Time.deltaTime;
        }

        transform.position = new Vector3(
            targetX,
            transform.position.y,
            transform.position.z
        );
    }
}
