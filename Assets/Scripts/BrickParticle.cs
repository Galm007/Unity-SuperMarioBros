using UnityEngine;

public class BrickParticle : MonoBehaviour
{
    public Vector3 velocity = Vector2.up;
    public float fallSpeed = 10.0f;
    public float rotationSpeed = 2.0f;
    public float deleteAtYThreshold = -200.0f;

    void Update()
    {
        velocity.y -= fallSpeed * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        transform.eulerAngles += rotationSpeed * Time.deltaTime * Vector3.forward;

        if (transform.position.y < deleteAtYThreshold)
        {
            if (transform.parent.childCount <= 2)
            {
                Destroy(transform.parent.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
