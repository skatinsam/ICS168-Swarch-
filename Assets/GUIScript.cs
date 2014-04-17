using UnityEngine;
using System.Collections;

public class GUIScript : MonoBehaviour {

    public string stringToEdit = "Hello World";

    void OnGUI()
    {
        stringToEdit = GUI.TextField(new Rect(10, 10, 200, 20), stringToEdit, 25);
    }
}
