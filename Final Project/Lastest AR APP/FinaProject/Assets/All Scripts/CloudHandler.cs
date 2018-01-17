using System;
using UnityEngine;
using Vuforia;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;


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

	public GameObject buttonLearn;//this hold the button gameobject which redirects to the web view

	public static string webViewURL="https://www.igniterbee.com/"; //this variable holds the url for the web view to load

	public Text statusText; //this variable hold the UI tex that incidates the name of the scanned object

	public GameObject errorPanel;//this is the pop up error for internet unavailabilty

	#endregion

	#region UNTIY_MONOBEHAVIOUR_METHODS

	/// register for events at the CloudRecoBehaviour
	void Start()
	{
		errorPanel.SetActive(true);
		// register this event handler at the cloud reco behaviour
		CloudRecoBehaviour cloudRecoBehaviour = GetComponent<CloudRecoBehaviour>();
		if (cloudRecoBehaviour)
		{
			cloudRecoBehaviour.RegisterEventHandler(this);
		}

		// remember cloudRecoBehaviour for later
		mCloudRecoBehaviour = cloudRecoBehaviour;

		buttonLearn = GameObject.Find ("learnBtn");
		buttonLearn.SetActive(false);//setting the LEARN HOW TO MAKE THIS BUTTON deactivated


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
//
//		if (!gameObjectIsFound) {
//			//this box will appear if the object is not found
//			GUI.Box (new Rect (100, 100, 200, 50), "Error: Object is not found/null");
//		} else {
//			GUI.Box (new Rect(100,200,200,50), "Metadata: " + mTargetMetadata);
//		}
//			
	
	}
		

	//this method will download the assetsbunddel for AWS S3 bucket and search for a specific object and initialize it
	IEnumerator DownloadObject(String model_name){

		//this variable stores the name of the game object you need to download and display
		string nameOfObject = model_name;
		Debug.Log ("Model_Name  = = = = " + model_name); //For debugging purposes


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

			//Text sets your text to the name of the model detected
			statusText.text = model_name +" detected" ;


			//this is to prevent the flowing error - can't be loaded because another asset bundle with the same files are already loaded
			bundle.Unload (false); 

			//================================== SETTING UP WEB VIEW LINK  ================================

			webViewURL = "https://s3-ap-southeast-1.amazonaws.com/ar-app-tutes/html-tutes/"+ model_name +".html";  //setting the web view URL  //TODO - REPLACE WITH REAL URL
			buttonLearn.SetActive(true);//setting the LEARN HOW TO MAKE THIS BUTTON ACTIVATED

		} 
		else {

			Debug.Log ("GAME OBJECT IS NULL! ! ! ! So cant not instantiate the gameobject");
			//this is to prevent the flowing error - can't be loaded because another asset bundle with the same files are already loaded
			bundle.Unload (false); 
		}
			
	}

}