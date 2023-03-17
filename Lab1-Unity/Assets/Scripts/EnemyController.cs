using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyController : MonoBehaviour
{
    private enum ChaseBehaviour { ChasePlayer, IsChasedByPlayer };
    private enum IdleBehaviour { StayStill, MoveRandomly };
    private enum WhereToRun { ToRandomDestination, ToFarthestDestination, ToClosestObstacle };

    private Transform t;
    private Rigidbody2D rb;
    private Seeker seeker;
    //private Animator a;
    //private SpriteRenderer sr;

    [Header("Movement")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float movementSmoothing = 0.5f;

    private Vector2 direction;
    private Vector3 zeroVelocity = Vector3.zero;
    private Transform playerTransform;

    [Header("Behaviour")]
    [SerializeField] private ChaseBehaviour chaseOrChased = ChaseBehaviour.ChasePlayer;
    [SerializeField] private WhereToRun whereToRun = WhereToRun.ToFarthestDestination;
    [SerializeField] private bool triggerIfClose = true;
    [SerializeField] [Range(0, 10f)] private float triggerDistance = 2f;
    [SerializeField] private IdleBehaviour idleBehaviour = IdleBehaviour.StayStill;
    [SerializeField] private bool panicSpeed = false;
    [SerializeField] [Range(-3f, 3f)] private float panicSpeedMultiplier = 0f;

    private float nextWaypointDistance = 3f;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private Path path;
    private Transform target;
    
    



    private void Start()
    {
        t = GetComponent<Transform>();
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
        //a = GetComponent<Animator>();
        //sr = GetComponent<SpriteRenderer>();


        playerTransform = GameObject.Find("Player").GetComponent<Transform>();

        if (chaseOrChased == ChaseBehaviour.ChasePlayer)
            target = playerTransform;

        ChooseDestination();
        InvokeRepeating("UpdatePath", 0f, .5f);
    }

    private void Update()
    {
        ChooseDestination();
    }

    private void FixedUpdate()
    {
        if (path == null)
            return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * movementSpeed * Time.fixedDeltaTime;

        rb.AddForce(force);

        /*if (canMove)
        {
            var velocity = direction * movementSpeed;

            if (panicSpeed)
                velocity *= panicSpeedMultiplier;

            rb.velocity = Vector3.SmoothDamp(rb.velocity, velocity * movementSpeed * Time.fixedDeltaTime, ref zeroVelocity, movementSmoothing);
        }*/

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
            currentWaypoint++;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //Add score
            Destroy(this.gameObject);
        }

    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }            
    }

    private void UpdatePath()
    {
        if (seeker.IsDone())
            seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    private void ChooseDestination()
    {
        if (chaseOrChased == ChaseBehaviour.ChasePlayer)
            target = playerTransform;
        else
            switch (whereToRun)
            {
                case WhereToRun.ToRandomDestination:
                    break;
                case WhereToRun.ToFarthestDestination:
                    break;
                case WhereToRun.ToClosestObstacle:
                    break;
            }

        if (triggerIfClose && (playerTransform.position - t.position).magnitude > triggerDistance)
            switch (idleBehaviour)
            {
                case IdleBehaviour.StayStill:
                    target = t;
                    break;
                case IdleBehaviour.MoveRandomly:
                    //Choose random destination
                    break;
            }
    }
}
