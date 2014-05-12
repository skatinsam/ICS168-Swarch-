using UnityEngine;
using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

public class LoginScript : MonoBehaviour 
{
    public string username = "Enter username";
    public string password = "";

    private MD5 md5;
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
		canTryLogin = false;

        md5 = MD5.Create();


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

		}
	}

    void OnGUI()
    {
        

		if ( GUI.Button( new Rect( 0, 50, 100, 20), "Disconnect"))
		{
						
			gameProcess.returnSocket().endThread();
			gameProcess.returnSocket().Disconnect();

		}
		if (!loginEntered && canTryLogin)
        {
			username = GUI.TextField(new Rect(Screen.width / 4, Screen.height / 4, 200, 20), username, 25);
			password = GUI.PasswordField(new Rect(Screen.width / 4, Screen.height / 4 + 20, 200, 20), password, "*"[0],25);
			
			if (GUI.Button(new Rect(Screen.width / 2, Screen.height / 4, 100, 20), "Log In"))
            {
                if (username == "Enter username" || password == "")
                {
					guiT.text = "Incorrect Username or Password ";
                    Debug.Log("Please try again");
                }
                else
                {
					gameProcess.returnSocket().sendQueue.Enqueue("userAndPass\\"
					                                    + username +"\\"
					                                    + hash(password) );

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

			if(splitData[0] == "clientNumber")
			{
				gameProcess.clientNumber = (Convert.ToInt32(splitData[1]));

				for(int i = 2; i< splitData.Length; ++i)
				{
					gameProcess.pelletsLocation.Add(float.Parse(splitData[i])); 
				}

				Application.LoadLevel("swarch(Whale)");



				gameProcess.loadPellets = true;
			}
            
			if(splitData[0] == "incorrectUserPass")
			{
				guiT.text = "incorrect password or username, try again ";
				loginEntered = false;


			}
		}

    }

    public string hash(string info)
    {

        byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(info));
        StringBuilder s = new StringBuilder();

        for (int i = 0; i < data.Length; i++)
        {
            s.Append(data[i].ToString("x2"));
        }

        return s.ToString();
    }

}
