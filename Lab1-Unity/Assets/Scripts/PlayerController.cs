using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Transform t;
    private Rigidbody2D rb;
    //private Animator a;
    private Transform art;

    private float horizontal;
    private float vertical;
    private Vector3 zeroVelocity = Vector3.zero;

    [Header("Movement")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float movementSmoothing = 0.5f;

    private void Start()
    {
        t = GetComponent<Transform>();
        rb = GetComponent<Rigidbody2D>();
        //a = GetComponent<Animator>();
        art = t.GetChild(0).GetComponent<Transform>();
    }

    private void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        if (rb.velocity != Vector2.zero)
        {
            float angle = Vector3.Angle(Vector3.right, rb.velocity);
            if (rb.velocity.y < 0)
                angle = 360 - angle;

            art.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            var velocity = new Vector3(horizontal, vertical, 0).normalized * movementSpeed;

            //a.SetFloat("Speed", velocity.magnitude);

            /*if (velocity.x > 0)
                sr.flipX = true;
            if (velocity.x < 0)
                sr.flipX = false;*/

            rb.velocity = Vector3.SmoothDamp(rb.velocity, velocity * movementSpeed * Time.fixedDeltaTime, ref zeroVelocity, movementSmoothing);
        }
        else
            rb.velocity = Vector2.zero;
    }
}
