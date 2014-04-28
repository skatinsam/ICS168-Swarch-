using UnityEngine;
using System.Collections;

public class LoginScript : MonoBehaviour 
{
    public string username = "Enter username";
    public string password = "Enter password";

    bool loginEntered = false;

	private GameProcess gameProcess;
	private bool connectSuccess;
	private string data;
	private string[] splitData;
	public GUIText guiT;
	public bool faledConnection;

	//public GameProcess gp;
    void Awake()
    {
		DontDestroyOnLoad(transform.gameObject);
		splitData = new string[]{"",""};
		connectSuccess = false;
		faledConnection = false;
		gameProcess = GameObject.Find("GameProcess").GetComponent<GameProcess>();
		DontDestroyOnLoad(gameProcess);
		DontDestroyOnLoad(guiT);
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
					guiT.text = "Incorrect Username or Password ";
                    Debug.Log("Please try again");
                }
                else
                {



                     Debug.Log("Login successful");
                     loginEntered = true;
                     
					
				}

				if(loginEntered && !connectSuccess )
				{
					print("Connecting...");
					if ( gameProcess.socks.Connect() )
					{	
						guiT.text = "";

						print("Connect Succeeded");
						connectSuccess = true;
					}
					else
					{
						guiT.text = "Connect Failed, try again ";
						print ("\nCONNECTION HAS FAILED, TRY AGAIN ");
						//faledConnection =true;
						loginEntered = false;
						//guiT.text = "";
						// ***** have a button here incase need to try to connect to server again 
					}
				
			   	}
				   

			}

		}

	}
	//}
	
    void Update()
    {
	  
	 
		if(gameProcess.socks.recvBuffer.Count > 0)
		{
			
			data = (string)gameProcess.socks.recvBuffer.Dequeue();
			
			splitData = data.Split(gameProcess.delemeter);
			
			if(splitData[0] == "correctUserPass")
			{
				Application.LoadLevel("swarch(Whale)");
			}
            
			if(splitData[0] == "incorrectUserPass")
			{
				loginEntered = false;
				OnGUI();

			}
		}
/*
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
*/
    }


}
