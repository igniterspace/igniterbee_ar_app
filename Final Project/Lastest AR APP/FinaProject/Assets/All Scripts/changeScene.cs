using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class changeScene : MonoBehaviour {

	public GameObject errorPanel;

	void Start() {
		errorPanel.SetActive(false);
	}

	//this method will chage the scene with the scene number is given
	public void chageToScene(int sceneNumber){

		//scene number is the number stated in the build settingd
		SceneManager.LoadScene(sceneNumber); //loading the scene
	}


	//this method check for the internet connectivity and change the scene if internet is available...or else a panel is set active as an error ,messgae
	public void chageToSceneToArCam(){

		//checking if internet is reachable or not
		if (Application.internetReachability == NetworkReachability.NotReachable) {

			//if not reachable .. this pop up panel will be set to visible
			errorPanel.SetActive(true);

		} else{
			//scene number is the number stated in the build settingd
			SceneManager.LoadScene("ArCameraScene"); //loading the scene
			errorPanel.SetActive(false);
		}
	
	}

	//THIS METHOD WILL HIDE THE POPUP PANEL
	public void hidePanel(){
		errorPanel.SetActive(false);
	}


	void update(){
		

	}


}
