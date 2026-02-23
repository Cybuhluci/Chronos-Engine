using UnityEngine;

public class DieAfterSeconds : MonoBehaviour
{
    public float seconds = 10f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= seconds)
        {
            Destroy(gameObject);
        }
    }
}
