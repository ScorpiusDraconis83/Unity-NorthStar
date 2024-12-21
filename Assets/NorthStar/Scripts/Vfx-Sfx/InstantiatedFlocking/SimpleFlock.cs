// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

public class FlockingSystem_Simple : MonoBehaviour
{
    [Header("Basic Settings")]
    public Vector3 spawnArea = new(1f, 1f, 1f);
    [Tooltip("The area within which random waypoints will be generated.")]
    public Vector3 traversalArea = new(2f, 2f, 2f);

    [Header("Behaviour Settings")]
    public Vector2 minMaxSpeed = new(1, 2);
    public float variationSpeed = 0.1f;
    public float variationMagnitude = 1f;
    public Vector2 minMaxRotationSpeed = new(4.0f, 6.0f);
    public bool flockAroundOrigin;
    [Tooltip("This controls the chance that the system will change direction. A value of 50 would set a 50 in 10000 chance of changing on each fixed frame.")]
    [Range(1, 200)]
    public int directionChangeFrequency = 15;
    [Tooltip("How close do the agents have to be to start moving in a group.")]
    public float groupingDistance = 2.0f;
    [Tooltip("How close a neighbour has to be for them to start avoiding eachother.")]
    public float avoidanceDistance = 0.2f;

    [Header("Agent Types")]
    public ObjectPrefab[] prefabGroups = new ObjectPrefab[1];

    [Header("Fish Only")]
    public float animationSpeed = 6f;

    [Header("Debugging")]
    public bool visualiseSpawnArea = false;
    public bool visualiseTraversalArea = false;
    public bool visualiseWaypoint = false;

    //Private variables for storing info
    private GameObject[] m_agents;
    private Vector3 m_waypoint;
    private Vector3 m_systemStartPos;
    private int m_waypointIndex;
    private bool m_prefabCheck;
    private float[] m_rotationSpeeds;
    private float m_speedXCord;
    private float m_speedYCord;
    private float m_globalSpeed;
    private float[] m_agentSpeed;
    private float m_speedSine;

    private void Start()
    {
        m_prefabCheck = CheckPrefabExistance();

        if (m_prefabCheck)
        {
            m_systemStartPos = transform.position;
            if (flockAroundOrigin)
            {
                m_waypoint = m_systemStartPos;
            }
            else
            {
                NewRandomTarget();
            }

            SpawnAgents();
        }
        else
        {
            throw new UnityException("Flocking System " + transform.name + " is missing prefab definitions!");
        }
    }

    private void Update()
    {
        if (m_prefabCheck == true)
        {
            if (!flockAroundOrigin)
            {
                if (Random.Range(0, 10000) < directionChangeFrequency)
                {
                    NewRandomTarget();
                }
            }
            else
            {
                m_waypoint = m_systemStartPos;
            }

            if (Random.Range(0, 5) < 1)
            {
                UpdateMovement();
            }


            m_globalSpeed = Mathf.PerlinNoise(m_speedXCord + variationSpeed * Time.time, m_speedYCord + variationSpeed * Time.time);

            m_speedSine = Time.time + Time.deltaTime * ((m_globalSpeed + 1) * variationMagnitude);

            for (var i = 0; i < m_agents.Length; i++)
            {
                var newspeed = m_agentSpeed[i] * (m_globalSpeed * variationMagnitude + 1);
                m_agents[i].transform.Translate(0, 0, Time.deltaTime * newspeed);

                var rend = m_agents[i].GetComponent<Renderer>();
                var mat = rend.material;

                m_speedSine += Time.deltaTime * (m_globalSpeed * variationMagnitude + 2);

                mat.SetFloat("_SpeedMultiplierDontTouch", m_speedSine);
            }
        }
    }

