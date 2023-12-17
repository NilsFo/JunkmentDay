using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = System.Numerics.Vector3;

public class ClutterScrapCollapser : MonoBehaviour
{
    public List<GameObject> scrapStates;
    private int _collapsedState = 0;
    private GameState _gameState;

    public List<AudioClip> bumpSounds;
    public LayerMask bumpLayerMask;
    public float bumpAudioVolume = 0.5f;

    private void Awake()
    {
        foreach (GameObject scrap in scrapStates)
        {
            scrap.SetActive(false);
        }

        _gameState = FindObjectOfType<GameState>();
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject scrap in scrapStates)
        {
            scrap.SetActive(false);
        }

        _collapsedState = Random.Range(0, scrapStates.Count);
        scrapStates[_collapsedState].SetActive(true);

        transform.rotation = Random.rotation;
    }

    private void OnEnable()
    {
        scrapStates[_collapsedState].SetActive(true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((bumpLayerMask.value & (1 << collision.transform.gameObject.layer)) > 0) {
            _gameState.musicManager.CreateAudioClip(
                bumpSounds[Random.Range(0, bumpSounds.Count)],
                transform.position,
                pitchRange: 0.1f,
                soundInstanceVolumeMult: bumpAudioVolume,
                respectBinning: true
            );
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}