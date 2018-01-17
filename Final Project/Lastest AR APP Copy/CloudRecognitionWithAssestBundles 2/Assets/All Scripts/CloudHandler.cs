using System;
using UnityEngine;
using Vuforia;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// This MonoBehaviour implements the Cloud Reco Event handling for this sample.
/// It registers itself at the CloudRecoBehaviour and is notified of new search results.
/// </summary>
public class CloudHandler : MonoBehaviour, ICloudRecoEventHandler
{
	#region PRIVATE_MEMBER_VARIABLES

	// CloudRecoBehaviour reference to avoid lookups
	private CloudRecoBehaviour mCloudRecoBehaviour;
	// ImageTracker reference to avoid lookups
	private ObjectTracker mImageTracker;

	private bool mIsScanning = false;

	private string mTargetMetadata = "";

	#endregion // PRIVATE_MEMBER_VARIABLES



	#region EXPOSED_PUBLIC_VARIABLES

	/// <summary>
	/// can be set in the Unity inspector to reference a ImageTargetBehaviour that is used for augmentations of new cloud reco results.
	/// </summary>
	public ImageTargetBehaviour ImageTargetTemplate;

	#endregion

	#region UNTIY_MONOBEHAVIOUR_METHODS

	/// <summary>
	/// register for events at the CloudRecoBehaviour
	/// </summary>
	/// 




	void Start()
	{
		// register this event handler at the cloud reco behaviour
		CloudRecoBehaviour cloudRecoBehaviour = GetComponent<CloudRecoBehaviour>();
		if (cloudRecoBehaviour)
		{
			cloudRecoBehaviour.RegisterEventHandler(this);
		}

		// remember cloudRecoBehaviour for later
		mCloudRecoBehaviour = cloudRecoBehaviour;

	}

	#endregion // UNTIY_MONOBEHAVIOUR_METHODS


	#region ICloudRecoEventHandler_IMPLEMENTATION

	/// <summary>
	/// called when TargetFinder has been initialized successfully
	/// </summary>
	public void OnInitialized()
	{
		// get a reference to the Image Tracker, remember it
		mImageTracker = (ObjectTracker)TrackerManager.Instance.GetTracker<ObjectTracker>();
	}

	/// <summary>
	/// visualize initialization errors
	/// </summary>
	public void OnInitError(TargetFinder.InitState initError)
	{
	}

	/// <summary>
	/// visualize update errors
	/// </summary>
	public void OnUpdateError(TargetFinder.UpdateState updateError)
	{
	}

	/// <summary>
	/// when we start scanning, unregister Trackable from the ImageTargetTemplate, then delete all trackables
	/// </summary>
	public void OnStateChanged(bool scanning) {
		mIsScanning = scanning;
		if (scanning) {
			// clear all known trackables
			ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker> ();
			tracker.TargetFinder.ClearTrackables (false);
		}
	}

	/// <summary>
	/// Handles new search results
	/// </summary>
	/// <param name="targetSearchResult"></param>
	/// 
	public void OnNewSearchResult(TargetFinder.TargetSearchResult targetSearchResult)
	{
		// duplicate the referenced image target
		GameObject newImageTarget = Instantiate(ImageTargetTemplate.gameObject) as GameObject;

		GameObject augmentation = null;

		string model_name = targetSearchResult.MetaData;


		if( augmentation != null )
			augmentation.transform.parent = newImageTarget.transform;

		// enable the new result with the same ImageTargetBehaviour:
		ImageTargetAbstractBehaviour imageTargetBehaviour = mImageTracker.TargetFinder.EnableTracking(targetSearchResult, newImageTarget);

		Debug.Log("Metadata value is =========== " + model_name );
		mTargetMetadata = model_name;



//		if(model_name=="dualBrushBot"){
//			//clearing all objects
//			Destroy( imageTargetBehaviour.gameObject.transform.Find("electricFan").gameObject );
//		}
//			
	


		//============== this method call makes the object visble on the target image ============== 

		StartCoroutine (DownloadObject(model_name));

		//========================================================================================== 



		if (!mIsScanning)
		{
			// stop the target finder
			mCloudRecoBehaviour.CloudRecoEnabled = true;
		}
	}


	#endregion // ICloudRecoEventHandler_IMPLEMENTATION

	void OnGUI() {
		GUI.Box (new Rect(100,200,200,50), "Metadata: " + mTargetMetadata);
	}





	//this metod will download the assetsbunddel for AWS S3 bucket and search for a specific object and initialize it


	//Version of the AssetBundle. The file will only be loaded from the disk
	//cache if it has previously been downloaded with the same version parameter. 
	//By incrementing the version number requested by your application, you can force Caching to download a new copy of the AssetBundle from url.


	IEnumerator DownloadObject(String model_name){

		//this variable stores the name of the game object you need to download and display
		string nameOfObject = model_name;


		int version=11;


		if (model_name == "dualBrushBot") {
			version = 12;
		} else if (model_name == "electricFan") {
			version = 13;
		}
			
		//PROBLEM WITH VERSIONNING AND PROBLEM WITH CACHE AND OBJECT DELETION


		//this string is the download url
		string downloadUrl = "https://s3-ap-southeast-1.amazonaws.com/ar-app-objects/model."+ nameOfObject  ;

		//downloading asset bundele with the help of WWW
		WWW www = WWW.LoadFromCacheOrDownload(downloadUrl, version);
		Debug.Log ("Downloading and version ==== " +version);
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
		objScale.x = 2.9F;
		objScale.y = 2.9F;
		objScale.z = 2.9F;
		//setting the vector to the game object scale
		gameObject.transform.localScale = objScale;

		//this vector 3 will store the ROTATION of the object
		gameObject.transform.Rotate(new Vector3(-90,0,0));

		//================================== Instantiate game object ================================


		//initialzing/displaying the game object
		Instantiate<GameObject> (gameObject);


		//  -= -= -= -=- =-
		//bundle.Unload(false);
	}






}