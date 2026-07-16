#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public static class TempGhoulDiag
{
    [MenuItem("Tools/Diagnose Ghoul")]
    public static void Diagnose()
    {
        var z = Object.FindFirstObjectByType<ZombieAnimController>(FindObjectsInactive.Include);
        if (z == null) { Debug.LogError("GHOUL|No hay ZombieAnimController en la escena"); return; }

        var so = new SerializedObject(z);
        Debug.Log("GHOUL|obj=" + z.gameObject.name + " layer=" + z.gameObject.layer +
                  " | frontR=" + so.FindProperty("frontPerceptionRadious").floatValue +
                  " backR=" + so.FindProperty("backPerceptionRadoius").floatValue +
                  " sidesR=" + so.FindProperty("sidesPerceptionRadious").floatValue +
                  " | playerLayerMask=" + so.FindProperty("playerLayer").intValue +
                  " | attackRange=" + z.attackRange +
                  " | maxSpeed=" + so.FindProperty("maxSpeed").floatValue +
                  " maxForce=" + so.FindProperty("maxForce").floatValue);

        var agent = z.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            Debug.Log("GHOUL|NavMeshAgent: speed=" + agent.speed + " updatePosition=" + agent.updatePosition +
                      " updateRotation=" + agent.updateRotation + " enabled=" + agent.enabled +
                      " | onNavMesh(editor)=" + agent.isOnNavMesh);
        }
        var rb = z.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log("GHOUL|Rigidbody: isKinematic=" + rb.isKinematic + " useGravity=" + rb.useGravity +
                      " constraints=" + rb.constraints);
        }

        // NavMesh baked?
        var tri = NavMesh.CalculateTriangulation();
        Debug.Log("GHOUL|NavMesh: vertices=" + tri.vertices.Length + " (0 = sin navmesh horneado)");

        // Player side
        var rig = GameObject.Find("VR Rig");
        if (rig != null)
        {
            var cc = rig.GetComponent<CharacterController>();
            Debug.Log("GHOUL|VR Rig: layer=" + rig.layer + " (" + LayerMask.LayerToName(rig.layer) + ")" +
                      " | CharacterController=" + (cc != null) +
                      " | tag=" + rig.tag);
        }

        // Attack colliders state
        var left = so.FindProperty("attackColliderLeft").objectReferenceValue as SphereCollider;
        var right = so.FindProperty("attackColliderRight").objectReferenceValue as SphereCollider;
        Debug.Log("GHOUL|attackColliders: left=" + (left ? left.name + " trigger=" + left.isTrigger + " enabled=" + left.enabled : "NULL") +
                  " | right=" + (right ? right.name + " trigger=" + right.isTrigger + " enabled=" + right.enabled : "NULL"));
    }
}
#endif
