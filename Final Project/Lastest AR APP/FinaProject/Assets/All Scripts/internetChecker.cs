using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class internetChecker : MonoBehaviour {

	public GameObject errorPanel;

	// Use this for initialization
	void Start () {

		errorPanel.SetActive (true);

	}
	
	// Update is called once per frame
	void Update () {

		//checking if internet is reachable or not
		if (Application.internetReachability == NetworkReachability.NotReachable) {

			//if not reachable .. this pop up panel will be set to visible
			errorPanel.SetActive(true);

		} else{
			//scene number is the number stated in the build settingd
			//
			errorPanel.SetActive(false);
		}
	}

	//this method will take the user back to main menu
	public void okBtnForNoInternet(){
		errorPanel.SetActive(false);
		SceneManager.LoadScene("newMenuScene"); //loading the scene
	}
		




}
