// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

public class FlockingSystem_Manual : MonoBehaviour
{
    [Header("Basic Settings")]
    public float SpawnArea = 1f;

    [Header("Behaviour Settings")]
    public Vector2 MinMaxSpeed = new(1, 2);
    public Vector2 MinMaxRotationSpeed = new(4.0f, 6.0f);
    [Tooltip("This controls the chance that the system will change direction. It defines the chance in 1000 that this occurs each fixed frame, for example 100 would set a 100 in 1000 chance of changing.")]
    [Range(1, 150)]
    public int DirectionChangeFrequency = 40;
    [Tooltip("How close do the agents have to be to start moving in a group.")]
    public float GroupingDistance = 2.0f;
    [Tooltip("How close a neighbour has to be for them to start avoiding eachother.")]
    public float AvoidanceDistance = 0.2f;

    [Header("Waypoints")]
    [SerializeField]
    public Waypoint[] ManualWaypoints = new Waypoint[1];

    [Header("Agent Types")]
    public ObjectPrefab[] PrefabGroups = new ObjectPrefab[1];

    [Header("Debugging")]
    public bool VisualiseSpawnArea = false;
    public bool VisualiseManualWaypoints = false;

    //Private variables for storing info
    private GameObject[] m_agents;
    private Vector3 m_waypoint;
    private Vector3 m_systemStartPos;
    private int m_waypointIndex;
    private bool m_prefabCheck;
    private float[] m_rotationSpeeds;

    private void Start()
    {
        m_prefabCheck = CheckPrefabExistance();

        if (m_prefabCheck)
        {
            m_systemStartPos = transform.position;
            UpdateTargetPosition();
            SpawnAgents();
        }
        else
        {
            throw new UnityException("Flocking System " + transform.name + " is missing prefab definitions!");
        }
    }

    private void Update()
    {
        if (m_prefabCheck)
        {
            if (Random.Range(0, 10000) < DirectionChangeFrequency)
            {
                UpdateTargetPosition();
            }

            if (Random.Range(0, 5) < 1)
            {
                UpdateMovement();
            }

            for (var i = 0; i < m_agents.Length; i++)
            {
                m_agents[i].transform.Translate(0, 0, Time.deltaTime * Random.Range(MinMaxSpeed.x, MinMaxSpeed.y));
            }
        }
    }

    private void SpawnAgents()
    {
        var totalAgents = 0;

        foreach (var prefab in PrefabGroups)
        {
            if (prefab.Prefab != null)
            {
                totalAgents += prefab.SpawnCount;
            }
        }

        m_agents = new GameObject[totalAgents];
        m_rotationSpeeds = new float[totalAgents];

        var agentIndex = 0;

        for (var i = 0; i < PrefabGroups.Length; ++i)
        {
            if (PrefabGroups[i].Prefab != null)
            {
                for (var j = 0; j < PrefabGroups[i].SpawnCount; ++j)
                {
                    var spawnPoint = new Vector3(
                        Random.Range(-SpawnArea, SpawnArea),
                        Random.Range(-SpawnArea, SpawnArea),
                        Random.Range(-SpawnArea, SpawnArea));

                    var randomScale = Random.Range(PrefabGroups[i].MinMaxScale.x, PrefabGroups[i].MinMaxScale.y);

                    m_agents[agentIndex] = Instantiate(PrefabGroups[i].Prefab, transform.position + spawnPoint, Quaternion.identity);
                    m_agents[agentIndex].transform.name = PrefabGroups[i].PrefabName + "_" + (agentIndex + 1).ToString();
                    m_agents[agentIndex].transform.parent = transform;
                    m_agents[agentIndex].transform.localScale = new Vector3(randomScale, randomScale, randomScale);

                    m_rotationSpeeds[agentIndex] = Random.Range(MinMaxRotationSpeed.x, MinMaxRotationSpeed.y);

                    SetMaterials(i, agentIndex);

                    agentIndex++;
                }
            }
        }
    }

    private void UpdateMovement()
    {
        for (var i = 0; i < m_agents.Length; ++i)
        {
            Vector3 direction;
            var centre = Vector3.zero;
            var avoid = Vector3.zero;

            var groupSize = 0;
            foreach (var obj in m_agents)
            {

                if (obj != m_agents[i])
                {
                    var dist = Vector3.Distance(obj.transform.position, m_agents[i].transform.position);
                    if (dist < GroupingDistance)
                    {
                        centre += obj.transform.position;
                        groupSize++;
                    }
                    if (dist < 1.0f)
                    {
                        avoid += m_agents[i].transform.position - obj.transform.position;
                    }
                }
            }

            if (groupSize > 0)
            {
                centre = centre / groupSize + (m_waypoint - m_agents[i].transform.position);
                direction = centre + avoid - m_agents[i].transform.position;
            }
            else
            {
                direction = m_waypoint + avoid - m_agents[i].transform.position;
            }

            if (direction != Vector3.zero)
            {
                m_agents[i].transform.rotation = Quaternion.Slerp(
                    m_agents[i].transform.rotation,
                    Quaternion.LookRotation(direction),
                    m_rotationSpeeds[i] * Time.deltaTime);
            }
        }
    }

    private void UpdateTargetPosition()
    {
        m_waypoint = ManualWaypoints[m_waypointIndex].GetWaypoint(m_systemStartPos);

        m_waypointIndex = Random.Range(0, ManualWaypoints.Length);
    }

    private bool CheckPrefabExistance()
    {
        if (PrefabGroups == null)
        {
            return false;
        }

        var definedPrefabs = 0;

        for (var i = 0; i < PrefabGroups.Length; i++)
        {
            if (PrefabGroups[i].Prefab != null)
            {
                definedPrefabs += 1;
            }
        }

        return definedPrefabs != 0;
    }

    private void SetMaterials(int prefabIndex, int agentIndex)
    {
        var rend = m_agents[agentIndex].GetComponent<Renderer>();
        var mat = rend.material;

        if (PrefabGroups[prefabIndex].Material != null)
        {
            rend.material = PrefabGroups[prefabIndex].Material;
            mat = rend.material;
        }

        mat.SetFloat("_randomOffset", Random.Range(0f, 1000f));
    }

    private void OnDrawGizmos()
    {
        if (VisualiseSpawnArea)
        {
            Gizmos.color = Color.white * 0.6f;
            Gizmos.DrawWireCube(transform.position, new Vector3(SpawnArea * 2, SpawnArea * 2, SpawnArea * 2));
        }
        if (VisualiseManualWaypoints)
        {
            Gizmos.color = Color.cyan * 0.6f;
            for (var i = 0; i < ManualWaypoints.Length; i++)
            {
                var pos = ManualWaypoints[i].GetWaypoint(transform.position);

                if (pos == m_waypoint)
                {
                    Gizmos.color = Color.red * 0.6f;
                    Gizmos.DrawWireSphere(ManualWaypoints[i].GetWaypoint(transform.position), 1f);
                }
                else
                {
                    Gizmos.color = Color.cyan * 0.6f;
                    Gizmos.DrawWireSphere(ManualWaypoints[i].GetWaypoint(transform.position), 1f);
                }
            }
        }
    }
}

