/*
Copyright 2013, Nirlendu Saha and Pruthvi Prakasha.
Licensed to free distribution and usage.
*/
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// Making the bone assignment dynamic

public class ZigSkeleton : MonoBehaviour
{
    public Transform Head;
    public Transform Neck;
    public Transform Torso;
    public Transform Waist;

    public Transform LeftCollar;
    public Transform LeftShoulder;
    public Transform LeftElbow;
    public Transform LeftWrist;
    public Transform LeftHand;
    public Transform LeftFingertip;

    public Transform RightCollar;
    public Transform RightShoulder;
    public Transform RightElbow;
    public Transform RightWrist;
    public Transform RightHand;
    public Transform RightFingertip;

    public Transform LeftHip;
    public Transform LeftKnee;
    public Transform LeftAnkle;
    public Transform LeftFoot;

    public Transform RightHip;
    public Transform RightKnee;
    public Transform RightAnkle;
    public Transform RightFoot;
    public bool mirror = false;
    public bool UpdateJointPositions = false;
    public bool UpdateRootPosition = false;
    public bool UpdateOrientation = true;
    public bool RotateToPsiPose = false;
    public float RotationDamping = 30.0f;
    public float Damping = 30.0f;
    public Vector3 Scale = new Vector3(0.001f, 0.001f, 0.001f);

    public Vector3 PositionBias = Vector3.zero;

    private Transform[] transforms;

    private Quaternion[] initialRotations;
    private Vector3 rootPosition;

private Quaternion[] previousSmoothedValue ;
private Quaternion[] currentSmoothedValue ;  
private Quaternion[] previousTrendValue ;
private Quaternion[] currentTrendValue ;

// ALPHA CONSTATNS FOR HOLTS SMOOTHNING FUNCTIONS

private float alphaSpine=1.0f;
private float alphaHead=1.0f;
private float alphaNeck=1.0f;

private float alphaElbow=1.0f;
private float alphaShoulder=1.0f;
private float alphaWrist=1.0f;
private float alphaHand=1.0f;

private float alphaAnkle=1.0f;
private float alphaFoot=1.0f;
private float alphaHip=1.0f;
private float alphaKnee=1.0f;


// BETA CONSTATNS FOR HOLTS SMOOTHNING FUNCTIONS

private float betaSpine=1.0f;
private float betaHead=1.0f;
private float betaNeck=1.0f;

private float betaElbow=1.0f;
private float betaShoulder=1.0f;
private float betaWrist=1.0f;
private float betaHand=1.0f;

private float betaAnkle=1.0f;
private float betaFoot=1.0f;
private float betaHip=1.0f;
private float betaKnee=1.0f;
private Quaternion ForeCastedRotation  ;
	
	public bool FirstFrame= true;
	public bool FirstFrameInSmooth= true;

	public int count=1;
	
	private Quaternion[] FirstFrameValue ;
	
	private Quaternion[] SecondFrameValue ;

	
 public int TrackedRightElbow = (int)ZigJointId.RightElbow;
	public int TrackedLeftHip = (int)ZigJointId.LeftHip;
		//Trackedjointid = ((int)ZigJointId.RightElbow);


    ZigJointId mirrorJoint(ZigJointId joint)
    {
        switch (joint)
        {
            case ZigJointId.LeftCollar:
                return ZigJointId.RightCollar;
            case ZigJointId.LeftShoulder:
                return ZigJointId.RightShoulder;
            case ZigJointId.LeftElbow:
                return ZigJointId.RightElbow;
            case ZigJointId.LeftWrist:
                return ZigJointId.RightWrist;
            case ZigJointId.LeftHand:
                return ZigJointId.RightHand;
            case ZigJointId.LeftFingertip:
                return ZigJointId.RightFingertip;
            case ZigJointId.LeftHip:
                return ZigJointId.RightHip;
            case ZigJointId.LeftKnee:
                return ZigJointId.RightKnee;
            case ZigJointId.LeftAnkle:
                return ZigJointId.RightAnkle;
            case ZigJointId.LeftFoot:
                return ZigJointId.RightFoot;

            case ZigJointId.RightCollar:
                return ZigJointId.LeftCollar;
            case ZigJointId.RightShoulder:
                return ZigJointId.LeftShoulder;
            case ZigJointId.RightElbow:
                return ZigJointId.LeftElbow;
            case ZigJointId.RightWrist:
                return ZigJointId.LeftWrist;
            case ZigJointId.RightHand:
                return ZigJointId.LeftHand;
            case ZigJointId.RightFingertip:
                return ZigJointId.LeftFingertip;
            case ZigJointId.RightHip:
                return ZigJointId.LeftHip;
            case ZigJointId.RightKnee:
                return ZigJointId.LeftKnee;
            case ZigJointId.RightAnkle:
                return ZigJointId.LeftAnkle;
            case ZigJointId.RightFoot:
                return ZigJointId.LeftFoot;


            default:
                return joint;
        }
    }


