// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

/// <summary>
/// Prefab data for an agent within the flock
/// </summary>
[System.Serializable]
public class ObjectPrefab
{
    public string prefabName { get { return m_prefabName; } }
    public GameObject prefab { get { return m_prefab; } }
    public int spawnCount { get { return m_count; } }
    public Vector2 minMaxScale { get { return m_scale; } }
    public Material material { get { return m_material; } }

    [SerializeField]
    [Tooltip("The name that the agents of this system will be labelled as in the outliner. This can remain empty but it can help distinguishing different prefabs.")]
    private string m_prefabName;
    [SerializeField]
    private GameObject m_prefab;
    [SerializeField]
    [Range(0f, 30)]
    private int m_count = 5;
    [SerializeField]
    private Vector2 m_scale = new(0.5f, 1f);
    [SerializeField]
    private Material m_material;

    public void SetRandomValues(int count)
    {
        m_count = count;
    }
}

/// <summary>
/// Waypoint data for the flocking system
/// </summary>
[System.Serializable]
public class Waypoint
{
    [Tooltip("A GameObject that exists in the scene for the agents to target. If this is empty this waypoint will use the vector below instead.")]
    public Transform m_waypointObject;

    [Tooltip("A vector realative to the flocking system for the agents to target.")]
    public Vector3 m_waypointPosition;

    public Vector3 GetWaypoint(Vector3 systemPosition)
    {
        return m_waypointObject == null ? m_waypointPosition + systemPosition : m_waypointObject.position;
    }
}