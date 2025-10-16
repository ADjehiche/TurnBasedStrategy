using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayTextOnHover : MonoBehaviour
{
    public string displayText;
    public TextMeshProUGUI textBox;

    void OnMouseOver()
    {
        textBox.text = displayText;
    }

    void OnMouseExit()
    {
        textBox.text = "";
    }
}