    public void Awake()
    {
        int jointCount = Enum.GetNames(typeof(ZigJointId)).Length;

        transforms = new Transform[jointCount];
        initialRotations = new Quaternion[jointCount];
		
		
			//variables for smoothing
					previousSmoothedValue = new Quaternion[jointCount];
					currentSmoothedValue  = new Quaternion[jointCount];
					previousTrendValue    = new Quaternion[jointCount];
					currentTrendValue     = new Quaternion[jointCount];
				
		// Creating space for 1st and 2nd frame
			FirstFrameValue =new Quaternion[jointCount];
			SecondFrameValue=new Quaternion[jointCount];
			
		
		// Dynamically attaching the bones to its respective transformations
		
		
			// HEAD AND SPINE
		Head=(GameObject.Find("HEAD")).transform;
		Neck=(GameObject.Find("NECK")).transform;
		Torso=(GameObject.Find("SPINE")).transform;
		
			//LEFT SHOULDER AND CHILDREN
		LeftShoulder=(GameObject.Find("SHOULDER_LEFT")).transform;
		LeftElbow=(GameObject.Find("ELBOW_LEFT")).transform;
		LeftWrist=(GameObject.Find("WRIST_LEFT")).transform;
		LeftHand=(GameObject.Find("HAND_LEFT")).transform;
	
		
			//RIGHT SHOULDER AND CHILDREN
		RightShoulder=(GameObject.Find("SHOULDER_RIGHT")).transform;
		RightElbow=(GameObject.Find("ELBOW_RIGHT")).transform;
		RightWrist=(GameObject.Find("WRIST_RIGHT")).transform;
		RightHand=(GameObject.Find("HAND_RIGHT")).transform;
	
		
		
			// LEFT LEG
		LeftHip=(GameObject.Find("HIP_LEFT")).transform;
		LeftKnee=(GameObject.Find("KNEE_LEFT")).transform;
		LeftAnkle=(GameObject.Find("ANKLE_LEFT")).transform;
		LeftFoot=(GameObject.Find("FOOT_LEFT")).transform;
		
		
			// RIGHT LEG
		RightHip=(GameObject.Find("HIP_RIGHT")).transform;
		RightKnee=(GameObject.Find("KNEE_RIGHT")).transform;
		RightAnkle=(GameObject.Find("ANKLE_RIGHT")).transform;
		RightFoot=(GameObject.Find("FOOT_RIGHT")).transform;
		
		// DYNAMIC ASSIGNMET OVER
		
		
		

        transforms[(int)ZigJointId.Head] = Head;
        transforms[(int)ZigJointId.Neck] = Neck;
        transforms[(int)ZigJointId.Torso] = Torso;
        transforms[(int)ZigJointId.Waist] = Waist;
        transforms[(int)ZigJointId.LeftCollar] = LeftCollar;
        transforms[(int)ZigJointId.LeftShoulder] = LeftShoulder;
        transforms[(int)ZigJointId.LeftElbow] = LeftElbow;
        transforms[(int)ZigJointId.LeftWrist] = LeftWrist;
        transforms[(int)ZigJointId.LeftHand] = LeftHand;
        transforms[(int)ZigJointId.LeftFingertip] = LeftFingertip;
        transforms[(int)ZigJointId.RightCollar] = RightCollar;
        transforms[(int)ZigJointId.RightShoulder] = RightShoulder;
        transforms[(int)ZigJointId.RightElbow] = RightElbow;
        transforms[(int)ZigJointId.RightWrist] = RightWrist;
        transforms[(int)ZigJointId.RightHand] = RightHand;
        transforms[(int)ZigJointId.RightFingertip] = RightFingertip;
        transforms[(int)ZigJointId.LeftHip] = LeftHip;
        transforms[(int)ZigJointId.LeftKnee] = LeftKnee;
        transforms[(int)ZigJointId.LeftAnkle] = LeftAnkle;
        transforms[(int)ZigJointId.LeftFoot] = LeftFoot;
        transforms[(int)ZigJointId.RightHip] = RightHip;
        transforms[(int)ZigJointId.RightKnee] = RightKnee;
        transforms[(int)ZigJointId.RightAnkle] = RightAnkle;
        transforms[(int)ZigJointId.RightFoot] = RightFoot;

        // save all initial rotations
        // NOTE: Assumes skeleton model is in "T" pose since all rotations are relative to that pose
        foreach (ZigJointId j in Enum.GetValues(typeof(ZigJointId)))
        {
            if (transforms[(int)j])
            {
                // we will store the relative rotation of each joint from the gameobject rotation
                // we need this since we will be setting the joint's rotation (not localRotation) but we 
                // still want the rotations to be relative to our game object
                initialRotations[(int)j] = Quaternion.Inverse(transform.rotation) * transforms[(int)j].rotation;
				
		}
        }
    }

