using UnityEngine;
using System.Collections;
using System.Collections.Generic;  //Enables List<>s
using System.Linq;  //Enables LINQ queries

//The MPhase enum is used to track the phase of mouse interaction
public enum MPhase
{
	idle, 
	down, 
	drag
}

//MouseInfo stores information about the mouse in each frame of the itneraction
[System.Serializable]
public class MouseInfo
{
	public Vector3 loc;  //3D loc of the mouse near z=0
	public Vector3 screenLoc;  //Screen position of the mouse
	public Ray ray;  //Ray from the mouse into 3D space
	public float time;  //Time this mouseInfo was recorded
	public RaycastHit hitInfo;  //Info about what was hit by the ray
	public bool hit;  //Whether the mouse was over any collider

	//These methods see if the mouseRay hits anything
	public RaycastHit Raycast()
	{
		hit = Physics.Raycast (ray, out hitInfo);
		return(hitInfo);
	}

	public RaycastHit Raycast(int mask)
	{
		hit = Physics.Raycast (ray, out hitInfo, mask);
		return(hitInfo);
	}
}

//Mage is a subclass of PT_MonoBehaviour
public class Mage : PT_MonoBehaviour {

	static public Mage S;
	static public bool DEBUG = true;

	public float mTapTime = 0.1f;  //How long is considered a tap
	public float mDragDist = 5;  //Min dist in pixels to be a drag

	public float activeScreenWidth = 1;  //% of the screen to use

	public bool ____________________;

	public MPhase mPhase = MPhase.idle;
	public List<MouseInfo> mouseInfos = new List<MouseInfo>();

	void Awake()
	{
		S = this;  //Set the Mage singleton
		mPhase = MPhase.idle;
	}

	void Update()
	{
		//Find wether the mouse button 0 was pressed or released this frame
		bool b0Down = Input.GetMouseButtonDown (0);
		bool b0Up = Input.GetMouseButtonUp (0);

		//Handle all input here (except for Inventory buttons)
		/*
		There are only a few possiable actions:
		1.  Tap on the ground to move to that point
		2.  Drag on the ground with no spell selected to move to the mage
		3.  Drag on the ground with spell to cast along the ground
		4.  Tap on an enemy to attack (or force-ush away without an element)
		*/

		//An example of using < to return a bool value
		bool inActiveArea = (float)Input.mousePosition.x / Screen.width < activeScreenWidth;

		//This is handled as an if statement instead of switch because a tsap can sometimes happen within a single frame
		if (mPhase == MPhase.idle) //If the mosue is idle
		{
			if (b0Down && inActiveArea)
			{
				mouseInfos.Clear();  //Clear the mouseInfos
				AddMouseInfo();  //And add a first MouseInfo

				//If the mouse was clicked on something, it's a valid MouseDown
				if (mouseInfos[0].hit)  //Something was hit!
				{
					MouseDown();  //Call MouseDown()
					mPhase = MPhase.down;  //and set the mPhase
				}
			}
		}

		if (mPhase == MPhase.down)  //if the mouse is down
		{
			AddMouseInfo();  //Add a MouseInfo for this frame
			if (b0Up)
			{
				MouseTap();  //This was a tap
				mPhase = MPhase.idle;
			} else if (Time.time - mouseInfos[0].time > mTapTime) {
				//If it's been down for longer than a tap, this may be a drag, but
				//to be a drag, it must also have moved a certtian number of
				//pixls on screen.
				float dragDist = (lastMouseInfo.screenLoc - mouseInfos[0].screenLoc).magnitude;
				if (dragDist >= mDragDist)
				{
					mPhase = MPhase.drag;
				}
			}
		}

		if (mPhase == MPhase.drag)  //if the mouse is being drug
		{
			AddMouseInfo();
			if (b0Up)
			{
				//The mouse button was released
				MouseDragUp();
				mPhase = MPhase.idle;
			} else {
				MouseDrag();  //Still dragging
			}
		}
	}

	//Pulls info about the Mouse, adds it to MouseInfos, and returns it
	MouseInfo AddMouseInfo()
	{
		MouseInfo mInfo = new MouseInfo ();
		mInfo.screenLoc = Input.mousePosition;
		mInfo.loc = Utils.mouseLoc;  //Gets the position of the mouse at z=0
		mInfo.ray = Utils.mouseRay;  //Gets the ray from the main camera through the mouse mointer

		mInfo.time = Time.time;
		mInfo.Raycast ();  //Default is to raycast with no mask

		if (mouseInfos.Count == 0)
		{
			//If this is the first mouseInfo
			mouseInfos.Add(mInfo);  //Add mInfo to mouseInfos
		} else {
			float lastTime = mouseInfos[mouseInfos.Count-1].time;
			if (mInfo.time != lastTime)
			{
				//if time has passed since the last mouseinfo
				mouseInfos.Add(mInfo);  //Add mInfo to mouseInfos
			}
			//This time test is necessary because AddMouseInfo() could be called twice in one frame
		}
		return(mInfo);  //Return mInfo as well
	}

	public MouseInfo lastMouseInfo
	{
		//Access to the latest MouseInfo
		get
		{
			if (mouseInfos.Count == 0) return(null);
			return(mouseInfos[mouseInfos.Count-1]);
		}
	}

	void MouseDown()
	{
		//The mouse was pressed on something (it could be a drag or a tap)
		if (DEBUG) print("Mage.MouseDown()");
	}

	void MouseTap()
	{
		//Something was tapped like a button
		if (DEBUG) print("Mage.MouseTap()");
	}

	void MouseDrag()
	{
		//SThe mouse is being drug across something
		if (DEBUG) print("Mage.MouseDrag()");
	}

	void MouseDragUp()
	{
		//The mouse is released after being drug
		if (DEBUG) print("Mage.MouseDragUp()");
	}
///////////[Fola] Just testing "Create a New Branch" option //////////////////
}
