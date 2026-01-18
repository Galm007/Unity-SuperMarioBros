using UnityEngine;

public class EnemyActivator : MonoBehaviour
{
    public Camera cam;

    void Start()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        float rightBound = cam.transform.position.x + cam.orthographicSize * cam.aspect;

        foreach (Transform child in transform)
        {
            if (child.position.x < rightBound)
            {
                child.gameObject.SetActive(true);
            }
        }
    }
}
