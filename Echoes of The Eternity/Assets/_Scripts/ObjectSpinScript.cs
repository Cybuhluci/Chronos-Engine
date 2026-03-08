using UnityEngine;

public class ObjectSpinScript : MonoBehaviour
{
    [SerializeField] float spinSpeed = 1f;

    [SerializeField] bool clockwiseSpin = true;
    [SerializeField] bool XAxisSpin = false;
    [SerializeField] bool YAxisSpin = false;
    [SerializeField] bool ZAxisSpin = false;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(
            (XAxisSpin ? (clockwiseSpin ? Vector3.right : Vector3.left) : Vector3.zero) * spinSpeed * Time.deltaTime +
            (YAxisSpin ? (clockwiseSpin ? Vector3.up : Vector3.down) : Vector3.zero) * spinSpeed * Time.deltaTime +
            (ZAxisSpin ? (clockwiseSpin ? Vector3.forward : Vector3.back) : Vector3.zero) * spinSpeed * Time.deltaTime
        );
    }
}
