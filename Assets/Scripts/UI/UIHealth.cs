using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIHealth : MonoBehaviour {
    private TextMeshProUGUI _text;
    // Start is called before the first frame update
    void Start() {
        _text = GetComponent<TextMeshProUGUI>();
        FindObjectOfType<PlayerData>().OnHealthChanged.AddListener(newHealth => _text.text = newHealth.ToString());
    }

}
