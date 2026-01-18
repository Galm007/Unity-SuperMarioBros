using System;
using UnityEngine;

public class Flagpole : MonoBehaviour
{
    public Transform flag;
    public float flagDropHeight = 7.8f;
    public float flagDropSpeed = 8.0f;

    public event Action<float> OnFlagpoleTouched;

    float flagRestY;
    float flagTargetY;

    void Start()
    {
        flagRestY = flagTargetY = flag.position.y;
    }

    void Update()
    {
        flag.position = new Vector2(
            flag.position.x,
            Mathf.MoveTowards(flag.position.y, flagTargetY, flagDropSpeed * Time.deltaTime)
        );
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnFlagpoleTouched?.Invoke(flagDropSpeed);
            flagTargetY = flagRestY - flagDropHeight;
        }
    }
}
