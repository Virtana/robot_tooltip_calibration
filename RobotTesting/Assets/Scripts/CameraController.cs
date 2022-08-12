using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class CameraController : MonoBehaviour{

  public bool MaskOn;
  //Bool set in inspector to control if camera is masker or main

  public Shader ReplacShaderWhite;
  //Shader with Ids used to replace robot and marker selectively

  public GameObject ToolTipObj;
  //Small sphere on end of tooltip that is used to track tooltip

  public RenderTexture cameraTexture;
  //REnder tecture for camera to set desired resolution 

  private Vector4 _toolTipPos;
  //value for theposition of the tooltip

  private Matrix4x4 _camIntr;
  //camera projection matrix (intrinsics)

  private Vector3 _tooltipCoords ;
  //answer matrix (pixel coords of image)

  private Matrix4x4 cam_T_World ;
  //camera extrinsics

  private long _fileLength;
  private int _imageNumber;	
  private Texture2D _camView;
  private Camera _overHeadCam;
  private string _imageFilePath ;
  
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
    File.WriteAllBytes( _imageFilePath + _imageNumber + ".jpg" , bytes );

    if (!MaskOn) {
      if (ToolTipVisible()) {
        GetCoords();
      } else {
        System.IO.File.AppendAllText( @"coords.txt" , "null , null" + "\n" );
      }
    }
    _imageNumber++; 
  }

  private void Start(){
    InitialiseFiles();
    _overHeadCam = GetComponent<Camera>();
    
    _camIntr[0,0] = ((cameraTexture.width * 0.5f )/( (float)Math.Tan(_overHeadCam.fieldOfView *0.5f 
                      * (3.1415926/180))));
    //Focal length in pixels calculation found from:
    //https://answers.opencv.org/question/17076/conversion-focal-distance-from-mm-to-pixels/

    _camIntr[0,2] = cameraTexture.width/2;
    _camIntr[1,1] = ((cameraTexture.height * 0.5f )/( (float)Math.Tan(_overHeadCam.fieldOfView *0.5f
                    * (3.1415926/180))));

    _camIntr[1,2] = cameraTexture.height/2;
    _camIntr[2,2] = 1.0f;
    //Sets up the camera intrinsics matrix basedon the camera properties

    //If the tick box is selected in the inspector, the replacement shader is loaded for cam
    if (MaskOn) {
      _imageFilePath = "MaskedImages/MaskedImage";
      //_overHeadCam.SetReplacementShader(ReplacShaderWhite, "RenderQueue");
    } else {
      _imageFilePath = "Images/Image";  
    } 
    _camView = new Texture2D(_overHeadCam.targetTexture.width ,_overHeadCam.targetTexture.height);
		
    //Starting the repeating function that takes pictures
    InvokeRepeating("GetCamView",1.5f,3.0f);   
  }

  private void GetCoords(){
    _toolTipPos = ToolTipObj.transform.position ;
    //Reads the position of the TCP

    _toolTipPos[3] = 1.0f;
    //This is needed to set the 1 in the W component of the vector as it is reset every time the 
    //previous line is executed

    cam_T_World = _overHeadCam.worldToCameraMatrix ;
    //Sets the matrix that transforms the world space to camera space

    _tooltipCoords = _camIntr * cam_T_World * _toolTipPos;  
    //Camera projection equation, further details can be found at:
    //https://eikosim.com/en/non-classe-en/camera-calibration-principles-and-procedures/
    //https://docs.opencv.org/2.4/modules/calib3d/doc/camera_calibration_and_3d_reconstruction.html


    System.IO.File.AppendAllText( @"coords.txt" , _tooltipCoords[0]/ _tooltipCoords[2] + " , " 
                                 + _tooltipCoords[1]/ _tooltipCoords[2] + "\n" );
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
    //TODO : Change file length check from being hardcoded to reading file size from a dummy image
    //with no pixels (full black image )
  }

  private void InitialiseFiles(){
    if (File.Exists(@"coords.txt")) {
      File.Delete(@"coords.txt");
    }
    //Deletes the coordinates file if it already exists
  }
}
