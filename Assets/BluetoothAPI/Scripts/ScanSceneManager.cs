using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArduinoBluetoothAPI;
using System;

public class ScanSceneManager : MonoBehaviour {

	// Use this for initialization
	BluetoothHelper bluetoothHelper;
	string deviceName;

	public Text text;

	public GameObject sphere;

	string received_message;

	void Start () {
		try
		{
			BluetoothHelper.BLE = true;  //use Bluetooth Low Energy Technology
			bluetoothHelper = BluetoothHelper.GetInstance();
			bluetoothHelper.OnConnected += OnConnected;
			bluetoothHelper.OnConnectionFailed += OnConnectionFailed;
			bluetoothHelper.OnDataReceived += OnMessageReceived; //read the data
			bluetoothHelper.OnScanEnded += OnScanEnded;
			bluetoothHelper.OnScanStarted += () => {
				text.text = "Started Scan";
			};
			bluetoothHelper.setTerminatorBasedStream("\n");
			
			if(bluetoothHelper.isDeviceFound())
				sphere.GetComponent<Renderer>().material.color = Color.blue;
			else
				sphere.GetComponent<Renderer>().material.color = Color.grey;
			
			if(!bluetoothHelper.ScanNearbyDevices())
			{
				text.text = "cannot start scan";
				sphere.GetComponent<Renderer>().material.color = Color.black;
				
				// bluetoothHelper.setDeviceAddress("00:21:13:02:16:B1");
				bluetoothHelper.setDeviceName("HC-08");
				bluetoothHelper.Connect();
			}
		}
		catch (Exception ex) 
		{
			sphere.GetComponent<Renderer>().material.color = Color.yellow;
			Debug.Log (ex.Message);
			text.text = ex.Message;
		}
	}

	IEnumerator blinkSphere()
	{
		sphere.GetComponent<Renderer>().material.color = Color.cyan;
		yield return new WaitForSeconds(0.5f);
		sphere.GetComponent<Renderer>().material.color = Color.green;
	}

	//Asynchronous method to receive messages
	void OnMessageReceived()
	{
		StartCoroutine(blinkSphere());
		received_message = bluetoothHelper.Read ();
		text.text = received_message;
		Debug.Log(received_message);
	}

	void OnScanEnded(LinkedList<BluetoothDevice> nearbyDevices)
	{
		text.text = "Found "+nearbyDevices.Count+" devices";
		if(nearbyDevices.Count == 0)
			return;
		text.text = nearbyDevices.First.Value.DeviceAddress;
		bluetoothHelper.setDeviceAddress(nearbyDevices.First.Value.DeviceAddress);
		// bluetoothHelper.setDeviceAddress("00:21:13:02:16:B1");
		bluetoothHelper.Connect();
	}

	void OnConnected()
	{
		sphere.GetComponent<Renderer>().material.color = Color.green;
		try{
			bluetoothHelper.StartListening ();
		}catch(Exception ex){
			Debug.Log(ex.Message);
		}
			
	}

	void OnConnectionFailed()
	{
		sphere.GetComponent<Renderer>().material.color = Color.red;
		Debug.Log("Connection Failed");
	}

	//Call this function to emulate message receiving from bluetooth while debugging on your PC.
	void OnGUI()
	{
		if(bluetoothHelper!=null)
			bluetoothHelper.DrawGUI();
		else 
			return;

		// if(!bluetoothHelper.isConnected())
		// if(GUI.Button(new Rect(Screen.width/2-Screen.width/10, Screen.height/10, Screen.width/5, Screen.height/10), "Connect"))
		// {
		// 	if(bluetoothHelper.isDeviceFound())
		// 		bluetoothHelper.Connect (); // tries to connect
		// 	else
		// 		sphere.GetComponent<Renderer>().material.color = Color.magenta;
		// }

		if(bluetoothHelper.isConnected())
		if(GUI.Button(new Rect(Screen.width/2-Screen.width/10, Screen.height - 2*Screen.height/10, Screen.width/5, Screen.height/10), "Disconnect"))
		{
			bluetoothHelper.StopListening ();
			sphere.GetComponent<Renderer>().material.color = Color.blue;
		}

		if(bluetoothHelper.isConnected())
		if(GUI.Button(new Rect(Screen.width/2-Screen.width/10, Screen.height/10, Screen.width/5, Screen.height/10), "Send text"))
		{
			bluetoothHelper.SendData("This is a very long long long long text");
		}
	}

	void OnDestroy()
	{
		if(bluetoothHelper!=null)
			bluetoothHelper.StopListening ();
	}
}
