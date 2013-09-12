using UnityEngine;
using System.Collections;

public class NetworkUtils 
	//: MonoBehaviour 
{
	// Use this for initialization
	void Start () {}

	
	/*
	 * http://docs.unity3d.com/Documentation/Components/net-MasterServer.html
	 Here we just decide if NAT punchthrough is needed by checking whether or not the machine has a public address. There is a more sophisticated function available called Network.TestConnection which can tell you if the host machine can do NAT or not. It also does connectivity testing for public IP addresses to see if a firewall is blocking the game port. Machines which have public IP addresses always pass the NAT test but if the test fails then the host will not be able to connect to NAT clients. In such a case, the user should be informed that port forwarding must be enabled for the game to work. Domestic broadband connections will usually have a NAT address but will not be able to set up port forwarding (since they don't have a personal public IP address). In these cases, if the NAT test fails, the user should be informed that running a server is inadvisable given that only clients on the same local network will be able to connect.

     If a host enables NAT functionality without needing it then it will still be accessible. However, clients which cannot do NAT punchthrough might incorrectly think they cannot connect on the basis that the server has NAT enabled.
	 */
	public static class ConnectionTester //only use on the server side
	{
		//network testing
		//http://docs.unity3d.com/Documentation/ScriptReference/Network.TestConnection.html
		public static string testStatus = "Testing network connection capabilities.";
		public static string testMessage = "Test in progress";
		public static string shouldEnableNatMessage = "";
		public static bool useNat = false; // Indicates if the useNat parameter be enabled when starting a server
		
		private static bool doneTesting = false;
		private static bool probingPublicIP = false;
		//private static int serverPort = 9999;
		private static ConnectionTesterStatus connectionTestResult = ConnectionTesterStatus.Undetermined;
		
		private static float timer;
		
		public static bool isDoneTesting()
		{
			return doneTesting;
		}
		
		//returns doneTesting
		public static bool TestConnection( int port, bool forceTest ) 
		{
	    // Start/Poll the connection test, report the results in a label and 
	    // react to the results accordingly
	    	//Network.connectionTesterPort = port;
			
			connectionTestResult = Network.TestConnection( forceTest );
	   		switch (connectionTestResult) {
	        case ConnectionTesterStatus.Error: 
	            testMessage = "Problem determining NAT capabilities";
	            doneTesting = true;
	            break;
	            
	        case ConnectionTesterStatus.Undetermined: 
	            testMessage = "Undetermined NAT capabilities";
	            doneTesting = false;
	            break;
	                        
	        case ConnectionTesterStatus.PublicIPIsConnectable:
	            testMessage = "Directly connectable public IP address.";
	            useNat = false;
	            doneTesting = true;
	            break;
	            
	        // This case is a bit special as we now need to check if we can 
	        // circumvent the blocking by using NAT punchthrough
	        case ConnectionTesterStatus.PublicIPPortBlocked:
	            testMessage = "Non-connectable public IP address (port " +
	                port +" blocked), running a server is impossible.";
	            useNat = false;
	            // If no NAT punchthrough test has been performed on this public 
	            // IP, force a test
				
	            if (!probingPublicIP) {
	                connectionTestResult = Network.TestConnectionNAT();
	                probingPublicIP = true;
	                testStatus = "Testing if blocked public IP can be circumvented";
	                timer = Time.time + 10;
	            }
	            // NAT punchthrough test was performed but we still get blocked
	            else if (Time.time > timer) {
	                probingPublicIP = false;         // reset
	                useNat = true;
	                doneTesting = true;
	            }
	            break;
	        case ConnectionTesterStatus.PublicIPNoServerStarted:
	            testMessage = "Public IP address but server not initialized, "+
	                "it must be started to check server accessibility. Restart "+
	                "connection test when ready (not necc.).";
				doneTesting = true;
	            break;
	                        
	        case ConnectionTesterStatus.LimitedNATPunchthroughPortRestricted:
	            testMessage = "Limited NAT punchthrough capabilities. Cannot "+
	                "connect to all types of NAT servers. Running a server "+
	                "is ill advised as not everyone can connect.";
	            useNat = true;
	            doneTesting = true;
	            break;
	            
	        case ConnectionTesterStatus.LimitedNATPunchthroughSymmetric:
	            testMessage = "Limited NAT punchthrough capabilities. Cannot "+
	                "connect to all types of NAT servers. Running a server "+
	                "is ill advised as not everyone can connect.";
	            useNat = true;
	            doneTesting = true;
	            break;
	        
	        case ConnectionTesterStatus.NATpunchthroughAddressRestrictedCone:
	        case ConnectionTesterStatus.NATpunchthroughFullCone:
	            testMessage = "NAT punchthrough capable. Can connect to all "+
	                "servers and receive connections from all clients. Enabling "+
	                "NAT punchthrough functionality.";
	            useNat = true;
	            doneTesting = true;
	            break;
	
	        default: 
	            testMessage = "Error in test routine, got " + connectionTestResult;
				break;
	    	}
		    if (doneTesting) {
				DebugConsole.Log("Connection Test done.");
		        if (useNat)
		            shouldEnableNatMessage = "When starting a server the NAT "+
		                "punchthrough feature should be enabled (useNat parameter)";
		        else
		            shouldEnableNatMessage = "NAT punchthrough not needed";
		        testStatus = "Done testing";
		    }
			return doneTesting;
		}		
	}

}

