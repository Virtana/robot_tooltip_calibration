using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.UrdfImporter;
using UnityEngine;
using System;
using System.Globalization;

//The base robot controller code is referenced from git repo with address :
//https://github.com/sarika93/SimpleUnityROSSimulation/blob/main/SimpleROSArm/Assets/Scripts/RobotController.cs
// from user : sarika93

public class RobotController : MonoBehaviour{
  public ArticulationConfiguration ArticulationConfig;
  public float MaxJointVelDeg = 300;
  public float[] JointGoalsDeg = {0.0f, 0.0f, 0.0f, 0.0f, 0.0f};
  public float[] CurrentAnglesRad;
  public float[] CurrJointVelRad;    //actual velocity
  public bool GoalReached;

  private UrdfJointRevolute[] _revoluteJoints;
  private ArticulationBody[] _revoluteArticulations;
  private int _numJoints;
  private float[] _jointVelsTargetDeg;  // what we want velocity to be
  
  private int _posnChanger = 0;    //iterator to track position number up to 2000
  private float _newAngle = 0.0f; //each angle to be loaded into the joint on each iteration
  
  //File to read joint positions from
  //Th file contains 2000+ angle values for each joint for which the robot will not self collide
  private string[] _textLines = System.IO.File.ReadAllLines(@"SafeJointAngles.csv");

  //5 element array of joint positions for each record in csv file
  private string[] _angleValues;

  void UpdatePosnArray(){
    //Reads every 5 values of Textlines into the angles for joints, seperated by a comma 
    _angleValues = _textLines[_posnChanger].Split(",");

    for (int i=0; i<5; i++){
      //Takes the read angle from the csv and converts to float,then inserts into angle array
      _newAngle = float.Parse( _angleValues[i] , CultureInfo.InvariantCulture.NumberFormat );
      JointGoalsDeg[i] = _newAngle ;
    }
    _posnChanger++;
  }

  private void Awake(){
    //Finding all joints
    _revoluteJoints = GetComponentsInChildren<UrdfJointRevolute>();
    _numJoints = _revoluteJoints.Length;
    _revoluteArticulations = new ArticulationBody[_numJoints];
    CurrentAnglesRad = new float[_numJoints];
    for (int i = 0; i < _numJoints; i++){
      _revoluteArticulations[i] = _revoluteJoints[i].GetComponent<ArticulationBody>();
      if (_revoluteArticulations[i].jointType != ArticulationJointType.RevoluteJoint){
        Debug.LogError("Not a revolute joint!");
      }
    }

    if (JointGoalsDeg == null){
      JointGoalsDeg = new float[_numJoints];
    }
  }

  [System.Serializable]
  public class ArticulationConfiguration{
    public float Stiffness;
    public float Damping;
    // The below aren't used right now
    public float ForceLimit;
    public float Torque;
  }

  private void Start(){
    InvokeRepeating("UpdatePosnArray",0.0f,3.0f);

    for (int i = 0; i < _numJoints; i++){
      ArticulationDrive joint = _revoluteArticulations[i].xDrive;
      joint.damping = ArticulationConfig.Damping;
      joint.stiffness = ArticulationConfig.Stiffness;
      joint.forceLimit = ArticulationConfig.ForceLimit;
      _revoluteArticulations[i].xDrive = joint;
    }
    _jointVelsTargetDeg = new float[_numJoints];
    CurrJointVelRad = new float[_numJoints];
  }

  private void FixedUpdate(){
    UpdateCurrentPosVelDeg();
    // Updates the target joint velocities that we want
    UpdateJointVelTargets();
    
    if (GoalReached){
      return;
    } else {
      for (int i = 0; i < _numJoints; i++){
        ArticulationDrive drive = _revoluteArticulations[i].xDrive;
        float nextTarget = drive.target + Time.fixedDeltaTime * _jointVelsTargetDeg[i];
        if ((_jointVelsTargetDeg[i] > 0) && (nextTarget > JointGoalsDeg[i])){
          nextTarget = JointGoalsDeg[i];
        } else if ((_jointVelsTargetDeg[i] < 0) && (nextTarget < JointGoalsDeg[i])){
          nextTarget = JointGoalsDeg[i];
        } else if (_jointVelsTargetDeg[i] == 0f){
          nextTarget = drive.target;
        }
        drive.target = nextTarget;
        _revoluteArticulations[i].xDrive = drive;
      }
    }
  }

  private void UpdateCurrentPosVelDeg(){
    for (int i = 0; i < _numJoints; i++){
      CurrentAnglesRad[i] = _revoluteJoints[i].GetPosition();
      CurrJointVelRad[i] = _revoluteJoints[i].GetVelocity();
    }
  }

  private void UpdateJointVelTargets(){
    float[] jointErrors = new float[_numJoints];
    float maxError = 0.0f;  // MaxError/MaxVel will give overall time.
    for (int i = 0; i < _numJoints; i++){
      // Using target rather than current position, since the actual position
      // lags target, and we are using this for updating the xDrive target
      jointErrors[i] = JointGoalsDeg[i] - _revoluteArticulations[i].xDrive.target;
      if (Mathf.Abs(jointErrors[i]) > maxError){
        maxError = Mathf.Abs(jointErrors[i]);
      }
    }

    if (maxError <= 0.005f){
      GoalReached = true;
      for (int i = 0; i < _numJoints; i++){
        _jointVelsTargetDeg[i] = 0f;
      }
      return;
    } else {
      GoalReached = false;
    }

    // TODO: Some weirdness with this case. Check this out.
    if (MaxJointVelDeg == 0f){
      for (int i = 0; i < _numJoints; i++){
        _jointVelsTargetDeg[i] = 0f;
        ArticulationDrive drive = _revoluteArticulations[i].xDrive;
        drive.target = CurrentAnglesRad[i] * Mathf.Rad2Deg;
        _revoluteArticulations[i].xDrive = drive;
      }
      return;
    }

    float time = maxError / MaxJointVelDeg;

    for (int i = 0; i < _numJoints; i++){
      if (time != 0f){
        //Saving the velocities we want for each joint
        _jointVelsTargetDeg[i] = jointErrors[i] / time;
      } else {
        // TODO: Add in a velocity cap
        _jointVelsTargetDeg[i] = MaxJointVelDeg; // should move as fast as allowed
      }
    }
  }
}