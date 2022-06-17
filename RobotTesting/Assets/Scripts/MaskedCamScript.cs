using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MaskedCamScript : MonoBehaviour{

  private int _imageNumber= 0;	
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

	  byte[] Bytes= _camView.EncodeToJPG();
    Destroy(_camView);

	  System.IO.File.WriteAllBytes( "MaskedImages/MaskedImage" + _imageNumber + ".jpg", Bytes );
	  _imageNumber++;
    Debug.Log("Masked Picture Taken");
  }

  private void Start(){
    _overHeadCam = GetComponent<Camera>();
    
    //Starting the repeating function that takes pictures
    InvokeRepeating("GetCamView", 1.0f , 3.0f);   
  }

}
