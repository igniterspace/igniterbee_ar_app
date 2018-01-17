using System;
using UnityEngine;
using Vuforia;
using UnityEngine.UI;
using System.Collections;



/// This MonoBehaviour implements the Cloud Reco Event handling 
/// It registers itself at the CloudRecoBehaviour and is notified of new search results.
public class CloudHandler : MonoBehaviour, ICloudRecoEventHandler
{
	#region PRIVATE_MEMBER_VARIABLES

	// CloudRecoBehaviour reference to avoid lookups
	private CloudRecoBehaviour mCloudRecoBehaviour;
	// ImageTracker reference to avoid lookups
	private ObjectTracker mImageTracker;

	private bool mIsScanning = false; //this bool check it states is scanning or not

	private string mTargetMetadata = ""; //this variable stores the meta data value of the cloud reco image target

	//this boolean checks if the game object is found or not
	private bool gameObjectIsFound = false;

	#endregion // PRIVATE_MEMBER_VARIABLES

	#region EXPOSED_PUBLIC_VARIABLES

	/// can be set in the Unity inspector to reference a ImageTargetBehaviour that is used for augmentations of new cloud reco results.
	public ImageTargetBehaviour ImageTargetTemplate;



	#endregion

	#region UNTIY_MONOBEHAVIOUR_METHODS

	/// register for events at the CloudRecoBehaviour
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

	/// called when TargetFinder has been initialized successfully
	public void OnInitialized()
	{
		// get a reference to the Image Tracker, remember it
		mImageTracker = (ObjectTracker)TrackerManager.Instance.GetTracker<ObjectTracker>();
	}


	/// visualize initialization errors
	public void OnInitError(TargetFinder.InitState initError)
	{
	}


	/// visualize update errors
	public void OnUpdateError(TargetFinder.UpdateState updateError)
	{
	}
		
	/// when we start scanning, unregister Trackable from the ImageTargetTemplate, then delete all trackables
	public void OnStateChanged(bool scanning) {
		mIsScanning = scanning;
		if (scanning) {
			// clear all known trackables
			ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker> ();

			tracker.TargetFinder.ClearTrackables (true); //TODO change this to false
		}
	}


	//this array will save a list of model names
	ArrayList arrayListOfModelName = new ArrayList();

	/// Handles new search results
	public void OnNewSearchResult(TargetFinder.TargetSearchResult targetSearchResult)
	{
		// duplicate the referenced image target - THIS THE CLONE IMAGE TARGET ON WITH THE OBJECT APEARS
		GameObject newImageTarget = Instantiate(ImageTargetTemplate.gameObject) as GameObject;

		//  GETTING THE META DATA OF IMAGE TARGET
		string model_name = targetSearchResult.MetaData;


		// enable the new result with the same ImageTargetBehaviour:
		ImageTargetAbstractBehaviour imageTargetBehaviour = mImageTracker.TargetFinder.EnableTracking(targetSearchResult, newImageTarget);

		Debug.Log("Metadata value is =  " + model_name );

		mTargetMetadata = model_name;//setting the global variable with the model name

		//======== clearing the previously set object ========

		arrayListOfModelName.Add (model_name); //adding the model name to the array list of model names

		if( arrayListOfModelName.Count> 1){

			String nameOfGameObjectToDestroy =(String)arrayListOfModelName[arrayListOfModelName.Count-2]; //getting the previous objects name

			Debug.Log ("nameOfGameObjectToDestroy === === == "+ nameOfGameObjectToDestroy);

			GameObject gameObjectToDestroy = GameObject.Find(nameOfGameObjectToDestroy+"(Clone)"); //finding the object to destroy
			Destroy (gameObjectToDestroy);//destroying the object

		}
			
	
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


	//this method is there to call gui functions
	void OnGUI() {
		


		if (!gameObjectIsFound) {
			//this box will appear if the object is not found
			GUI.Box (new Rect (100, 100, 200, 50), "Error: Object is not found/null");
		} else {
			GUI.Box (new Rect(100,200,200,50), "Metadata: " + mTargetMetadata);
		}
			
			

	}


	//Version of the AssetBundle. The file will only be loaded from the disk
	//cache if it has previously been downloaded with the same version parameter. 
	//By incrementing the version number requested by your application, you can force Caching to download a new copy of the AssetBundle from url.

	//this method will download the assetsbunddel for AWS S3 bucket and search for a specific object and initialize it
	IEnumerator DownloadObject(String model_name){

		//this variable stores the name of the game object you need to download and display
		string nameOfObject = model_name;


//		int version=11;// ======   THERE IS SOMETHING WRONG WITH THE VERSIONING =========
//
//
//		if (model_name == "dualBrushBot") {
//			version = 12;
//		} else if (model_name == "electricFan") {
//			version = 13;
//
//		} else if (model_name == "bike") {
//			version = 15;
//		}
//

		//this has will hold a unique hash value for each model name
		Hash128 hashOfModel_Name = Hash128.Parse(model_name);

			
		//THERE WASSSSSS A PROBLEM WITH VERSIONNING AND PROBLEM WITH CACHE AND OBJECT DELETION ----- SOLOUTION - USED HASH instead of version


		//this string is the download url
		string downloadUrl = "https://s3-ap-southeast-1.amazonaws.com/ar-app-objects/model."+ nameOfObject  ;


		//downloading asset bundele with the help of WWW
		WWW www = WWW.LoadFromCacheOrDownload(downloadUrl, hashOfModel_Name);
		Debug.Log ("Downloading and hashOfModel_Name  = " + hashOfModel_Name); //For debugging purposes
		yield return www;

		//this check if the asset buddle is cached OR not
		Debug.Log(" Asset bundle is Cached = "+Caching.IsVersionCached(downloadUrl, hashOfModel_Name));//For debugging purposes

		//setting the asset bundle to the bunddel downloaded
		AssetBundle bundle = www.assetBundle;


		//requestion a specific object in the assets bunndle using the objec/prefab name
		AssetBundleRequest request = bundle.LoadAssetAsync<GameObject> (nameOfObject);

		Debug.Log ("Making request");
		yield return request;

		//making a game object and setting it with the object recived from the request
		GameObject gameObject = request.asset as GameObject;

		if (gameObject != null) {

			//making the boolean true - which says that the game object is found and is ready to instatiate
			gameObjectIsFound = true;
			
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
			gameObject.transform.Rotate (new Vector3 (-90, 0, 0));

			//================================== Instantiate game object ================================

			//initialzing/displaying the game object
			Instantiate<GameObject> (gameObject);


			//this is to prevent the flowing error - can't be loaded because another asset bundle with the same files are already loaded
			bundle.Unload (false); 
		} 
		else {

			Debug.Log ("GAME OBJECT IS NULL! ! ! ! So cant not instantiate the gameobject");

		}
			
	}

}