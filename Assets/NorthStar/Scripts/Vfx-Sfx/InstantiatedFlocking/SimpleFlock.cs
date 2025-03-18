// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

public class FlockingSystem_Simple : MonoBehaviour
{
    [Header("Basic Settings")]
    public Vector3 SpawnArea = new(1f, 1f, 1f);
    [Tooltip("The area within which random waypoints will be generated.")]
    public Vector3 TraversalArea = new(2f, 2f, 2f);

    [Header("Behaviour Settings")]
    public Vector2 MinMaxSpeed = new(1, 2);
    public float VariationSpeed = 0.1f;
    public float VariationMagnitude = 1f;
    public Vector2 MinMaxRotationSpeed = new(4.0f, 6.0f);
    public bool FlockAroundOrigin;
    [Tooltip("This controls the chance that the system will change direction. A value of 50 would set a 50 in 10000 chance of changing on each fixed frame.")]
    [Range(1, 200)]
    public int DirectionChangeFrequency = 15;
    [Tooltip("How close do the agents have to be to start moving in a group.")]
    public float GroupingDistance = 2.0f;
    [Tooltip("How close a neighbour has to be for them to start avoiding eachother.")]
    public float AvoidanceDistance = 0.2f;

    [Header("Agent Types")]
    public ObjectPrefab[] PrefabGroups = new ObjectPrefab[1];

    [Header("Fish Only")]
    public float AnimationSpeed = 6f;

    [Header("Debugging")]
    public bool VisualiseSpawnArea = false;
    public bool VisualiseTraversalArea = false;
    public bool VisualiseWaypoint = false;

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
            if (FlockAroundOrigin)
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
        if (m_prefabCheck)
        {
            if (!FlockAroundOrigin)
            {
                if (Random.Range(0, 10000) < DirectionChangeFrequency)
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


            m_globalSpeed = Mathf.PerlinNoise(m_speedXCord + VariationSpeed * Time.time, m_speedYCord + VariationSpeed * Time.time);

            m_speedSine = Time.time + Time.deltaTime * ((m_globalSpeed + 1) * VariationMagnitude);

            for (var i = 0; i < m_agents.Length; i++)
            {
                var newspeed = m_agentSpeed[i] * (m_globalSpeed * VariationMagnitude + 1);
                m_agents[i].transform.Translate(0, 0, Time.deltaTime * newspeed);

                var rend = m_agents[i].GetComponent<Renderer>();
                var mat = rend.material;

                m_speedSine += Time.deltaTime * (m_globalSpeed * VariationMagnitude + 2);

                mat.SetFloat("_SpeedMultiplierDontTouch", m_speedSine);
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

        m_rotationSpeeds = new float[totalAgents];
        m_agentSpeed = new float[totalAgents];
        m_speedXCord = Random.Range(1, 10);
        m_speedYCord = Random.Range(1, 10);
        m_speedSine = 0;

        m_agents = new GameObject[totalAgents];

        var agentIndex = 0;

        for (var i = 0; i < PrefabGroups.Length; ++i)
        {
            if (PrefabGroups[i].Prefab != null)
            {
                for (var j = 0; j < PrefabGroups[i].SpawnCount; ++j)
                {
                    var spawnPoint = new Vector3(
                        Random.Range(-SpawnArea.x, SpawnArea.x),
                        Random.Range(-SpawnArea.y, SpawnArea.y),
                        Random.Range(-SpawnArea.z, SpawnArea.z));

                    var randomScale = Random.Range(PrefabGroups[i].MinMaxScale.x, PrefabGroups[i].MinMaxScale.y);

                    m_agentSpeed[agentIndex] = Random.Range(MinMaxSpeed.x, MinMaxSpeed.y);

                    m_agents[agentIndex] = Instantiate(PrefabGroups[i].Prefab, transform.position + spawnPoint, Quaternion.identity);
                    m_agents[agentIndex].transform.name = PrefabGroups[i].PrefabName + "_" + (agentIndex + 1).ToString();
                    m_agents[agentIndex].transform.parent = transform;
                    m_agents[agentIndex].transform.localScale = new Vector3(randomScale, randomScale, randomScale);

                    SetMaterials(i, agentIndex);

                    m_rotationSpeeds[agentIndex] = Random.Range(MinMaxRotationSpeed.x, MinMaxRotationSpeed.y);

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
                    if (dist < GroupingDistance)
                    {
                        centre += obj.transform.position;
                        groupSize++;
                    }
                    if (dist < AvoidanceDistance)
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
                    Random.Range(-TraversalArea.x, TraversalArea.x),
                    Random.Range(-TraversalArea.y, TraversalArea.y),
                    Random.Range(-TraversalArea.z, TraversalArea.z));
        m_waypoint = newGoal + m_systemStartPos;
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

        mat.SetFloat("_randomOffsetDontTouch", Random.Range(0f, 100f));
        mat.SetFloat("_SwimSpeed", AnimationSpeed);
    }

    private void OnDrawGizmos()
    {
        if (VisualiseSpawnArea)
        {
            Gizmos.color = Color.white * 0.6f;
            Gizmos.DrawWireCube(transform.position, new Vector3(SpawnArea.x * 2, SpawnArea.y * 2, SpawnArea.z * 2));
        }
        if (VisualiseTraversalArea)
        {
            Gizmos.color = Color.green * 0.6f;
            Gizmos.DrawWireCube(transform.position, new Vector3(TraversalArea.x * 2, TraversalArea.y * 2, TraversalArea.z * 2));
        }
        if (VisualiseWaypoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_waypoint, 0.1f);
        }
    }
}