    void Start()
    {
        // start out in calibration pose
        if (RotateToPsiPose)
        {
            RotateToCalibrationPose();
        }
    }

	void OnGUI () {
		GUI.Label (new Rect (0,0, 100, 30), "Head");
		GUI.Label (new Rect (0, 25, 100, 30), "Neck");
		GUI.Label (new Rect (0, 50, 100, 30), "Spine");
		GUI.Label (new Rect (0, 75, 100, 30), "Shoulder");
		GUI.Label (new Rect (0, 100, 100, 30), "Elbow");
		GUI.Label (new Rect (0, 125, 100, 30), "Wrist");
		GUI.Label (new Rect (0, 150, 100, 30), "Hand");
		GUI.Label (new Rect (0, 175, 100, 30), "Hip");
		GUI.Label (new Rect (0, 200, 100, 30), "Knee");
		GUI.Label (new Rect (0, 225, 100, 30), "Ankle");
		GUI.Label (new Rect (0, 250, 100, 30), "Foot");
		alphaHead = GUI.HorizontalSlider (new Rect (100, 0, 100, 30), alphaHead, 0.0f, 1.0f);
		alphaNeck = GUI.HorizontalSlider (new Rect (100, 25, 100, 30), alphaNeck, 0.0f, 1.0f);
		alphaSpine = GUI.HorizontalSlider (new Rect (100, 50, 100, 30), alphaSpine, 0.0f, 1.0f);
		
		alphaShoulder = GUI.HorizontalSlider (new Rect (100, 75, 100, 30), alphaShoulder, 0.0f, 1.0f);
		alphaElbow = GUI.HorizontalSlider (new Rect (100, 100, 100, 30), alphaElbow, 0.0f, 1.0f);
		alphaWrist = GUI.HorizontalSlider (new Rect (100, 125, 100, 30), alphaWrist, 0.0f, 1.0f);
		alphaHand = GUI.HorizontalSlider (new Rect (100, 150, 100, 30), alphaHand, 0.0f, 1.0f);
		
		
		alphaHip = GUI.HorizontalSlider (new Rect (100, 175, 100, 30), alphaHip, 0.0f, 1.0f);
		alphaKnee = GUI.HorizontalSlider (new Rect (100, 200, 100, 30), alphaKnee, 0.0f, 1.0f);
		alphaAnkle = GUI.HorizontalSlider (new Rect (100, 225, 100, 30), alphaAnkle, 0.0f, 1.0f);
		alphaFoot = GUI.HorizontalSlider (new Rect (100, 250, 100, 30), alphaFoot, 0.0f, 1.0f);
	
	
betaHead = GUI.HorizontalSlider (new Rect (220, 0, 100, 30), betaHead, 0.0f, 1.0f);
		betaNeck = GUI.HorizontalSlider (new Rect (220, 25, 100, 30), betaNeck, 0.0f, 1.0f);
		betaSpine = GUI.HorizontalSlider (new Rect (220, 50, 100, 30), betaSpine, 0.0f, 1.0f);
		
		betaShoulder = GUI.HorizontalSlider (new Rect (220, 75, 100, 30), betaShoulder, 0.0f, 1.0f);
		betaElbow = GUI.HorizontalSlider (new Rect (220, 100, 100, 30), betaElbow, 0.0f, 1.0f);
		betaWrist = GUI.HorizontalSlider (new Rect (220, 125, 100, 30), betaWrist, 0.0f, 1.0f);
		betaHand = GUI.HorizontalSlider (new Rect (220, 150, 100, 30), betaHand, 0.0f, 1.0f);
		
		betaHip = GUI.HorizontalSlider(new Rect (220,175,100,30),betaHip,0.0f,1.0f);
		betaKnee = GUI.HorizontalSlider (new Rect (220, 200, 100, 30), betaKnee, 0.0f, 1.0f);
		betaAnkle = GUI.HorizontalSlider (new Rect (220, 225, 100, 30), betaAnkle, 0.0f, 1.0f);
		betaFoot = GUI.HorizontalSlider (new Rect (220, 250, 100, 30), betaFoot, 0.0f, 1.0f);

		//hSliderValue1 = GUI.HorizontalSlider (new Rect (100, 275, 100, 30), hSliderValue1, 0.0f, 1.0f);
	}
	
