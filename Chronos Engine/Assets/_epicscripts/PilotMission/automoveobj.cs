using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class automoveobj : MonoBehaviour
{
    [SerializeField] GameObject obj;

    [SerializeField] Vector3 position1;
    [SerializeField] Vector3 position2;

    [SerializeField] float moveSpeed = 1f; // Speed of movement

    private float lerpTime = 0f; // Tracks how far along the movement is (0 to 1)
    private bool movingTowardsPosition2 = true; // Keeps track of direction

    void Update()
    {
        // Update lerpTime based on movement direction
        if (movingTowardsPosition2)
        {
            lerpTime += Time.deltaTime * moveSpeed;
        }
        else
        {
            lerpTime -= Time.deltaTime * moveSpeed;
        }

        // Clamp lerpTime between 0 and 1
        lerpTime = Mathf.Clamp01(lerpTime);

        // Move object smoothly between position1 and position2 using Slerp
        obj.transform.position = Vector3.Slerp(position1, position2, lerpTime);

        // When the object reaches position2 or position1, reverse the direction
        if (lerpTime >= 1f || lerpTime <= 0f)
        {
            movingTowardsPosition2 = !movingTowardsPosition2;
        }
    }
}
