using UnityEngine;

public class GrappleLauncher : MonoBehaviour
{
    public LineRenderer lr;
    public Rigidbody rb;
    public LayerMask whatIsGrappleable;
    private SpringJoint joint;

    public void LaunchTo(Vector3 point)
    {
        Debug.Log("launchto");
        // 类似 Grappling.cs 中 ExecuteGrapple 逻辑
        Vector3 velocity = CalculateJumpVelocity(transform.position, point, 3f);
        rb.velocity = velocity;
        // 可加摄像机特效等
    }

    public void StartSwing(Vector3 point)
    {
        StopSwing();

        joint = gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = point;

        float distance = Vector3.Distance(transform.position, point);
        joint.maxDistance = distance * 0.8f;
        joint.minDistance = distance * 0.25f;
        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;

        lr.positionCount = 2;
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, point);
    }

    public void StopSwing()
    {
        if (joint != null) Destroy(joint);
        lr.positionCount = 0;
    }

    private Vector3 CalculateJumpVelocity(Vector3 start, Vector3 end, float height)
    {
        float gravity = Physics.gravity.y;
        float displacementY = end.y - start.y;
        Vector3 displacementXZ = new Vector3(end.x - start.x, 0, end.z - start.z);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * height);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * height / gravity) + Mathf.Sqrt(2 * (displacementY - height) / gravity));
        return velocityXZ + velocityY;
    }
}