    private void SpawnAgents()
    {
        var totalAgents = 0;

        foreach (var prefab in prefabGroups)
        {
            if (prefab.prefab != null)
            {
                totalAgents += prefab.spawnCount;
            }
        }

        m_rotationSpeeds = new float[totalAgents];
        m_agentSpeed = new float[totalAgents];
        m_speedXCord = Random.Range(1, 10);
        m_speedYCord = Random.Range(1, 10);
        m_speedSine = 0;

        m_agents = new GameObject[totalAgents];

        var agentIndex = 0;

        for (var i = 0; i < prefabGroups.Length; ++i)
        {
            if (prefabGroups[i].prefab != null)
            {
                for (var j = 0; j < prefabGroups[i].spawnCount; ++j)
                {
                    var spawnPoint = new Vector3(
                        Random.Range(-spawnArea.x, spawnArea.x),
                        Random.Range(-spawnArea.y, spawnArea.y),
                        Random.Range(-spawnArea.z, spawnArea.z));

                    var randomScale = Random.Range(prefabGroups[i].minMaxScale.x, prefabGroups[i].minMaxScale.y);

                    m_agentSpeed[agentIndex] = Random.Range(minMaxSpeed.x, minMaxSpeed.y);

                    m_agents[agentIndex] = Instantiate(prefabGroups[i].prefab, transform.position + spawnPoint, Quaternion.identity);
                    m_agents[agentIndex].transform.name = prefabGroups[i].prefabName + "_" + (agentIndex + 1).ToString();
                    m_agents[agentIndex].transform.parent = transform;
                    m_agents[agentIndex].transform.localScale = new Vector3(randomScale, randomScale, randomScale);

                    SetMaterials(i, agentIndex);

                    m_rotationSpeeds[agentIndex] = Random.Range(minMaxRotationSpeed.x, minMaxRotationSpeed.y);

                    agentIndex++;
                }
            }
        }
    }

    private void UpdateMovement()
    {
        for (var i = 0; i < m_agents.Length; ++i)
        {
            var centre = Vector3.zero;
            var avoid = Vector3.zero;

            var groupSize = 0;
            foreach (var obj in m_agents)
            {

                if (obj != m_agents[i])
                {
                    var dist = Vector3.Distance(obj.transform.position, m_agents[i].transform.position);
                    if (dist < groupingDistance)
                    {
                        centre += obj.transform.position;
                        groupSize++;
                    }
                    if (dist < avoidanceDistance)
                    {
                        avoid += m_agents[i].transform.position - obj.transform.position;
                    }
                }
            }

            Vector3 direction;
            if (groupSize > 0)
            {
                centre = centre / groupSize + (m_waypoint - m_agents[i].transform.position);
                direction = centre + avoid - m_agents[i].transform.position;
            }
            else
            {
                direction = m_waypoint - m_agents[i].transform.position;
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

    public void NewRandomTarget()
    {
        var newGoal = new Vector3(
                    Random.Range(-traversalArea.x, traversalArea.x),
                    Random.Range(-traversalArea.y, traversalArea.y),
                    Random.Range(-traversalArea.z, traversalArea.z));
        m_waypoint = newGoal + m_systemStartPos;
    }

    private bool CheckPrefabExistance()
    {
        if (prefabGroups == null)
        {
            return false;
        }

        var definedPrefabs = 0;

        for (var i = 0; i < prefabGroups.Length; i++)
        {
            if (prefabGroups[i].prefab != null)
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

        if (prefabGroups[prefabIndex].material != null)
        {
            rend.material = prefabGroups[prefabIndex].material;
            mat = rend.material;
        }

        mat.SetFloat("_randomOffsetDontTouch", Random.Range(0f, 100f));
        mat.SetFloat("_SwimSpeed", animationSpeed);
    }

    private void OnDrawGizmos()
    {
        if (visualiseSpawnArea)
        {
            Gizmos.color = Color.white * 0.6f;
            Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea.x * 2, spawnArea.y * 2, spawnArea.z * 2));
        }
        if (visualiseTraversalArea)
        {
            Gizmos.color = Color.green * 0.6f;
            Gizmos.DrawWireCube(transform.position, new Vector3(traversalArea.x * 2, traversalArea.y * 2, traversalArea.z * 2));
        }
        if (visualiseWaypoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_waypoint, 0.1f);
        }
    }
}
