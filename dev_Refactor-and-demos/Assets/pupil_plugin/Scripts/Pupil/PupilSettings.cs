﻿using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NetMQ;
using NetMQ.Sockets;

[CreateAssetMenu (fileName = "PupilSettings")]
public class PupilSettings:ScriptableObject
{
	static PupilSettings _instance = null;

	public static PupilSettings Instance
	{
		get
		{
			if (_instance == null)
				_instance = PupilTools.pupilSettings;
			return _instance;
		}
	}

	public enum EStatus
	{
		Idle,
		ProcessingGaze,
		Calibration
	}



	[Serializable]
	public struct CalibrationType
	{
		public string name;
		public string pluginName;
		public string positionKey;
		public double[] ref_data;
		public float depth;
//		public List<float[]> calibPoints;
//		public float[] center;
		public float radius;
		public float points;
	}

	[Serializable]
	public class Marker
	{
		public string name;
		public Vector3 position;
		public bool calibrationPoint;
		public PupilSettings.Calibration.CalibMode calibMode;
		private GameObject gameObject;
		private Camera _camera;
		public Camera camera
		{
			get
			{
				if (_camera == null)
				{
					_camera = Camera.main;
				}
				return _camera;
			}
			set
			{
				_camera = value;
			}
		}

		public void UpdatePosition(Vector2 newPosition)
		{
			UpdatePosition (newPosition.x, newPosition.y);
		}

		public void UpdatePosition(float newX, float newY)
		{
			position.x = newX;
			position.y = newY;
			position.z = Instance.calibration.currentCalibrationType.depth;

			if (gameObject == null)
				InitializeGameObject ();
			gameObject.transform.position = camera.ViewportToWorldPoint(position);
		}

		public void SetMaterialColor(Color color)
		{
			if (gameObject == null)
				InitializeGameObject ();

			var material = gameObject.GetComponent<MeshRenderer> ().sharedMaterial;
			if (material == null)
				material = new Material (Resources.Load<Material> ("MarkerMaterial"));
			material.SetColor("_EmissionColor",color);
		}

		private void InitializeGameObject()
		{
			gameObject = GameObject.Instantiate (Resources.Load<GameObject> ("MarkerObject"));
			gameObject.name = name;
			gameObject.GetComponent<MeshRenderer> ().sharedMaterial = new Material (Resources.Load<Material> ("MarkerMaterial"));
			//				gameObject.hideFlags = HideFlags.HideInHierarchy;
		}

		public void SetActive(bool toggle)
		{
			if (gameObject == null)
				InitializeGameObject ();

			gameObject.SetActive (toggle);
		}
	}

	[Serializable]
	public class Calibration
	{
		public enum CalibMode
		{
			_2D,
			_3D
		}

		private CalibrationType CalibrationType2D = new CalibrationType () 
		{ 
			name = "2D",
			pluginName = "HMD_Calibration",
			positionKey = "norm_pos",
			ref_data = new double[]{ 0.0, 0.0 },
			depth = 2f,
//			calibPoints = new List<float[]>() {
//				new float[]{0.5f,0.5f},
//				new float[]{0.42f,0.555f},
//				new float[]{0.5f,0.62f},
//				new float[]{0.58f,0.555f},
//				new float[]{0.65f,0.5f},
//				new float[]{0.58f,0.445f},
//				new float[]{0.5f,0.38f},
//				new float[]{0.42f,0.445f},
//				new float[]{0.35f,0.5f},
////				new float[]{0.5f,0.5f},
//			},
//			center = new float[]{0.5f,0.5f},
			radius = 0.08f,
			points = 8
		};

		private CalibrationType CalibrationType3D = new CalibrationType () 
		{ 
			name = "3D",
			pluginName = "HMD_Calibration_3D",
			positionKey = "mm_pos",
			ref_data = new double[]{ 0.0, 0.0, 0.0 },
			depth = 100f,
//			calibPoints = new List<float[]> () {
//				new float[]{ 0f, 0f, 100f },
//				new float[]{ -40, -40, 100f },
//				new float[]{ -40, -0f, 100f },
//				new float[]{ 40, -0f, 100f },
//				new float[]{ -20, -20, 100f },
//				new float[]{ -40, 40, 100f },
//				new float[]{ 0f, 40, 100f },
//				new float[]{ 0f, -40, 100f },
//				new float[]{ -20, 20, 100f },
//				new float[]{ 40, 40, 100f },
//				new float[]{ 20, 20, 100f },
//				new float[]{ 40, -40, 100f },
//				new float[]{ 20, -20, 100f }
////				new float[]{0f,0f, 100f}
//			},
//			center = new float[]{0f,0f,0f},
			radius = 1f,
			points = 10
		};
				

