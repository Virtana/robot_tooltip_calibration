using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CameraController : MonoBehaviour{

  public bool MaskOn;
  public Shader ReplacShaderWhite;
  
  private int _imageNumber;	
  private Texture2D _camView;
  private Camera _overHeadCam;

  void GetCamView(){
    RenderTexture currentRenText = RenderTexture.active;
    RenderTexture.active = _overHeadCam.targetTexture;

    _overHeadCam.Render();

    _camView.ReadPixels(new Rect(0,0, _overHeadCam.targetTexture.width,
                                 _overHeadCam.targetTexture.height),0,0);

    _camView.Apply();
    RenderTexture.active = currentRenText;

    byte[] bytes = _camView.EncodeToJPG();

    //If the tick box is selected in the inspector, then the path is changed accordingly
    if(MaskOn) {
      File.WriteAllBytes( "MaskedImages/MaskedImage" + _imageNumber + ".jpg" , bytes );
    } else {
      File.WriteAllBytes( "Images/Image" + _imageNumber + ".jpg" , bytes );
    }
    
    _imageNumber++; 
  }

  private void Start(){
    _overHeadCam = GetComponent<Camera>();
     
    //If the tick box is selected in the inspector, the replacement shader is loaded for cam
    if (MaskOn)
    {
      _overHeadCam.SetReplacementShader(ReplacShaderWhite, "RenderQueue");
    } 
    _camView = new Texture2D(_overHeadCam.targetTexture.width ,_overHeadCam.targetTexture.height);
		
    //Starting the repeating function that takes pictures
    InvokeRepeating("GetCamView",1.5f,3.0f);   
  }

}