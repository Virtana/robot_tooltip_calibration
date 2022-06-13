using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CamScript : MonoBehaviour{

  private int _imageNumber;	

  void getCamView(){

    Camera overHeadCam = GetComponent<Camera>();

	RenderTexture currentRenText = RenderTexture.active;
	RenderTexture.active = overHeadCam.targetTexture;

	overHeadCam.Render();

	Texture2D camView = new Texture2D(overHeadCam.targetTexture.width ,overHeadCam.targetTexture.height);
	camView.ReadPixels(new Rect(0,0, overHeadCam.targetTexture.width,overHeadCam.targetTexture.height),0,0);
	camView.Apply();
	RenderTexture.active = currentRenText;

	var Bytes= camView.EncodeToJPG();
	Destroy(camView);

	File.WriteAllBytes(Application.dataPath + "/Images/" + _imageNumber + ".jpg", Bytes );
	_imageNumber++;

  }

  private void Start()
  {

    InvokeRepeating("getCamView", 1.0f , 1.0f);   
        //Starting the repeating function that takes pictures

  }

}
