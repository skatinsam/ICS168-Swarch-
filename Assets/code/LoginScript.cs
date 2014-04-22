using UnityEngine;
using System.Collections;

public class LoginScript : MonoBehaviour 
{
    public string username = "Enter username";
    public string password = "Enter password";

    bool loginEntered = false;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    void OnGUI()
    {
        username = GUI.TextField(new Rect(Screen.width / 4, Screen.height / 4, 200, 20), username, 25);
        password = GUI.PasswordField(new Rect(Screen.width / 4, Screen.height / 4 + 20, 200, 20), password, "*"[0],25);

        if (!loginEntered)
        {
            if (GUI.Button(new Rect(Screen.width / 2, Screen.height / 4, 100, 20), "Log In"))
            {
                if (username == "Enter username" || password == "Enter password")
                {
                    Debug.Log("Please try again");
                }
                else
                {
                    Debug.Log("Login successful");
                    loginEntered = true;
                    Application.LoadLevel("swarch(Whale)");
                }
            }
        }
    }

    /*
    void Update()
    {
       
        if(Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Something");
            if (username == "Enter username" || password == "Enter password")
            {
                Debug.Log("Please try again");
            }
            else
            {
                Debug.Log("Login successful");
                loginEntered = true;
                Application.LoadLevel("swarch(Whale)");
            }
        }

    }
     */

    

}
