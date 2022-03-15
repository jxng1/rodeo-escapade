using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Vector3 moveDelta;

    private BoxCollider2D boxCollider2D;
    public Vector3 defaultPlayerScale;

    [Range(5, 10)]
    public int moveSpeed;


    // Start is called before the first frame update
    void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // Calculates sprite facing vector.
        moveDelta = new Vector3(x, y, 0);

        // Flips player sprite depending on direction.
        if (moveDelta.x > 0) {
            transform.localScale = defaultPlayerScale;
        } else if (moveDelta.x < 0) {
            // Flip player in x-axis.
            transform.localScale = new Vector3(-defaultPlayerScale.x, defaultPlayerScale.y, defaultPlayerScale.z);
        }

        transform.Translate(moveDelta * Time.deltaTime * moveSpeed);
    }
}
