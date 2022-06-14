using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CamScript : MonoBehaviour{

  private int _imageNumber;	
  private Texture2D _camView;
  private Camera _overHeadCam;

  void GetCamView(){

    RenderTexture currentRenText = RenderTexture.active;
	  RenderTexture.active = _overHeadCam.targetTexture;

	  _overHeadCam.Render();

	  _camView = new Texture2D(_overHeadCam.targetTexture.width ,
	                        _overHeadCam.targetTexture.height);
							
	  _camView.ReadPixels(new Rect(0,0, _overHeadCam.targetTexture.width,
	                              _overHeadCam.targetTexture.height),0,0);
	  _camView.Apply();
	  RenderTexture.active = currentRenText;

	  var Bytes= _camView.EncodeToJPG();

	  File.WriteAllBytes(Application.dataPath + "/Images/" + _imageNumber + ".jpg", Bytes );
	  _imageNumber++;
  }

  private void Start(){
    _overHeadCam = GetComponent<Camera>();
    
    //Starting the repeating function that takes pictures
    InvokeRepeating("GetCamView", 1.0f , 2.0f);   
  }

}
