using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.Rendering.Universal;

public class EnemyController : MonoBehaviour
{
    private enum ChaseBehaviour { ChasePlayer, IsChasedByPlayer };
    private enum IdleBehaviour { StayStill, MoveRandomly };
    private enum WhereToRun { OppositeDirection, ToClosestObstacle };

    private Transform t;
    private Rigidbody2D rb;
    private Seeker seeker;
    //private Animator a;
    private Transform art;
    private Light2D light;

    [Header("Movement")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float movementSmoothing = 0.5f;

    private Vector2 direction;
    private Vector3 zeroVelocity = Vector3.zero;
    private Transform playerTransform;

    [Header("Behaviour")]
    [SerializeField] private ChaseBehaviour chaseOrChased = ChaseBehaviour.ChasePlayer;
    [SerializeField] private WhereToRun whereToRun = WhereToRun.OppositeDirection;
    [SerializeField] private bool triggerIfClose = true;
    [SerializeField] [Range(0, 10f)] private float triggerDistance = 2f;
    [SerializeField] private bool triggerWithCone = false;
    [SerializeField] private float coneLength = 3f;
    [SerializeField] [Range(0, 180f)] private float coneAngle = 90f;
    [SerializeField] private IdleBehaviour idleBehaviour = IdleBehaviour.StayStill;
    [SerializeField] [Range(0, 10f)] private float randomMovementDistance = 5f;
    [SerializeField] private bool panicSpeed = false;
    [SerializeField] [Range(-3f, 3f)] private float panicSpeedMultiplier = 0f;

    private float nextWaypointDistance = .5f;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private Path path;
    private Vector3 target;
    private bool triggered = false;

    private void Start()
    {
        t = GetComponent<Transform>();
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
        //a = GetComponent<Animator>();
        art = t.GetChild(0).GetComponent<Transform>();
        light = art.GetChild(0).GetComponent<Light2D>();

        light.pointLightOuterRadius = coneLength;
        light.pointLightOuterAngle = coneAngle;

        playerTransform = GameObject.Find("Player").GetComponent<Transform>();

        if (chaseOrChased == ChaseBehaviour.ChasePlayer)
            target = playerTransform.position;
        else
            target = t.position;

        ChooseDestination();
        InvokeRepeating("UpdatePath", 0f, .5f);
    }

    private void Update()
    {
        ChooseDestination();

        if (path != null && currentWaypoint < path.vectorPath.Count)
            direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;

        if (rb.velocity != Vector2.zero)
        {
            float angle = Vector3.Angle(Vector3.right, rb.velocity);
            if (rb.velocity.y < 0)
                angle = 360 - angle;

            art.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (triggered)
            light.color = Color.red;
        else
            light.color = Color.white;
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

        if (canMove)
        {
            var velocity = direction * movementSpeed;

            if (panicSpeed && triggered)
                velocity *= panicSpeedMultiplier;

            rb.velocity = Vector3.SmoothDamp(rb.velocity, velocity * movementSpeed * Time.fixedDeltaTime, ref zeroVelocity, movementSmoothing);
        }

        float distance = Vector2.Distance(t.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
            currentWaypoint++;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (chaseOrChased == ChaseBehaviour.IsChasedByPlayer)
                Destroy(this.gameObject);
            else
                Debug.Log("I got you");
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
            seeker.StartPath(rb.position, target, OnPathComplete);
    }

    private void ChooseDestination()
    {
        triggered = false;

        if (triggerIfClose && (playerTransform.position - t.position).magnitude < triggerDistance)
            triggered = true;

        if (!triggerWithCone || PlayerInConesight())
            triggered = true;

        if (!triggerIfClose && !triggerWithCone)
            triggered = true;


        if (triggered)
            if (chaseOrChased == ChaseBehaviour.ChasePlayer)
                target = playerTransform.position;
            else
                switch (whereToRun)
                {
                    case WhereToRun.OppositeDirection:
                        target = ChooseFarDestination();
                        break;
                    case WhereToRun.ToClosestObstacle:
                        if (reachedEndOfPath)
                            target = ChooseCloseObstacle();
                        break;
                }
        else
            switch (idleBehaviour)
            {
                case IdleBehaviour.StayStill:
                    if (chaseOrChased == ChaseBehaviour.ChasePlayer || reachedEndOfPath)
                        target = t.position;
                    break;
                case IdleBehaviour.MoveRandomly:
                    if (chaseOrChased == ChaseBehaviour.ChasePlayer || reachedEndOfPath)
                        target = ChooseRandomDestination();
                    break;
            }
    }

    private bool PlayerInConesight()
    {
        Collider2D player;
        LayerMask playerMask = LayerMask.GetMask("Player");
        Vector2 front = art.right;

        player = Physics2D.OverlapCircle(t.position, coneLength, playerMask);

        if (player == null)
            return false;

        Vector2 dirToPlaywer = (player.transform.position - t.position).normalized;
        return Vector2.Angle(front, dirToPlaywer) < coneAngle / 2;
    }

    private Vector2 ChooseCloseObstacle()
    {
        Collider2D obstacle = null;
        Vector2 destination = t.position;
        LayerMask wallMask = LayerMask.GetMask("Wall");

        for (float f = .1f; f < 10f; f += .1f)
        {
            obstacle = Physics2D.OverlapCircle(t.position, f, wallMask);

            if (obstacle != null)
                break;            
        }

        for (int i = 0; i < 10; i++)
        {
            Vector2 dir = (obstacle.transform.position - t.position) + (t.position - playerTransform.position) * 2;
            destination = (Vector2)t.position + dir;

            destination += new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)).normalized;

            if (IsReachable(destination))
                break;
        }

        return destination;
    }

    private Vector2 ChooseFarDestination()
    {
        Vector2 destination = t.position;

        for (int i = 0; i < 20; i++)
        {
            Vector2 dir = (t.position - playerTransform.position).normalized;
            destination = (Vector2)t.position + dir * randomMovementDistance;

            destination += new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)).normalized;

            if (IsReachable(destination))
                break;
        }

        return destination;
    }

    private Vector2 ChooseRandomDestination()
    {
        Vector2 destination = t.position;

        for (int i = 0; i < 20; i++)
        {
            Vector2 dir = new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)).normalized;
            destination = (Vector2)t.position + dir * randomMovementDistance;

            if (IsReachable(destination))
                break;
        } 

        return destination;
    }

    private bool IsReachable(Vector2 position)
    {
        LayerMask wallMask = LayerMask.GetMask("Wall");
        LayerMask groundMask = LayerMask.GetMask("Ground");

        Collider2D hitGround = Physics2D.OverlapCircle(position, .1f, groundMask);
        Collider2D hitWall = Physics2D.OverlapCircle(position, .1f, wallMask);

        return hitGround != null && hitWall == null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
        Gizmos.DrawLine(transform.position, transform.position + transform.GetChild(0).right * coneLength);
    }
}
