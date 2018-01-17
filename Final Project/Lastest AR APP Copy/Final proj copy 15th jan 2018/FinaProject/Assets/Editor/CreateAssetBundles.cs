using UnityEngine;
using System.Collections;
using UnityEditor;

public class CreateAssetBundles : Editor 
{
	[MenuItem ("Assets/Create the AssetBundles")]
	static void BulidAllAssetBundles()
	{
		BuildPipeline.BuildAssetBundles ("Assets/AssetBundles", BuildAssetBundleOptions.None,BuildTarget.Android);
	}

}
