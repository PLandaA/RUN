using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class ZombieAnimController : MonoBehaviour
{
    #region//SerializedField
    [SerializeField] Transform frontPerception, backPerception, leftPerception, rightPerception, arrivePoint;
    [SerializeField] float arriveRadious, frontPerceptionRadious, backPerceptionRadoius, sidesPerceptionRadious, wheelRadious, wheelDisplacement,wanderCooldown,maxSpeed,maxForce,rotationSpeed ;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] SphereCollider attackColliderRight, attackColliderLeft;
    [SerializeField] GameObject leftArm, rightArm;
    #endregion
    #region//Member
    bool isPlayerInPerception;
    float sightTimer;
    Animator animator;
    Rigidbody rigidBody;
    private Vector3 desired_Vel, steering, wheel,randPos,test;
    private NavMeshAgent navMeshAgent;
    #endregion
    #region//Public
    public float wanderNewPos, attackRange;
    [Tooltip("Seconds the ghoul keeps hunting after losing sight of the player.")]
    public float sightMemorySeconds = 6f;
    [Tooltip("Movement speed while patrolling (slow, stalking).")]
    public float patrolSpeed = 1.4f;
    [Tooltip("Movement speed while actively chasing the player.")]
    public float chaseSpeed = 3.8f;

    public Graph<Vector3> pathFindGraph;
    [HideInInspector] Transform target;
    #endregion
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();

        // Robustness: snap the agent onto the baked NavMesh if it starts slightly off it.
        if (navMeshAgent != null && !navMeshAgent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                navMeshAgent.Warp(hit.position);
            }
        }
    }
private void FixedUpdate()
    {
        // Sight memory keeps the hunt alive briefly after losing the player.
        if (PlayerInVision())
        {
            sightTimer = sightMemorySeconds;
        }
        else
        {
            sightTimer -= Time.fixedDeltaTime;
        }

        bool hunting = sightTimer > 0f;
        animator.SetBool("isSeeking", hunting);

        // The missing link: nothing ever drove the NavMeshAgent. Now the brain
        // runs every physics tick — chase the player while hunting, wander otherwise.
        if (navMeshAgent == null || !navMeshAgent.isOnNavMesh)
        {
            return;
        }

        // Slow, stalking patrol vs. faster chase. Speed switches with the state.
        navMeshAgent.speed = hunting ? chaseSpeed : patrolSpeed;

        if (hunting)
        {
            GameObject player = getPlayerInVision();
            Vector3 destination = player != null ? player.transform.position
                                 : (target != null ? target.position : transform.position);
            Seek(destination);
        }
        else
        {
            Wander();
        }
    }
    void Update()
    {

        getLocalCoordinates(attackColliderLeft, leftArm.transform);
        getLocalCoordinates(attackColliderRight, rightArm.transform);
    }
    void getLocalCoordinates(SphereCollider collider,Transform localPos)
    {
        Vector3 colliderToLocalPos = transform.InverseTransformPoint(localPos.position);
        collider.center = colliderToLocalPos;
    }
    void LookAtFixed(Vector3 target)
    {
        if (target != Vector3.zero)
        {
            Quaternion toRotate = Quaternion.LookRotation(target);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotate, rotationSpeed * Time.deltaTime);
        }
       
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(frontPerception.position, frontPerceptionRadious);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(backPerception.position, backPerceptionRadoius);
        Gizmos.DrawWireSphere(rightPerception.position, sidesPerceptionRadious);
        Gizmos.DrawWireSphere(leftPerception.position, sidesPerceptionRadious);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(wheel, wheelRadious);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(test, 1);
    }
    public bool PlayerInVision()
    {
        // 360-degree perception: all four sensors participate in detection.
        isPlayerInPerception =
            Physics.CheckSphere(frontPerception.position, frontPerceptionRadious, playerLayer) ||
            Physics.CheckSphere(backPerception.position, backPerceptionRadoius, playerLayer) ||
            Physics.CheckSphere(leftPerception.position, sidesPerceptionRadious, playerLayer) ||
            Physics.CheckSphere(rightPerception.position, sidesPerceptionRadious, playerLayer);
        return isPlayerInPerception;
    }
    public GameObject getPlayerInVision()
    {
        // The last valid target is kept as fallback (last known position).
        TryFindPlayer(frontPerception.position, frontPerceptionRadious);
        TryFindPlayer(backPerception.position, backPerceptionRadoius);
        TryFindPlayer(leftPerception.position, sidesPerceptionRadious);
        TryFindPlayer(rightPerception.position, sidesPerceptionRadious);
        return target != null ? target.gameObject : null;
    }

    private void TryFindPlayer(Vector3 center, float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(center, radius, playerLayer);
        foreach (Collider hit in colliders)
        {
            target = hit.gameObject.transform;
        }
    }
    public void Seek(Vector3 target)
    {
        // NavMeshAgent is the single movement authority; steering vector kept for debug rays.
        desired_Vel.x = target.x - transform.position.x;
        desired_Vel.z = target.z - transform.position.z;
        desired_Vel = desired_Vel.normalized * maxSpeed;

        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(target);
        }

        Debug.DrawRay(transform.position, desired_Vel.normalized * 2, Color.magenta);
    }
public void Wander()
    {
        if (navMeshAgent == null || !navMeshAgent.isOnNavMesh) return;

        // Map-wide patrol: pick a fresh random point on the baked NavMesh and walk to
        // it; when we arrive (or can't reach it), choose another. This roams the whole
        // level instead of loitering in a small circle like the old local wander.
        bool needNewGoal =
            !navMeshAgent.hasPath ||
            (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.5f) ||
            navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial;

        if (needNewGoal)
        {
            Vector3 goal;
            if (TryGetRandomNavMeshPoint(out goal))
            {
                test = goal; // reuse existing debug gizmo field
                navMeshAgent.SetDestination(goal);
            }
        }
    }

    // Samples a random point anywhere on the baked NavMesh. Grabs a random NavMesh
    // triangle vertex as a seed, then snaps to the nearest walkable position.
    private bool TryGetRandomNavMeshPoint(out Vector3 result)
    {
        NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
        result = transform.position;
        if (tri.vertices == null || tri.vertices.Length == 0) return false;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector3 seed = tri.vertices[Random.Range(0, tri.vertices.Length)];
            if (NavMesh.SamplePosition(seed, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                // Prefer a point that isn't right on top of us, so it actually travels.
                if (Vector3.Distance(hit.position, transform.position) > 3f)
                {
                    result = hit.position;
                    return true;
                }
            }
        }
        return false;
    }
    public void ActivateLeftCollider()
    {
        attackColliderLeft.enabled = true;
    }
    public void ActivateRightCollider()
    {
        attackColliderRight.enabled = true;
    }
    public void DeactivateLeftCollider()
    {
        attackColliderLeft.enabled = false;
    }
    public void DeactivateRightCollider()
    {
        attackColliderRight.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        // Attack colliders (animation-event driven) live on this GameObject.
        if (((1 << other.gameObject.layer) & playerLayer.value) != 0)
        {
            PlayerCaught();
        }
    }

    private void PlayerCaught()
    {
        // Caught by the ghoul: restart the level for an instant retry loop.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