    void UpdateRoot(Vector3 skelRoot)
    {
        // +Z is backwards in OpenNI coordinates, so reverse it
        rootPosition = Vector3.Scale(new Vector3(skelRoot.x, skelRoot.y, skelRoot.z), doMirror(Scale)) + PositionBias;
        if (UpdateRootPosition)
        {
            transform.localPosition = (transform.rotation * rootPosition);
        }
    }

	
	Quaternion DoubleExSmooth(ZigJointId joint,Quaternion CurrentRotation,float alpha,float beta)
	{
		//Quaternion ForeCastedRotation  ;
		
		if (count==103){
		
		
			
			//fOR THE SMOOTH ROTATION PART
			currentSmoothedValue[(int)joint].x = alpha*CurrentRotation.x+(1-alpha)*(previousSmoothedValue[(int)joint].x+previousTrendValue[(int)joint].x);
			
			currentSmoothedValue[(int)joint].y = alpha*CurrentRotation.y+(1-alpha)*(previousSmoothedValue[(int)joint].y+previousTrendValue[(int)joint].y);
		
			currentSmoothedValue[(int)joint].z = alpha*CurrentRotation.z+(1-alpha)*(previousSmoothedValue[(int)joint].z+previousTrendValue[(int)joint].z);
		
		
			//FOR TREND ROTATION PART
			
			currentTrendValue[(int)joint].x = beta*(currentSmoothedValue[(int)joint].x-previousSmoothedValue[(int)joint].x)+(1-beta)*previousTrendValue[(int)joint].x;
			currentTrendValue[(int)joint].y = beta*(currentSmoothedValue[(int)joint].y-previousSmoothedValue[(int)joint].y)+(1-beta)*previousTrendValue[(int)joint].y;
			currentTrendValue[(int)joint].z = beta*(currentSmoothedValue[(int)joint].z-previousSmoothedValue[(int)joint].z)+(1-beta)*previousTrendValue[(int)joint].z;
			
			// Next ForeCasted rotation
			
			ForeCastedRotation.x =currentSmoothedValue[(int)joint].x+currentTrendValue[(int)joint].x;
			ForeCastedRotation.y =currentSmoothedValue[(int)joint].y+currentTrendValue[(int)joint].y;
			ForeCastedRotation.z =currentSmoothedValue[(int)joint].z+currentTrendValue[(int)joint].z;
			//ForeCastedRotation.x =currentSmoothedValue[(int)joint].x;
			//ForeCastedRotation.y =currentSmoothedValue[(int)joint].y;
			//ForeCastedRotation.z =currentSmoothedValue[(int)joint].z;
			
			previousSmoothedValue[(int)joint]=currentSmoothedValue[(int)joint];
		previousTrendValue[(int)joint]=currentTrendValue[(int)joint];
			
			return (ForeCastedRotation);
			
	}	}
	
	
	
	
    void UpdateRotation(ZigJointId joint, Quaternion orientation)
    {		
		
		
	
	joint = mirror ? mirrorJoint(joint) : joint;
		// make sure something is hooked up to this joint
        if (!transforms[(int)joint])
        {
            return;
        }

        if (UpdateOrientation)
        {
            	if(count>102)
				{
					
					if((int)joint==(int)ZigJointId.RightElbow ||  (int)joint==(int)ZigJointId.LeftElbow)
					 orientation=	DoubleExSmooth(joint,orientation,alphaElbow,betaElbow);
				     else 
					 if((int)joint==(int)ZigJointId.LeftHip ||  (int)joint==(int)ZigJointId.RightHip)
					 orientation=	DoubleExSmooth(joint,orientation,alphaHip,betaHip);
					 else
					 if((int)joint==(int)ZigJointId.RightKnee ||  (int)joint==(int)ZigJointId.LeftKnee)
								 orientation=	DoubleExSmooth(joint,orientation,alphaKnee,betaKnee);
					 else
					 if((int)joint==(int)ZigJointId.RightAnkle ||  (int)joint==(int)ZigJointId.LeftAnkle)
								 orientation=	DoubleExSmooth(joint,orientation,alphaAnkle,betaAnkle);
					 else
					 if((int)joint==(int)ZigJointId.RightFoot ||  (int)joint==(int)ZigJointId.LeftFoot)
							 orientation=	DoubleExSmooth(joint,orientation,alphaFoot,betaFoot);
					 else
					 if((int)joint==(int)ZigJointId.RightShoulder ||  (int)joint==(int)ZigJointId.LeftShoulder)
								 orientation=	DoubleExSmooth(joint,orientation,alphaShoulder,betaShoulder);
					 else
					 if((int)joint==(int)ZigJointId.RightWrist ||  (int)joint==(int)ZigJointId.LeftWrist)
								 orientation=	DoubleExSmooth(joint,orientation,alphaWrist,betaWrist);
					 else
					 if((int)joint==(int)ZigJointId.RightHand ||  (int)joint==(int)ZigJointId.LeftHand)
								 orientation=	DoubleExSmooth(joint,orientation,alphaHand,betaHand);
					 else
					 if((int)joint==(int)ZigJointId.Torso )
								 orientation=	DoubleExSmooth(joint,orientation,alphaSpine,betaSpine);
					else
					 if((int)joint==(int)ZigJointId.Neck )
								 orientation=	DoubleExSmooth(joint,orientation,alphaNeck,betaNeck);		 
					else
					 if((int)joint==(int)ZigJointId.Head )
								 orientation=	DoubleExSmooth(joint,orientation,alphaHead,betaHead);
						
					
					
					}

			
			Quaternion newRotation = transform.rotation * orientation * initialRotations[(int)joint];            
			if (mirror)
            {
                newRotation.y = -newRotation.y;
                newRotation.z = -newRotation.z;
            }
	
			//  CALLING THE SMOOTHNING FUNCTION
		
            transforms[(int)joint].rotation = Quaternion.Slerp(transforms[(int)joint].rotation, newRotation, Time.deltaTime * RotationDamping);

			//Debug.Log("rotaion updated " + transforms[(int)joint].rotation.ToString("F4"));// , transforms[(int)joint].rotation.y,transforms[(int)joint].rotation.z );							Debug.Log ("Outputing");
			Debug.Log ("joingID: " + joint.ToString());

			if((int)joint ==TrackedRightElbow)
			{
			Debug.Log ("Outputing");
			System.IO.File.AppendAllText("C:/Users/prp/Desktop/TrackedRightElbow.txt",transforms[(int)joint].rotation.ToString("F4")+"\n");
			//System.IO.File.AppendAllText("C:/Users/prp/Desktop/a.txt","\n");
		}
			if((int)joint ==TrackedLeftHip)
			{
			Debug.Log ("Outputing");
			System.IO.File.AppendAllText("C:/Users/prp/Desktop/TrackedLeftFoot.txt",transforms[(int)joint].rotation.ToString("F4")+"\n");
			//System.IO.File.AppendAllText("C:/Users/prp/Desktop/a.txt","\n");
		}
			
		
			
		}
		
    }
	
