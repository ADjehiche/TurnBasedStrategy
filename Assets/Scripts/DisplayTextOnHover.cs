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
        if (textBox != null)
        {
            textBox.text = displayText;
        }
    }

    void OnMouseExit()
    {
        if (textBox != null)
        {
            textBox.text = "";
        }
    }
    
    void OnDestroy()
    {
        if (textBox != null)
        {
            textBox.text = "";
        }
    }
}
