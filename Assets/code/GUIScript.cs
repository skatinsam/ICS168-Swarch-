using UnityEngine;
using System.Collections;

public class GUIScript : MonoBehaviour {

    private LoginScript login;

    void Start()
    {
       login = (LoginScript) GameObject.Find("Login Manager").GetComponent("LoginScript");
       DestroyObject(GameObject.Find("Login Manager"));
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), login.username);
    }
}
