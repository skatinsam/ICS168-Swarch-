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
	public bool failedConnection;
	public bool canTryLogin;
	//public GameProcess gp;
    void Awake()
    {
		DontDestroyOnLoad(transform.gameObject);
		splitData = new string[]{"",""};
		connectSuccess = false;
		failedConnection = false;
		gameProcess = GameObject.Find("GameProcess").GetComponent<GameProcess>();
		DontDestroyOnLoad(gameProcess);
		DontDestroyOnLoad(guiT);
		//DontDestroyOnLoad(gameProcess.socks);
		canTryLogin = false;


	}

	void Start()
	{
		print("Connecting...");
		if ( gameProcess.returnSocket().Connect() )
		{	
			guiT.text = "";
			
			print("Connect Succeeded");
			connectSuccess = true;
			
			
			
			
		}
		else
		{
			guiT.text = "Connect Failed, try again ";
			print ("\nCONNECTION HAS FAILED, TRY AGAIN ");
			failedConnection =true;
			//loginEntered = false;
			//guiT.text = "";
			// ***** have a button here incase need to try to connect to server again 
		}
	}

    void OnGUI()
    {
        

		if ( GUI.Button( new Rect( 0, 0, 100, 20), "Disconnect"))
		{
			//********* COMPLETE THE FOLLOWING CODE
			//********* KILL THREAD AND SEVER CONNECTION
			
			//returnSocket().SendTCPPacket ((byte) (process.commands[(int)GameProcess.codes.roll]));
			
			//process.sendEndGame();
			
			gameProcess.returnSocket().endThread();
			gameProcess.returnSocket().Disconnect();
			

		}
		if (!loginEntered && canTryLogin)
        {
			username = GUI.TextField(new Rect(Screen.width / 4, Screen.height / 4, 200, 20), username, 25);
			password = GUI.PasswordField(new Rect(Screen.width / 4, Screen.height / 4 + 20, 200, 20), password, "*"[0],25);
			
			if (GUI.Button(new Rect(Screen.width / 2, Screen.height / 4, 100, 20), "Log In"))
            {
                if (username == "Enter username" || password == "Enter password")
                {
					guiT.text = "Incorrect Username or Password ";
                    Debug.Log("Please try again");
                }
                else
                {
					gameProcess.returnSocket().sendQueue.Enqueue("userAndPass\\"
					                                    + username +"\\"
					                                    + password );


                     Debug.Log("Login successful");
                     loginEntered = true;
                     
					
				}


			}

		}
		if(failedConnection)
		{
			if (GUI.Button (new Rect (150,60,100,20), "Connect")) 
			{
				failedConnection = false;
				print("Connecting...");
				if ( gameProcess.returnSocket().Connect() )
				{	
					guiT.text = "";
					
					print("Connect Succeeded");
					connectSuccess = true;

				}
				else
				{
					guiT.text = "Connect Failed, try again ";
					print ("\nCONNECTION HAS FAILED, TRY AGAIN ");
					failedConnection =true;
				
				}
			}

		}

	}
	//}
	
    void Update()
    {
	  
	 
		if(gameProcess.returnSocket().recvBuffer.Count > 0)
		{
			
			data = (string)gameProcess.returnSocket().recvBuffer.Dequeue();
			
			splitData = data.Split(gameProcess.delemeter);

			if(splitData[0] == "connected")
			{
				canTryLogin = true;
			}

			if(splitData[0] == "correctUserPass")
			{

				Application.LoadLevel("swarch(Whale)");
				gameProcess.loadPellets = true;
			}
            
			if(splitData[0] == "incorrectUserPass")
			{
				loginEntered = false;
				//OnGUI();

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
