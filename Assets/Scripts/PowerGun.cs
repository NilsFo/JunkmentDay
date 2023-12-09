using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PowerGun : MonoBehaviour {
    private PlayerData _playerData;
    private List<Markable> marks;
    // Start is called before the first frame update
    void Start() {
        _playerData = FindObjectOfType<PlayerData>();
    }

    // Update is called once per frame
    void Update() {
        var mouse = Mouse.current;
        var keyboard = Keyboard.current;
        if (mouse == null)
            return;
        if (mouse.leftButton.wasPressedThisFrame) {
            ShootMarkerGun();
        }
        if (mouse.rightButton.wasPressedThisFrame) {
            ShootPowerGun();
        }
        if (keyboard.rKey.wasPressedThisFrame) {
            ResetMarkers();
        }
    }

    void ShootPowerGun() {
        var camTransform = transform;
        Ray r = new Ray(camTransform.position, camTransform.rotation * Vector3.forward);
        var hit = Physics.Raycast(r, out RaycastHit hitInfo, 10000, LayerMask.GetMask("Default", "Entities"));
        if (hit) {
            if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Entities")) {
                // hit an entity
                Debug.DrawLine(transform.position, hitInfo.point, Color.red, 1f);
                var powerable = hitInfo.transform.GetComponent<Powerable>();
                if (powerable != null) {
                    // hit a powerable
                    Debug.Log("Shot power gun, hit a powerable entity", powerable.gameObject);
                    powerable.Power();
                } else {
                    Debug.Log("Shot power gun, hit a non-powerable entity", hitInfo.transform.gameObject);
                }
            }
            // hit something else
            Debug.DrawLine(transform.position, hitInfo.point, Color.blue, 1f);
        } else {
            Debug.Log("Shot power gun but nothing was hit");
        }
    }

    void ShootMarkerGun() {
        var camTransform = transform;
        Ray r = new Ray(camTransform.position, camTransform.rotation * Vector3.forward);
        var hit = Physics.Raycast(r, out RaycastHit hitInfo, 10000, LayerMask.GetMask("Default", "Entities"));
        if (hit) {
            if (hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Entities")) {
                // hit an entity
                Debug.DrawLine(transform.position, hitInfo.point, Color.green, 1f);
                var markable = hitInfo.transform.GetComponent<Markable>();
                if (markable != null) {
                    // hit a markable
                    Debug.Log("Shot marker gun, hit a markable entity", markable.gameObject);
                    markable.Mark();
                    marks.Add(markable);
                } else {
                    Debug.Log("Shot marker gun, hit a non-markable entity", hitInfo.transform.gameObject);
                }
            }
            // hit something else
            Debug.DrawLine(transform.position, hitInfo.point, Color.blue, 1f);
        } else {
            Debug.Log("Shot marker gun but nothing was hit");
        }
    }

    void ResetMarkers() {
        foreach (var mark in marks) {
            mark.Unmark();
        }
        marks.Clear();
    }
}
