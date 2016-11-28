using UnityEngine;
using System.Collections;

/****************************************
タイトル画面でボールが当たった時に回転してからゲームに進む

ボールが当たると回転を始め、一定角度回転するとゲームの画面に進みます
Kinectがプレイヤーを認証していなければたまにエフェクトとして回転させる
******************************************/
public class ModeSelect : MonoBehaviour {
	public bool ok;					//ボールが当たった
	public int number;				//進むモードの番号

	Vector3 pos;					//当たり判定のための座標
	Vector3 sca;					//当たり判定のためのサイズ

	float deg = 0;					//回転する角度
	float sum = 0;					//回転している角度の合計
	void Start () {
		pos = transform.position;
		sca = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		//もしもKinectが認識していないなら
		if (View.active == false) {
			//0.1%の確率でエフェクトとして回転させる
			if(Random.Range (0,1000) >= 999) {
				deg= 10;
				sum = 0;
				transform.localEulerAngles = Vector3.zero;
			}
		}
		//ボールに当たったら回転させる
		if(hit (BallTitle.pos,new Vector3(1,1,1))) {
			deg= 10;
			sum = 0;
			transform.localEulerAngles = Vector3.zero;
			//モードを切り替えるフラグをonにする
			ok = true;
		}
		//回転する角度が1以上の時
		if (deg > 0) {
			//回転させる
			transform.localEulerAngles += new Vector3 (0, deg, 0);
			//回転した合計の角度を求める
			sum += deg;
			//ある程度回転したら回転する角度を弱める
			if (sum > 2000) {
				deg -= 0.02f;
			}
			//一定量回転したら止まる
			if (sum >= 4320) {
				//初期化
				deg = 0;
				//ボールが当たったのなら
				if(ok) {
					//ゲームの指定されているモードに飛ぶ
					GM.mode = number;
					int fr = 80;
					if(number == 1)fr = 90;
					else if(number == 2) fr = 100;
					Application.targetFrameRate = fr;
					TitleM.audio.Stop();

					Application.LoadLevel("test");
				}
			}
			//deg -= 0.01f;
			//Debug.Log (sum);
		} else {
			//回転しないのなら初期化する
			sum = 0;
			ok = false;
		}
	}

	//当たり判定
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
}
