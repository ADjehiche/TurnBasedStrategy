using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(DeckManager))]
public class DeckManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DeckManager deckManager = (DeckManager)target;
        if (GUILayout.Button("Draw Next Card"))
        {
            HandManager handManager = Object.FindFirstObjectByType <HandManager>();
            if (handManager != null){
                var card = deckManager.DrawOne();
                if (card != null)
                {
                    handManager.AddCardToHand(card);
                }
            }
        }
    }
}
#endif