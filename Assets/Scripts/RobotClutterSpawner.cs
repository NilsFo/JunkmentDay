using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RobotClutterSpawner : MonoBehaviour
{
    [Header("Setup")] public GameObject scrapPrefab;
    public BoxCollider originCollider;
    private GameState _gameState;

    private List<GameObject> _myScrap;

    [Header("Config")] public int clutterCountMin;
    public int clutterCountMax;

    private void Awake()
    {
        _gameState = FindObjectOfType<GameState>();
        _myScrap = new List<GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        int spawnCount = Random.Range(clutterCountMin, clutterCountMax);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = RandomPointInBounds(originCollider);
            GameObject scrap = Instantiate(scrapPrefab, transform);
            scrap.transform.position = pos;
            scrap.SetActive(false);
            _myScrap.Add(scrap);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SpawnClutter()
    {
        Debug.Log("Spawning clutter");
        foreach (GameObject scrap in _myScrap)
        {
            scrap.SetActive(true);
            scrap.transform.rotation = Random.rotation;
            scrap.transform.parent = null;

            Rigidbody rb = scrap.GetComponent<Rigidbody>();
            Vector3 direction = rb.transform.position - transform.position;
            direction = direction.normalized;

            rb.AddRelativeTorque(new Vector3(
                Random.Range(-20f, 20f),
                Random.Range(-20f, 20f),
                0
            ));

            float force = Random.Range(3f, 8f);
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }

    public static Vector3 RandomPointInBounds(BoxCollider collider)
    {
        // Vector3 point = new Vector3(
        //     Random.Range(collider.bounds.min.x, collider.bounds.max.x),
        //     Random.Range(collider.bounds.min.y, collider.bounds.max.y),
        //     Random.Range(collider.bounds.min.z, collider.bounds.max.z)
        // );

        // if (point != collider.ClosestPoint(point))
        // {
        //     Debug.Log("Out of the collider! Looking for other point...");
        //     point = GetRandomPointInVolume();
        // }

        Vector3 point = collider.transform.position;
        point.x = point.x + Random.Range(-0.5f, 0.5f);
        point.y = point.y + Random.Range(-0.5f, 0.5f);
        point.z = point.z + Random.Range(-0.5f, 0.5f);

        return collider.bounds.ClosestPoint(point);
    }
}