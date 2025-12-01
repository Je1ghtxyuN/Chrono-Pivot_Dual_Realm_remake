using UnityEngine;
using System.Collections;

public class ten_demo_ControllerFreeCamera : MonoBehaviour {

	//PUBLIC VARIABLES
	public bool isActive = false;
	public bool  isControllable = true;
	public Transform cameraTarget;
	public bool  reverseYAxis = true;
	public bool  reverseXAxis = false;
	public float cameraFOV = 35.0f;
	public Vector3 rotationLimits = Vector3.zero;

	//PRIVATE VARIABLES
	private Transform cameraObject;
	private Vector2 axisSensitivity = new Vector2(4.0f,4.0f);
	private float camRotation = 0.0f;
	private Vector3 camRot;
	private float camHeight = 0.0f;
	private float MouseRotationDistance = 0.0f;
	private float MouseVerticalDistance = 0.0f;
	private bool orbitView = false;
	private Vector3 newRotation;
	private float oldMouseRotation;
	private float oldMouseVRotation;
	private ten_demo_ControllerMaster MC;
	private ten_demo_InputController IC;



	void Awake(){
		MC = this.gameObject.GetComponent<ten_demo_ControllerMaster>() as ten_demo_ControllerMaster;
		IC = this.gameObject.GetComponent<ten_demo_InputController>() as ten_demo_InputController;
		cameraTarget = MC.cameraObject;

	}




	void Update(){

		if (isActive){
			//------------------------------------
			//  GET DATA FROM MASTER CONTROLLER
			//------------------------------------
			cameraObject = MC.cameraObject;
			
			
			//---------------------------------
			//  GET KEYBOARD AND MOUSE INPUTS
			//---------------------------------
			if (isControllable){

				if (Input.mousePosition.x > 365f || Input.mousePosition.y < 335f){
					orbitView = IC.inputMouseKey0;
				} else {
					orbitView = false;
				}
			}

			//CHECK FOR MOUSE INPUT
			oldMouseRotation = MouseRotationDistance;
			oldMouseVRotation = MouseVerticalDistance;
			
			//GET MOUSE MOVEMENT
			MouseRotationDistance = IC.inputMouseX;
			MouseVerticalDistance = IC.inputMouseY;
			if (reverseXAxis) MouseRotationDistance = -IC.inputMouseX;
			if (reverseYAxis) MouseVerticalDistance = -IC.inputMouseY;
			
			
			
			if (isControllable){

				//---------------------------------
				//  CAMERA POSITIONING
				//---------------------------------

				// Calculate Rotation
				newRotation = cameraObject.transform.eulerAngles;

				if (orbitView){
					camHeight = Mathf.Lerp(oldMouseVRotation,MouseVerticalDistance*axisSensitivity.y,Time.deltaTime);
					camRotation = Mathf.Lerp(oldMouseRotation,MouseRotationDistance*axisSensitivity.x,Time.deltaTime);
					newRotation.x += camHeight;
					newRotation.y += camRotation;
				}

				//Set Rotations
				cameraObject.transform.eulerAngles = newRotation;

			}
			
			
			//---------------------------------
			//  SET CAMERA SETTINGS and FX
			//---------------------------------
			if (isControllable){
				cameraObject.GetComponent<Camera>().fieldOfView = cameraFOV;
			}

		}

	}


}
