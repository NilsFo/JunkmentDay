using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PowerGun : MonoBehaviour
{
    private PlayerData _playerData;
    private List<Markable> marks;
    private List<StickyFlechette> _flechettes;
    private GameState _gameState;

    [Header("Flechette Gun")] public Transform flechetteProjectileOrigin;
    public GameObject flechetteProjectilePrefab;
    public float flechetteProjectileSpeed = 100f;

    private void Awake()
    {
        _flechettes = new List<StickyFlechette>();
        _gameState = FindObjectOfType<GameState>();

        _playerData = _gameState.player;
        marks = new List<Markable>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        var mouse = Mouse.current;
        var keyboard = Keyboard.current;
        if (mouse == null)
            return;
        if (mouse.leftButton.wasPressedThisFrame)
        {
            ShootFlechetteGun();
        }

        if (mouse.rightButton.wasPressedThisFrame)
        {
            ShootPowerGun();
        }

        if (keyboard.rKey.wasPressedThisFrame)
        {
            ResetMarkers();
        }

        // cleanup flechettes
        List<StickyFlechette> deleteListFlechette = new List<StickyFlechette>();
        foreach (StickyFlechette flechette in _flechettes)
        {
            if (flechette.IsDestroyed())
            {
                deleteListFlechette.Add(flechette);
            }
        }

        foreach (StickyFlechette flechette in deleteListFlechette)
        {
            print("removing a flechette that was deleted");
            _flechettes.Remove(flechette);
        }

        // updating marked state
        List<Markable> deleteListMarkable = new List<Markable>();
        foreach (Markable markable in marks)
        {
            bool stillMarked = false;
            foreach (StickyFlechette flechette in _flechettes)
            {
                if (flechette.myMark == markable)
                {
                    stillMarked = true;
                }
            }

            if (!stillMarked)
            {
                print("unmarking because no more flechettes: " + markable.gameObject.name);
                deleteListMarkable.Add(markable);
            }
        }

        foreach (Markable markable in deleteListMarkable)
        {
            markable.Unmark();
            marks.Remove(markable);
        }
    }

    void ShootPowerGun()
    {
        var camTransform = transform;
        Ray r = new Ray(camTransform.position, camTransform.rotation * Vector3.forward);
        var hit = Physics.Raycast(r, out RaycastHit hitInfo, 10000, LayerMask.GetMask("Default", "Entities", "World"));
        if (hit)
        {
            if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Entities"))
            {
                // hit an entity
                var hitPoint = hitInfo.point;
                Debug.DrawLine(transform.position, hitPoint, Color.red, 1f);
                var powerable = hitInfo.transform.GetComponent<Powerable>();
                if (powerable != null)
                {
                    // hit a powerable
                    Debug.Log("Shot power gun, hit a powerable entity", powerable.gameObject);
                    powerable.Power();
                }
                else
                {
                    Debug.Log("Shot power gun, hit a non-powerable entity", hitInfo.transform.gameObject);
                }
            }

            // hit something else
            Debug.DrawLine(transform.position, hitInfo.point, Color.blue, 1f);
        }
        else
        {
            Debug.Log("Shot power gun but nothing was hit");
        }
    }

    private void ShootFlechetteGun()
    {
        var camTransform = transform;
        var direction = camTransform.rotation * Vector3.forward;

        GameObject projectile = Instantiate(flechetteProjectilePrefab,
            flechetteProjectileOrigin.transform.position, Quaternion.identity);
        projectile.transform.rotation = flechetteProjectileOrigin.transform.rotation;
        projectile.transform.RotateAround(projectile.transform.position, projectile.transform.up, 180f);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.AddForce(direction * flechetteProjectileSpeed, ForceMode.Impulse);
    }

    [Obsolete]
    void ShootMarkerGun()
    {
        var camTransform = transform;
        Ray r = new Ray(camTransform.position, camTransform.rotation * Vector3.forward);
        var hit = Physics.Raycast(r, out RaycastHit hitInfo, 10000, LayerMask.GetMask("Default", "Entities"));
        if (hit)
        {
            if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Entities"))
            {
                // hit an entity
                var hitPoint = hitInfo.point;
                Debug.DrawLine(transform.position, hitPoint, Color.green, 1f);
                var markable = hitInfo.transform.GetComponent<Markable>();
                if (markable != null)
                {
                    // hit a markable
                    Debug.Log("Shot marker gun, hit a markable entity", markable.gameObject);

                    // Creating sticky flechete
                    GameObject flechetteObj =
                        Instantiate(_gameState.stickyFlechettePrefab, hitPoint, Quaternion.identity);
                    flechetteObj.transform.LookAt(_gameState.player.head.transform);
                    flechetteObj.transform.parent = markable.transform;

                    StickyFlechette flechette = flechetteObj.GetComponent<StickyFlechette>();
                    flechette.myMark = markable;
                    _flechettes.Add(flechette);

                    markable.Mark();

                    if (!marks.Contains(markable))
                    {
                        marks.Add(markable);
                    }
                }
                else
                {
                    Debug.Log("Shot marker gun, hit a non-markable entity", hitInfo.transform.gameObject);
                }
            }

            // hit something else
            Debug.DrawLine(transform.position, hitInfo.point, Color.blue, 1f);
        }
        else
        {
            Debug.Log("Shot marker gun but nothing was hit");
        }
    }

    void ResetMarkers()
    {
        foreach (var mark in marks)
        {
            mark.Unmark();
        }

        marks.Clear();

        foreach (StickyFlechette flechette in _flechettes)
        {
            flechette.DestroyFlechette();
        }

        _flechettes.Clear();
    }

    public void ReportFlechetteHit(StickyFlechette flechette)
    {
        _flechettes.Add(flechette);
        var markable = flechette.myMark;

        if (!marks.Contains(markable))
        {
            marks.Add(markable);
        }
    }
}