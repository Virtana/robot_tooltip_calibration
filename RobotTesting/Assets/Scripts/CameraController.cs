using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CameraController : MonoBehaviour{

  public bool MaskOn;
  //Bool set in inspector to control if camera is masker or main

  public Shader ReplacShaderWhite;
  //Shader with Ids used to replace robot and marker selectively

  public GameObject ToolTipObj;
  //Small sphere on end of tooltip that is used to track tooltip

  private Vector4 _toolTipPosn;
  //value for theposition of the tooltip

  private Matrix4x4 _camIntr;
  //camera projection matrix (intrinsics)

  private Vector3 _screenCoords ;
  //answer matrix (pixel coords of image)

  private Matrix4x4 _camToWorld ;
  //camera extrinsics

  private long _fileLength;
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
      
      if ( ToolTipVisible() ) {
        Debug.Log("TOOL TIP IS VISIBLE");
        GetCoords();
      } else {
        Debug.Log("TOOL TIP IS BLOCKED");
        System.IO.File.AppendAllText( @"coords.txt" , "null , null" + "\n" );
      }
    } else {
      File.WriteAllBytes( "Images/Image" + _imageNumber + ".jpg" , bytes );
    }
    
    _imageNumber++; 
  }

  private void Start(){
    _overHeadCam = GetComponent<Camera>();
    
    _camIntr[0,0] = 3464.10161514f;
    _camIntr[0,2] = 2000.0f;
    _camIntr[1,1] = 3464.10161514f;
    _camIntr[1,2] = 2000.0f;
    _camIntr[2,2] = 1.0f;
    //Sets uep the camera intrinsics matrix

    //If the tick box is selected in the inspector, the replacement shader is loaded for cam
    if (MaskOn)
    {
      _overHeadCam.SetReplacementShader(ReplacShaderWhite, "RenderQueue");
    } 
    _camView = new Texture2D(_overHeadCam.targetTexture.width ,_overHeadCam.targetTexture.height);
		
    //Starting the repeating function that takes pictures
    InvokeRepeating("GetCamView",1.5f,3.0f);   
  }

  private void GetCoords(){
    _toolTipPosn = ToolTipObj.transform.position ;
    //Reads the position of the TCP

    _toolTipPosn[3] = 1.0f;
    //This is needed to set the 1 in the W component of the vector as it is reset every time the 
    //previous line is executed

    _camToWorld = _overHeadCam.worldToCameraMatrix ;
    //Sets the matrix that transforms the world space to camera space

    _screenCoords = _camIntr * _camToWorld * _toolTipPosn;  
    //Camera calibration calulation

    //Debug.Log( _screenCoords[0]/ _screenCoords[2] + " and " + _screenCoords[1]/ _screenCoords[2]  );

    System.IO.File.AppendAllText( @"coords.txt" , _screenCoords[0]/ _screenCoords[2] + " , " 
                                 + _screenCoords[1]/ _screenCoords[2] + "\n" );
      //Writes the coordinates to a txt file
  }
  
  private bool ToolTipVisible() {
    _fileLength = new System.IO.FileInfo("MaskedImages/MaskedImage" + _imageNumber + ".jpg").Length;
    if ( _fileLength > 250628 ) {
      return true;
    }
    else {
      return false;
    }
  }
}