    Vector3 doMirror(Vector3 vec)
    {
        return new Vector3(mirror ? -vec.x : vec.x, vec.y, vec.z);
    }
    void UpdatePosition(ZigJointId joint, Vector3 position)
    {
        joint = mirror ? mirrorJoint(joint) : joint;
        // make sure something is hooked up to this joint
        if (!transforms[(int)joint])
        {
            return;
        }

        if (UpdateJointPositions)
        {
            Vector3 dest = Vector3.Scale(position, doMirror(Scale)) - rootPosition;
            transforms[(int)joint].localPosition = Vector3.Lerp(transforms[(int)joint].localPosition, dest, Time.deltaTime * Damping);
        }
    }

    public void RotateToCalibrationPose()
    {
        foreach (ZigJointId j in Enum.GetValues(typeof(ZigJointId)))
        {
            if (null != transforms[(int)j])
            {
                transforms[(int)j].rotation = transform.rotation * initialRotations[(int)j];
            }
        }

        // calibration pose is skeleton base pose ("T") with both elbows bent in 90 degrees
        if (null != RightElbow)
        {
            RightElbow.rotation = transform.rotation * Quaternion.Euler(0, -90, 90) * initialRotations[(int)ZigJointId.RightElbow];
        }
        if (null != LeftElbow)
        {
            LeftElbow.rotation = transform.rotation * Quaternion.Euler(0, 90, -90) * initialRotations[(int)ZigJointId.LeftElbow];
        }
    }

