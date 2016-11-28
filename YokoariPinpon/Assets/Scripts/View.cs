using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

/****************************************
 Kinectでプレイヤーを認識し、ラケットをうごかす

	選択された利き手にラケットを合わせて動かす
	身長の差による変化が起きないよう腰からの手の位置を計算する
 ****************************************/
public class View : MonoBehaviour 
{
	public static ulong id = 0;			//プレイヤーの番号
	GameObject cube;
	public static float rhandheight;
	public static float lhandheight;
	public static bool active;
	public static bool kikite;


	float sz;

	public Material BoneMaterial;
	public GameObject BodySourceManager;
	
	private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
	private BodySourceManager _BodyManager;
	
	private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
	{
		{ Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
		{ Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
		{ Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
		{ Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
		
		{ Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
		{ Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
		{ Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
		{ Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
		
		{ Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
		{ Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
		{ Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
		{ Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
		{ Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
		{ Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
		
		{ Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
		{ Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
		{ Kinect.JointType.HandRight, Kinect.JointType.WristRight },
		{ Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
		{ Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
		{ Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
		
		{ Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
		{ Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
		{ Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
		{ Kinect.JointType.Neck, Kinect.JointType.Head },
	};
	void Start() {
		//認識がシーン移動で外れないように消えないゲームオブジェクトにする
		DontDestroyOnLoad(this);
		//ラケットを認識する
		cube = GameObject.Find ("raket");
	}
	void Update () 
	{
		//ラケットがシーン移動などで外れたらもう一度ラケットを探す
		if (!cube) {
			cube = GameObject.Find ("raket");
		}
		//Kinectがプレイヤーを認識しているなら
		if (GameObject.Find ("ok")) {
			active = true;
			//プレイヤーの体の腰の座標を取得する
			Transform jointObj = _Bodies [id].transform.FindChild (Kinect.JointType.SpineBase.ToString ()).transform;
			Transform yubi;
			Vector3 sp,sp2;
			Vector3 kakudo = Vector3.zero;
			Vector3 height = jointObj.position;
			//Kinectの認識範囲を制限する
			if(jointObj.position.z <= 27
			   && jointObj.position.x < 6 && 
			   jointObj.position.x > -6) {
				active = true;
			}else {
				active = false;

			}
			//利き手が選択されていないのならプレイヤーの手の高さを渡す
			if(TitleM.handSelect  == false) {
				//active = true;
				rhandheight = _Bodies [id].transform.FindChild (Kinect.JointType.HandRight.ToString ()).transform.position.y;
				lhandheight = _Bodies [id].transform.FindChild (Kinect.JointType.HandLeft.ToString ()).transform.position.y;
				//腰のy座標を引くことで身長による制限をなくす
				rhandheight -= height.y;
				rhandheight -= height.y;
				
			}
			else {
				//利き手に応じて座標を取得する
				if(kikite == true) {
					jointObj = _Bodies [id].transform.FindChild (Kinect.JointType.WristRight.ToString ()).transform;

					yubi = _Bodies [id].transform.FindChild (Kinect.JointType.HandTipRight.ToString ()).transform;
				}else {
					jointObj = _Bodies [id].transform.FindChild (Kinect.JointType.WristLeft.ToString ()).transform;

					yubi = _Bodies [id].transform.FindChild (Kinect.JointType.HandTipLeft.ToString ()).transform;
				}
				//ゲームのプレイする座標にあうように補正をかける
				sp = jointObj.position-new Vector3(0,0,3);
				sp.z *= -1;
				sp.x *= 2f;
				sp.y *= 1.5f;
				sp.y+= 5;
				sp2 = yubi.position-new Vector3(0,0,3);
				sp2.z *= -1;
				sp2.x *= 2f;
				sp2.y *= 1.5f;
				sp2.y+= 5;
				//腰の高さに応じた高さにする
				sp.y += (-height.y-4)-1;
				sp.z += 5;
				//ラケットの角度を実際に合わせる
				kakudo.x = -((sp.y-sp2.y)*90);
				kakudo.z = (sp.x - sp2.x)*45; 
				//ラケットの移動と回転を行う
				if(cube) {
					cube.transform.position = sp;
					cube.transform.localEulerAngles = kakudo;
				}
			}
		} else {
			//認識していないなら初期化
			id = 0;
			active = false;
			//active = false;
		}
		if (BodySourceManager == null)
		{
			return;
		}
		
		_BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
		if (_BodyManager == null)
		{
			return;
		}
		
		Kinect.Body[] data = _BodyManager.GetData();
		if (data == null)
		{
			return;
		}
		
		List<ulong> trackedIds = new List<ulong>();
		foreach(var body in data)
		{
			if (body == null)
			{
				continue;
			}
			
			if(body.IsTracked)
			{
				trackedIds.Add (body.TrackingId);
			}
		}
		
		List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
		
		// First delete untracked bodies
		foreach(ulong trackingId in knownIds)
		{
			if(!trackedIds.Contains(trackingId))
			{
				Destroy(_Bodies[trackingId]);
				_Bodies.Remove(trackingId);
			}
		}
		
		foreach(var body in data)
		{
			if (body == null)
			{
				continue;
			}
			if(id == 0 || id == body.TrackingId) {
				if(body.IsTracked)
				{
					if(!_Bodies.ContainsKey(body.TrackingId))
					{
						_Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
						_Bodies[body.TrackingId].name = "ok";
						DontDestroyOnLoad(_Bodies[body.TrackingId]);
						id = body.TrackingId;
						}
					
					RefreshBodyObject(body, _Bodies[body.TrackingId]);
				}
			}
		}

		/*foreach(var body in data)
		{
			if (body == null)
			{
				continue;
			}
			
			if(body.IsTracked)
			{
				if(!_Bodies.ContainsKey(body.TrackingId))
				{
					Transform jointObj = _Bodies [body.TrackingId].transform.FindChild ("Kinect.JointType.SpineBase");
					Debug.Log (jointObj.name + ":" + jointObj.transform.localPosition);
				}
				
				RefreshBodyObject(body, _Bodies[body.TrackingId]);
			}
		}*/
			
	}

	void douki(string st) {

	}
	private GameObject CreateBodyObject(ulong id)
	{
		GameObject body = new GameObject("Body:" + id);
		
		for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
		{
			GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
			
			LineRenderer lr = jointObj.AddComponent<LineRenderer>();
			lr.SetVertexCount(2);
			lr.material = BoneMaterial;
			lr.SetWidth(0.05f, 0.05f);
			
			jointObj.transform.localScale = new Vector3(0f, 0f, 0f);
			jointObj.name = jt.ToString();
			jointObj.transform.parent = body.transform;

		}

		return body;
	}
	
	private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
	{
		for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
		{
			Kinect.Joint sourceJoint = body.Joints[jt];
			Kinect.Joint? targetJoint = null;
			
			if(_BoneMap.ContainsKey(jt))
			{
				targetJoint = body.Joints[_BoneMap[jt]];
			}
			
			Transform jointObj = bodyObject.transform.FindChild(jt.ToString());
			jointObj.localPosition = GetVector3FromJoint(sourceJoint);

			LineRenderer lr = jointObj.GetComponent<LineRenderer>();
			if(targetJoint.HasValue)
			{
				lr.SetPosition(0, jointObj.localPosition);
				lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
				lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
			}
			else
			{
				lr.enabled = false;
			}
		}
	}
	
	private static Color GetColorForState(Kinect.TrackingState state)
	{
		switch (state)
		{
		case Kinect.TrackingState.Tracked:
			return Color.green;
			
		case Kinect.TrackingState.Inferred:
			return Color.red;
			
		default:
			return Color.black;
		}
	}
	
	private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
	{
		return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
	}
}
