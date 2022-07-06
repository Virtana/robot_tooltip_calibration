using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CameraController : MonoBehaviour{

  public bool MaskOn;
  
  private int _imageNumber;	
  private Texture2D _camView;
  private Camera _overHeadCam;
  private string _filePath;

  void GetCamView() {
    RenderTexture currentRenText = RenderTexture.active;
    RenderTexture.active = _overHeadCam.targetTexture;

    _overHeadCam.Render();

    _camView.ReadPixels(new Rect(0,0, _overHeadCam.targetTexture.width,
                                 _overHeadCam.targetTexture.height),0,0);

    _camView.Apply();
    RenderTexture.active = currentRenText;

    byte[] bytes = _camView.EncodeToJPG();

    //If the tick box is selected in the inspector, then the path is changed accordingly

    File.WriteAllBytes( _filePath + _imageNumber + ".jpg" , bytes );
    
    _imageNumber++; 
  }

  private void Start(){
    _overHeadCam = GetComponent<Camera>();
    Shader replacementShader = Resources.Load<Shader>("Shader/WhiteReplacementShader");
     
    //If the tick box is selected in the inspector, the replacement shader is loaded for cam
    if (MaskOn) {
      _filePath = "MaskedImages/MaskedImage";
      _overHeadCam.SetReplacementShader( replacementShader , "RenderType");

      InitilaiseDirectories();
      //The directories need to be checked first, does not matter if it checked in the masked
      //script or regular, just only once , ie not twice across the two scripts of the camera

    } else {
      _filePath = "Images/Image";
    }

    _camView = new Texture2D(_overHeadCam.targetTexture.width ,_overHeadCam.targetTexture.height);
		
    //Starting the repeating function that takes pictures
    InvokeRepeating("GetCamView",1.5f,3.0f);   
  }

  void InitilaiseDirectories(){ 
    if (!Directory.Exists("MaskedImages")) {
      Directory.CreateDirectory("MaskedImages");
    }
    

    if (!Directory.Exists("Images")) {
      Directory.CreateDirectory("Images");
    }
    //creates the directory if it doesnt exist
  }
}
