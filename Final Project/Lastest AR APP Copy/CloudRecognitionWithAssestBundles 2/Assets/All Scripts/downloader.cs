using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class downloader : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine (DownloadObject());
	}

	//this metod will download the assetsbunddel for AWS S3 bucket and search for a specific object and initialize it

	IEnumerator DownloadObject(){

		//this variable stores the name of the game object you need to download and display
		string nameOfObject = "dualBrushBot";

		//this string is the download url
		string downloadUrl = "https://s3-ap-southeast-1.amazonaws.com/ar-app-objects/"+nameOfObject ;

		//downloading asset bundele with the help of WWW
		WWW www = WWW.LoadFromCacheOrDownload(downloadUrl,1);
		Debug.Log ("Downloading");
		yield return www;

		//setting the asset bundle to the bunddel downloaded
		AssetBundle bundle = www.assetBundle;
		//requestion a specific object in the assets bunndle using the objec/prefab name
		AssetBundleRequest request = bundle.LoadAssetAsync<GameObject> (nameOfObject);
		Debug.Log ("Making request");
		yield return request;
		 
		//making a game object and setting it with the object recived from the request
		GameObject gameObject = request.asset as GameObject;


		//================================= TRANFORMATIONS ==========================================
		//this vector 3 will store the POSITION of the object
		Vector3 objPos = gameObject.transform.position;
		//setting postions x ,y and z axises to 0,0,0
		objPos.x = 0;
		objPos.y = 0;
		objPos.z = 0;
		//setting the vector to the game object position
		gameObject.transform.position = objPos;

		//this vector 3 will store the SCALE of the object
		Vector3 objScale = gameObject.transform.localScale;
		//setting scale x ,y and z axises to 0,0,0
		objScale.x = 0.1F;
		objScale.y = 0.1F;
		objScale.z = 0.1F;
		//setting the vector to the game object scale
		gameObject.transform.localScale = objScale;

		//this vector 3 will store the ROTATION of the object
		gameObject.transform.Rotate(new Vector3(-90,0,0));

		//================================== Instantiate game object ================================

	
		//initialzing/displaying the game object
		Instantiate<GameObject> (gameObject);
	}

		


	// Update is called once per frame
	void Update () {
		
	}
}
