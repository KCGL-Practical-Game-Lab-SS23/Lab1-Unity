using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private enum WhereToGo { AwayFromPlayer, RandomDirection, RandomDestination, ToFarthestDestination, ToClosestObstacle };

    private Transform t;
    private Rigidbody2D rb;
    //private Animator a;
    //private SpriteRenderer sr;

    private Vector2 direction;
    private Vector3 zeroVelocity = Vector3.zero;
    private Transform playerTransform;
    private bool playerIsClose = false;

    [Header("Movement")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float movementSmoothing = 0.5f;

    [Header("Behaviour")]
    [SerializeField] private bool moveOnlyIfPlayerIsClose = true;
    [SerializeField] [Range(0, 10f)] private float triggerPlayerDistance = 2f;
    [SerializeField] private WhereToGo whereToGo = WhereToGo.AwayFromPlayer;
    //[SerializeField] private bool chasePlayer = false;
    [SerializeField] private bool changeSpeedWhenCloseToPlayer = false;
    [SerializeField] [Range(-3f, 3f)] private float changedSpeedMultiplier = 0f;



    private void Start()
    {
        t = GetComponent<Transform>();
        rb = GetComponent<Rigidbody2D>();
        //a = GetComponent<Animator>();
        //sr = GetComponent<SpriteRenderer>();

        playerTransform = GameObject.Find("Player").GetComponent<Transform>();
    }

    private void Update()
    {
        switch (whereToGo)
        {
            case WhereToGo.AwayFromPlayer:
                direction = t.position - playerTransform.position;
                break;
            case WhereToGo.RandomDirection:
                direction = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1));
                break;
            case WhereToGo.RandomDestination:
                //To Do
                break;
            case WhereToGo.ToFarthestDestination:
                //To Do
                break;
            case WhereToGo.ToClosestObstacle:
                //To Do
                break;
        }

        playerIsClose = (playerTransform.position - t.transform.position).magnitude <= triggerPlayerDistance;
                
        if (moveOnlyIfPlayerIsClose && !playerIsClose)
            direction = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            var velocity = direction.normalized * movementSpeed;

            //a.SetFloat("Speed", velocity.magnitude);

            /*if (velocity.x > 0)
                sr.flipX = true;
            if (velocity.x < 0)
                sr.flipX = false;*/

            if (changeSpeedWhenCloseToPlayer)
                velocity *= changedSpeedMultiplier;

            rb.velocity = Vector3.SmoothDamp(rb.velocity, velocity * movementSpeed * Time.fixedDeltaTime, ref zeroVelocity, movementSmoothing);
        }
        else
            rb.velocity = Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //Add score
            Destroy(this.gameObject);
        }

    }
}
