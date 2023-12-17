using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIHealth : MonoBehaviour {
    private TextMeshProUGUI _text;
    // Start is called before the first frame update
    void Start() {
        _text = GetComponent<TextMeshProUGUI>();
        FindObjectOfType<PlayerData>().OnHealthChanged.AddListener(UpdateHealthText);
    }
    private void UpdateHealthText(int newHealth) {
        _text.text = string.Format("{0:D3}", newHealth);
    }

}
