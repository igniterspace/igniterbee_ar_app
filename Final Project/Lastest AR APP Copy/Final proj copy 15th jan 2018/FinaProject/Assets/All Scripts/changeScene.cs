using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class changeScene : MonoBehaviour {

	//this method will chage the scene with the scene number is given
	public void chageToScene(int sceneNumber){

		//scene number is the number stated in the build settingd
		SceneManager.LoadScene(sceneNumber); //loading the scene
	}


//	public void openWebUrl(){
//
//		Application.OpenURL("https://www.igniterbee.com/");
//
//	}
//

}
