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

        animator.SetBool("isSeeking", sightTimer > 0f);
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
        Vector3 heading = navMeshAgent != null && navMeshAgent.velocity.sqrMagnitude > 0.01f
            ? navMeshAgent.velocity.normalized
            : transform.forward;
        wheel = (heading * wheelDisplacement) + transform.position;

        if (wanderNewPos <= 0 || (transform.position - randPos).magnitude <= 0.5f)
        {
            randPos = new Vector3(Random.Range(-1f, 1f), transform.position.y, Random.Range(-1f, 1f));
            randPos = randPos.normalized * wheelRadious;
            randPos += wheel;
            randPos.y = transform.position.y;
            test = randPos;
            wanderNewPos = wanderCooldown;
        }
        else
        {
            wanderNewPos -= Time.deltaTime;
        }
        Seek(test);
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
