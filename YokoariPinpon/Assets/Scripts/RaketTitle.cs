using UnityEngine;
using System.Collections;
		
/****************************************
	タイトルでのラケットの移動

	WASDキーで前後左右、矢印キー上下で上下に移動します
	ボールに当たるとボールの移動量をZ方向に加算します
*****************************************/
public class RaketTitle : MonoBehaviour {
	Vector3 pos;						//座標
	public static Vector3 movePower;	//打つ強さ
	GameObject ballp;					//球
	Vector3 sca;						//当たり判定の大きさ
	// Use this for initialization
	void Start () {
		BallTitle.pos = new Vector3 (0, 2, -4);
		BallTitle.vect = Vector3.zero;

		ballp = GameObject.Find ("ball");
		sca = transform.localScale*4;
		pos = Vector3.zero;
		movePower = new Vector3(0,0,0.1f);
	}
			
	// Update is called once per frame
	void Update () {
		//モデルの切り替え
		if(change_model()) {
			
			model_set();
		}
		pos = transform.position;

		//ラケットの移動
		move ();

		//ボールに当たったらまっすぐに飛ばす
		if (hit (ballp.transform.position, ballp.transform.localScale)) {
			BallTitle.vect = movePower;
		}
				
	
		transform.position = pos;
	}
	bool hit(Vector3 p,Vector3 s) {
		bool ret = false;
		if (pos.x + sca.x/2 > p.x - s.x/2 && 
		    pos.x - sca.x/2 < p.x + s.x/2 && 
		    pos.y + sca.y/2 > p.y - s.y/2 && 
		    pos.y - sca.y/2 < p.y + s.y/2 && 
		    pos.z + sca.z/2 > p.z - s.z/2 && 
		    pos.z - sca.z/2 < p.z + s.z/2) {
			ret =true;
		}
		return ret;
	}
/***************************************
	モデルを変更する関数
	Z、X、C、キーを押すとモデルが切り替わります
 **************************************/
	bool change_model() {
		bool ret = true;
		if (Input.GetKeyDown (KeyCode.Z)) {
			GM.raketnum = 0;
		} else if (Input.GetKeyDown (KeyCode.X)) {
			GM.raketnum = 1;
		} else if (Input.GetKeyDown (KeyCode.C)) {
			GM.raketnum = 2;
		} else {
			ret = false;
		}
		return ret;
	}
/***************************************
	モデルを切り替える処理
 **************************************/
	void model_set() {
		if (transform.childCount > 0) {
			if (GM.raketnum == 0) {
				transform.FindChild ("srippa").gameObject.SetActive (false);
				transform.FindChild ("shamoji").gameObject.SetActive (false);
				transform.FindChild ("raket (1)").gameObject.SetActive (true);
				
			}
			if (GM.raketnum == 1) {
				transform.FindChild ("shamoji").gameObject.SetActive (true);
				transform.FindChild ("srippa").gameObject.SetActive (false);
				transform.FindChild ("raket (1)").gameObject.SetActive (false);
				
			}
			if (GM.raketnum == 2) {
				transform.FindChild ("shamoji").gameObject.SetActive (false);
				transform.FindChild ("srippa").gameObject.SetActive (true);
				transform.FindChild ("raket (1)").gameObject.SetActive (false);
				
			}
		}
	}
	void move() {
		if (Input.GetKey (KeyCode.W)) {
			pos.z += 0.1f;
		}
		if (Input.GetKey (KeyCode.S)) {
			pos.z += -0.1f;
		}
		if (Input.GetKey (KeyCode.D)) {
			pos.x += 0.1f;
		}
		if (Input.GetKey (KeyCode.A)) {
			pos.x += -0.1f;
		}
		if (Input.GetKey (KeyCode.UpArrow)) {
			pos.y += 0.1f;
		}
		if (Input.GetKey (KeyCode.DownArrow)) {
			pos.y += -0.1f;
		}
	}
}

