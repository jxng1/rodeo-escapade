using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform target;

    [Range(0f, 1f)]
    public float cameraDelay;

    private Vector3 cameraPos;
    private Vector3 velocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        cameraPos = new Vector3(target.position.x, target.position.y, -1);
        
        transform.position = Vector3.SmoothDamp(gameObject.transform.position, cameraPos, ref velocity, cameraDelay);
    }
}
