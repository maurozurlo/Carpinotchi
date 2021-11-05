using System.Text;
using UnityEngine;
using UnityEngine.AI;
public class Wander : MonoBehaviour
{
    NavMeshAgent m_NavAgent;
    public GameObject floor;
    public GameObject marker;
    Animator animator;

    public bool enablePathFollow;

    // Start is called before the first frame update
    void Start()
    {
        m_NavAgent = this.GetComponent<NavMeshAgent>();
        m_NavAgent.SetDestination(getRandomPointInMesh());

        InvokeRepeating("changeDestination", 0, 5);
    }


    void changeDestination()
    {
        Vector3 target = getRandomPointInMesh();
        if (pathComplete())
            m_NavAgent.SetDestination(target);

        animator = GetComponentInChildren<Animator>();
    }

    protected bool pathComplete()
    {
        if (m_NavAgent.remainingDistance <= m_NavAgent.stoppingDistance)
        {

            if (!m_NavAgent.hasPath || Mathf.Abs(m_NavAgent.velocity.sqrMagnitude) < float.Epsilon)

                return true;

        }
        return false;
    }



    private void Update()
    {
        if (enablePathFollow)
            PathFollow();

        float velocity = m_NavAgent.velocity.magnitude / m_NavAgent.speed;
        animator.SetFloat("speed", velocity);
    }

    void PathFollow()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();

            if (Physics.Raycast(ray, out hit))
            {
                m_NavAgent.SetDestination(hit.point);
            }
        }
    }

    Vector3 getRandomPointInMesh()
    {
        float sizeX = floor.GetComponent<MeshFilter>().mesh.bounds.size.x * floor.transform.localScale.x;
        float sizeZ = floor.GetComponent<MeshFilter>().mesh.bounds.size.z * floor.transform.localScale.z;

        float startingPointX = floor.transform.position.x + (sizeX / 2);
        float startingPointZ = floor.transform.position.z + (sizeZ / 2);

        float endingPointX = floor.transform.position.x - (sizeX / 2);
        float endingPointZ = floor.transform.position.z - (sizeZ / 2);

        float xCoord = Random.Range(startingPointX, endingPointX);
        float zCoord = Random.Range(startingPointZ, endingPointZ);

        /*        GameObject startObj = Instantiate(marker, new Vector3(startingPointX, 0, startingPointZ), Quaternion.identity) as GameObject;
                startObj.name = "Starting point";
                GameObject endObj = Instantiate(marker, new Vector3(endingPointX, 0, endingPointZ), Quaternion.identity) as GameObject;
                endObj.name = "Ending point";*/

        return new Vector3(xCoord, 0, zCoord);
    }

}

public class Puts
{
    public Puts(params object[] args)
    {
        var sb = new StringBuilder();
        foreach (var arg in args)
            sb.Append(arg).Append(" ");
        Debug.Log(sb.ToString());
    }
}