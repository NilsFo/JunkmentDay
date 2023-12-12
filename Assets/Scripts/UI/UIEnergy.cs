using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIEnergy : MonoBehaviour
{
    private TextMeshProUGUI _text;
    // Start is called before the first frame update
    void Start() {
        _text = GetComponent<TextMeshProUGUI>();
        FindObjectOfType<PlayerData>().OnEnergyChanged.AddListener(newEnergy => _text.text = newEnergy.ToString());
    }

}