		private CalibMode _currentCalibrationMode;
		public CalibMode currentCalibrationMode
		{
			get
			{
				if (_currentCalibrationMode == null)
					_currentCalibrationMode = CalibMode._3D;	// 3D is standard mode
				return _currentCalibrationMode;
			}
			set
			{
				_currentCalibrationMode = value;
				Debug.Log ("Calibration mode changed to: " + _currentCalibrationMode.ToString ());
			}
		}
		public void SwitchCalibrationMode ()
		{
			if (_currentCalibrationMode == CalibMode._2D)
				_currentCalibrationMode = CalibMode._3D;
			else
				_currentCalibrationMode = CalibMode._2D;
			PupilTools.ResetMarkerVisuals(PupilSettings.Instance.dataProcess.state);
		}	

		public float[] GetCalibrationPoint(int index)
		{
			switch (currentCalibrationMode)
			{
			case CalibMode._2D:
				float[] point = new float[]{0.5f,0.5f};
				if (index > 0 && index < CalibrationType2D.points)
				{	
					point [0] += CalibrationType2D.radius * (float) Math.Cos (2f * Math.PI * (index - 1) / (CalibrationType2D.points-1));
					point [1] += CalibrationType2D.radius * (float) Math.Sin (2f * Math.PI * (index - 1) / (CalibrationType2D.points-1));
				}
				return point;
			default:
				return new float[]{0,0,0};
			}
		}

		public CalibrationType currentCalibrationType
		{
			
			get
			{
				
				if (currentCalibrationMode == CalibMode._2D)
				{
					
					return CalibrationType2D;

				} else
				{
					
					return CalibrationType3D;

				}

			}

		}

		public bool initialized = false;
		public Marker[] CalibrationMarkers;

	}

	[Serializable]
	public class Connection
	{
		public bool isConnected = false;
		public bool isAutorun;
		public string IP;
		public string IPHeader;
		public int PORT;
		public string subport;
		public bool isLocal;
		public List<string> topicList;

		public SubscriberSocket subscribeSocket = null;
	}

	[Serializable]
	public class PupilServiceApp
	{
		public string servicePath;
	}

	[Serializable]
	public class DataProcess
	{
		private EStatus _state;
		public EStatus state
		{
			get { return _state; }
			set { _state = value; }
		}
		public string benchMarkString;
	}

	[Serializable]
	public class CustomGUIVariables
	{
		[Serializable]
		public class Tabs
		{
			
			public int mainTab;

		}

		[Serializable]
		public class Bools
		{

			public bool isAdvanced;
			
		}

		public Tabs tabs;
		public Bools bools;

	}

	[Serializable]
	public class DebugView
	{
		public bool active = false;
	}

	[Serializable]
	public class DebugVars
	{
		public bool printSampling;
		public bool printMessage;
		public bool printMessageType;
	}

	[Serializable]
	public class FramePublishing
	{
		public int targetFPS = 20;
		public Texture2D eye0Image;
		public Texture2D eye1Image;
		[HideInInspector]
		public byte[] raw0;
		[HideInInspector]
		public byte[] raw1;
		[HideInInspector]
		public bool StreamCameraImages = false;
		public Material eye0ImageMaterial;
		public Material eye1ImageMaterial;
	}

	public DebugVars debug;
	public DataProcess dataProcess;
	public Connection connection;
	public PupilServiceApp pupilServiceApp;
	public Calibration calibration;
	public CustomGUIVariables customGUIVariables;
	public DebugView debugView;
	public FramePublishing framePublishing;
	public bool visualizeGaze;

	public List<GUIStyle> GUIStyles;

	public static int numberOfMessages = 6;

	public const int leftEyeID = 1;
	public const string stringForLeftEyeID = "1";
	public const int rightEyeID = 0;
	public const string stringForRightEyeID = "0";
}

