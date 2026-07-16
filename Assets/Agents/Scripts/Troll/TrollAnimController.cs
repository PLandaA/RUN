using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrollAnimController : MonoBehaviour
{
    #region//SerializedField
    [SerializeField] Transform frontPerception, arrivePoint;
    [SerializeField] float arriveRadious, frontPerceptionRadious, wheelRadious, wheelDisplacement, wanderCooldown, maxSpeed, maxForce, rotationSpeed;
    [SerializeField] LayerMask playerLayer;
    #endregion
    #region//Member
    bool isPlayerInPerception;
    Animator animator;
    Rigidbody rigidBody;
    private Vector3 desired_Vel, steering, wheel, randPos, test;
    #endregion
    #region//Public
    public float wanderNewPos;
    public Graph<Vector3> pathFindGraph;
    [HideInInspector] Transform target;
    #endregion
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (PlayerInVision())
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }
    void LookAtFixed(Vector3 target)
    {
        if (target != Vector3.zero)
        {
            Quaternion toRotate = Quaternion.LookRotation(target);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotate, rotationSpeed * Time.deltaTime);
        }

    }
    public bool PlayerInVision()
    {
        isPlayerInPerception = Physics.CheckSphere(frontPerception.position, frontPerceptionRadious, playerLayer);
        return isPlayerInPerception;
    }
    public GameObject getPlayerInVision()
    {
        Collider[] colliders = Physics.OverlapSphere(frontPerception.position, frontPerceptionRadious, playerLayer);
        foreach (Collider hit in colliders)
        {
            target = hit.gameObject.transform;
        }
        return target != null ? target.gameObject : null;
    }
    public void Flee(Vector3 threat)
    {
        // True flee: the desired velocity points AWAY from the threat.
        desired_Vel.x = transform.position.x - threat.x;
        desired_Vel.z = transform.position.z - threat.z;
        desired_Vel = desired_Vel.normalized * maxSpeed;
        steering = desired_Vel - rigidBody.linearVelocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);
        steering = steering / rigidBody.mass;
        rigidBody.linearVelocity = Vector3.ClampMagnitude(steering + rigidBody.linearVelocity, maxSpeed);
        LookAtFixed(desired_Vel);
        Debug.DrawRay(transform.position, rigidBody.linearVelocity.normalized * 2, Color.green);
        Debug.DrawRay(transform.position, desired_Vel.normalized * 2, Color.magenta);
    }
    public void Wander()
    {
        wheel = (rigidBody.linearVelocity.normalized * wheelDisplacement) + transform.position;
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
            //Seek(test);
            wanderNewPos -= Time.deltaTime;
        }
        Seek(test);
    }
    public void Seek(Vector3 target)
    {
        // Rigidbody velocity is the single mover; level collisions contain the agent.
        desired_Vel.x = target.x - transform.position.x;
        desired_Vel.z = target.z - transform.position.z;
        desired_Vel = desired_Vel.normalized * maxSpeed;
        steering = desired_Vel - rigidBody.linearVelocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);
        steering = steering / rigidBody.mass;
        rigidBody.linearVelocity = Vector3.ClampMagnitude(steering + rigidBody.linearVelocity, maxSpeed);
        LookAtFixed(desired_Vel);
        Debug.DrawRay(transform.position, rigidBody.linearVelocity.normalized * 2, Color.green);
        Debug.DrawRay(transform.position, desired_Vel.normalized * 2, Color.magenta);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(frontPerception.transform.position, frontPerceptionRadious);
    }
}