    public void SetRootPositionBias()
    {
        this.PositionBias = -rootPosition;
    }

    public void SetRootPositionBias(Vector3 bias)
    {
        this.PositionBias = bias;
    }

    void Zig_UpdateUser(ZigTrackedUser user)
    {
		
        UpdateRoot(user.Position);
        if (user.SkeletonTracked)
        {
			if (count<101)
			count++;
			if(count==101)
				{
					foreach (ZigInputJoint joint in user.Skeleton)
							{
								if (joint.GoodPosition) UpdatePosition(joint.Id, joint.Position);
								
								
									
								previousTrendValue[(int)joint.Id].x=joint.Rotation.x;
								previousTrendValue[(int)joint.Id].y=joint.Rotation.y;	
								previousTrendValue[(int)joint.Id].z=joint.Rotation.z;
									
								
								if (joint.GoodRotation) UpdateRotation(joint.Id, joint.Rotation);
							}
						count=102;
			}
			else if(count==102)
				{
					foreach (ZigInputJoint joint in user.Skeleton)
					{
						if (joint.GoodPosition) UpdatePosition(joint.Id, joint.Position);
						
						
						previousSmoothedValue[(int)joint.Id].x=joint.Rotation.x;
						previousSmoothedValue[(int)joint.Id].y=joint.Rotation.y;
						previousSmoothedValue[(int)joint.Id].z=joint.Rotation.z;
						
						previousTrendValue[(int)joint.Id].x=joint.Rotation.x-previousTrendValue[(int)joint.Id].x;
						previousTrendValue[(int)joint.Id].y=joint.Rotation.y-previousTrendValue[(int)joint.Id].y;	
						previousTrendValue[(int)joint.Id].z=joint.Rotation.z-previousTrendValue[(int)joint.Id].z;	
					
						if (joint.GoodRotation) UpdateRotation(joint.Id, joint.Rotation);
					}
				
				count=103;
			}
			
			else
				 foreach (ZigInputJoint joint in user.Skeleton)
					{
						if (joint.GoodPosition) UpdatePosition(joint.Id, joint.Position);					
						
						if (joint.GoodRotation) UpdateRotation(joint.Id, joint.Rotation);
					}
				
			
		}
    }

}
