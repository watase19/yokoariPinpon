using UnityEngine;
using System.Collections;
		
/*******************************************************************
 ラケットの移動とボールとの当たり判定のクラス

	WASDキーで前後左右、矢印キー上下で上下に移動します

	自分の陣地で1回バウンドするとボールに当たるようになります

	ラケットを動かすと力が加算され、ボールに当たると
	現在の力に応じた移動量をボールに与えます

	ボールの移動量は卓球台の中心に行く量から振った力の強さによって増減します
	
 *******************************************************************/
public class Raket : MonoBehaviour {

	public float ai_Speed = 0.2f;		//ヨコアリくんのラケットを動かすスピード

	public static bool ball_hitflg;		//ボールに当たったかどうか（台にボールがバウンドするとリセットされます）

	public bool player_flg;				//プレイヤーかAIかのフラグ
	public GameObject yokoariBase;		//ヨコアリくんの外枠（座標を扱う）
	public GameObject yokoari;			//ヨコアリくんの中身（アニメーションを行う)

	GameObject ball;					//ボールのゲームオブジェクト
	GameObject bord;					//卓球台のゲームオブジェクト

	Vector3 pos;						//現在の座標
	Vector3 pos_before;					//1フレーム前の座標
	Vector3 scale;						//ラケットの当たり判定のサイズ
	Vector3 point;						//ボールを落とす目的地点（卓球台の相手陣地の中央）
	Vector3 movePower;					//振った力

	float si,co,kaku;					//ボールの目標地点への方向
	float sa;							//ボールの目標地点への距離
	float tim;							//時間計測のための変数

	Animator animator;					//ヨコアリくんのアニメーター		

	// Use this for initialization
	void Start () {
		//モデルの読み込み
		bord = GameObject.Find ("dai");
		ball = GameObject.Find ("ball");

		//ボールの目標地点の設定
		if (player_flg) {
			point.z = bord.transform.position.z + bord.transform.localScale.z / 4;
			point.x = 0;
		} else {
			point.z = bord.transform.position.z - bord.transform.localScale.z / 4;
			point.x = 0;
			//アニメーションの設定
			animator = yokoari.GetComponent<Animator> ();
		}

		//初期化
		scale= transform.localScale*2;
		pos = Vector3.zero;
		pos_before = pos;
		movePower = Vector3.zero;
		ball_hitflg = false;	
		Ball.raketHitFlag = false;
		model_set();
	}
			
	// Update is called once per frame
	void Update () {
		//モデルの切り替え
		if(change_model()) {
			
			model_set();
		}
		//座標変数を更新
		pos = transform.position;

		//ラケットの移動
		move ();

		//移動している方向と反対に移動したら移動量を0にする
		revercePower ();

		//撃つ強さを加算
		movePower.x += pos.x - pos_before.x;
		movePower.y += pos.y - pos_before.y;
		movePower.z += pos.z - pos_before.z;

		//ラケットが動いていなかったら移動量を0にする
		stopPower ();

		//卓球台に当たったら上にどかす
		if (hit (bord.transform.position, bord.transform.localScale)) {
			pos.y = bord.transform.position.y + bord.transform.localScale.y/2
				+transform.localScale.y/2+0.1f;
		}

		//ボールに当たった時の処理
		if (hit (ball.transform.position, ball.transform.localScale)) {
			//ボールが台より下にあるとき上に戻す
			Vector3 bp= ball.transform.position;
			if(bp.y-ball.transform.localScale.y/2
			   < bord.transform.position.y + bord.transform.position.y/2) {
				bp.y = ball.transform.localScale.y/2
					+bord.transform.position.y + bord.transform.localScale.y/2+0.01f;
				ball.transform.position = bp;
			}

			//ボールが自分の陣地でバウンドしてから初めて当たったとき
			if (ball_hitflg==false && GM.bounds == player_flg) {

				//ラリーモードならプレイヤーのラケットに10回当たればクリア
				if(GM.mode == 0) {
					if(player_flg ) {
						//ラリーのスコアを足し、スコアボードをめくる
						GM.raricnt++;
						Debug.Log (GM.raricnt);
						Mekuru.sta (false);
						//クリア処理
						if(GM.raricnt >= 10) {
							Application.LoadLevel("Rally");

						}

					}
				}
				//ボールに音を鳴らさせる
				Ball.raketHitFlag = true;

				//ボールからの目標地点への方向を求める
				kaku = Mathf.Atan2 (ball.transform.position.x - point.x , ball.transform.position.z - point.z)
					*180/3.14f+180;

				si = Mathf.Sin(kaku * 3.14f/180);
				co = Mathf.Cos(kaku * 3.14f/180);



				//目標地点への距離を計算
				sa = Mathf.Sqrt((ball.transform.position.x - point.x) *
				                (ball.transform.position.x - point.x)+
				                (ball.transform.position.z - point.z) *
				                (ball.transform.position.z - point.z));

				//難しいモードでないとき打つ強さに補正をかける
				if(GM.mode != 2)
				movePower.z = 0.4f;


				//撃つ力の調節
				movePower = justingPower(movePower);

				//ヨコアリくんのAI
				yokoariAI();

				//ヨコアリくんに振るモーションをさせる
				if(player_flg == false) {
					animator.Play ("s", 0);
				}

				//着地するまでの処理回数を計算
				float cnt = landingCount();
				Debug.Log (cnt);

				//目標地点へ到達する為の移動量に、振った力分の補正をかけた移動量をボールに与える
				Ball.vect = new Vector3(sa/(cnt * 7) *(movePower.x),movePower.y*0.1f,co * sa/(cnt * 7)*(movePower.z/0.4f));

				//ボールの反射
				float v= Ball.vect.z;
				if(movePower.z == 0) {
					Ball.vect.z += -0.5f*v;

				}
				//ボールに当たった状態にする
				ball_hitflg = true;
			}
			Ball.cnt = 0;
		}
		//前座標変数の更新
		pos_before = transform.position;

		//座標を更新
		transform.position = pos;
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
	当たり判定
 **************************************/
	bool hit(Vector3 p,Vector3 s) {
		bool ret = false;
		if (pos.x + scale.x/2 > p.x - s.x/2 && 
		    pos.x - scale.x/2 < p.x + s.x/2 && 
		    pos.y + scale.y/2 > p.y - s.y/2 && 
		    pos.y - scale.y/2 < p.y + s.y/2 && 
		    pos.z + scale.z/2 > p.z - s.z/2 && 
		    pos.z - scale.z/2 < p.z + s.z/2) {
			ret =true;
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
/***************************************
	移動処理
 **************************************/
	void move() {
		if (player_flg) {//プレイヤー
			
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
		} else {//COM
			
			if(GM.mode == 1) ai_Speed = 0.1f;
			if(ball.transform.position.z > 5) {
				if(ball.transform.position.x > pos.x+scale.x/2 ){
					pos.x+=ai_Speed;
				}
				else if(ball.transform.position.x < pos.x-scale.x/2 ){
					pos.x-=ai_Speed;
				}
				if(ball.transform.position.y > pos.y+scale.y/2 ){
					pos.y+=ai_Speed;
				}
				else if(ball.transform.position.y < pos.y-scale.y/2 ){
					pos.y-=ai_Speed;
				}
				if(GM.mode != 0) {
					if(ball.transform.position.z > pos.z+scale.z/2 ){
						pos.z+=0.05f;
					}
					else if(ball.transform.position.z < pos.z-scale.z/2 ){
						pos.z-=0.05f;
					}
				}
			}else {
				if(pos.z < 18)
					pos.z += 0.05f;
			}
			//ヨコアリくんの座標更新
			yokoariBase.transform.position = new Vector3(yokoariBase.transform.localScale.x + pos.x,-2,20);
		}
	}
/***************************************
	移動の向きが変わった時に移動量を0にする
 **************************************/
	void revercePower() {
		if (movePower.x > 0) {
			if(pos.x < pos_before.x){
				movePower.x = 0;
			}
		}
		else if (movePower.x < 0) {
			if(pos.x > pos_before.x){
				movePower.x = 0;
			}
		}
		if (movePower.y > 0) {
			if(pos.y < pos_before.y){
				movePower.y = 0;
			}
		}
		else if (movePower.y < 0) {
			if(pos.y > pos_before.y){
				movePower.y = 0;
			}
		}
		if (movePower.z > 0) {
			if(pos.z < pos_before.z){
				movePower.z = 0;
			}
		}
		else if (movePower.z < 0) {
			if(pos.z > pos_before.z){
				movePower.z = 0;
			}
		}
	}
/***************************************
	移動が止まった時、移動量を0にする
 **************************************/
	void stopPower() {
		if (pos.x- 0.05f <  pos_before.x && pos.x + 0.05f >  pos_before.x && 
		    pos.y- 0.05f <  pos_before.y && pos.y + 0.05f >  pos_before.y && 
		    pos.z- 0.05f <  pos_before.z && pos.z + 0.05f >  pos_before.z) {
			tim += Time.deltaTime;
			if(tim > 0.3f) //一定時間がたった時に0にする（すぐに0にすると振り切った時に止まってしまっているかもしれないため）
				movePower = Vector3.zero;
		} else {
			tim = 0;
		}
	}
/***************************************
	力を調節する
 **************************************/
	Vector3 justingPower(Vector3 power) {
		Vector3 mP = power;
		
		if(mP.y > -0.3f) {
			mP.y=1.7f;
		}else {
			mP.y = -1f;
			mP.y-=0.5f;
			mP.z += 0.5f;
		}
		if(mP.y > 4) {
			mP.y = 4;
		}
		if(player_flg) {
			if(mP.y > -0.5f) {
				if(mP.z > 0.39f-(mP.y-1.4f)/15) {
					mP.z = 0.39f-(mP.y-1.4f)/15;
				}else if(mP.z <  0.39f-((mP.y-1.4f)/15)-0.11f) {
					mP.z = 0.39f-((mP.y-1.4f)/15)-0.11f;
				}
			}else {
				mP.y -= 1.5f;
				mP.z = 1.2f;
				//Debug.Log (mP);
			}
		}
		mP.x = mP.x* 0.5f;
		if(mP.x > 0.3f) {
			mP.x = 0.3f;
		}else if(mP.x < -0.3f){
			
			mP.x = -0.3f;
		}
		
		mP.z *= 0.9f;
		return mP;
	}
	
/***************************************
	ヨコアリのAI
 **************************************/
	void yokoariAI() {
		if(player_flg == false) {
			if(GM.mode == 0) {
				float ran = 0;
				float y;
				y = 1.4f;
				movePower = new Vector3(si,y,0.3f-ran);
			}
			else if(GM.mode  == 1) {
				float ran = Random.Range(0,2)*0.1f;
				float ranx = si  + (float)(Random.Range(0,20)-10)/80;
				float y;
				y = 1.4f +  Random.Range(5,10)/10*2;
				if(y - 1.4f > 1.3f) {
					ran = 0;
				}
				ran += (y-1.4f)/12;
				movePower = new Vector3(ranx,y,0.4f-ran);
			}
			else if(GM.mode == 2) {
				float ran = Random.Range(0,2)*0.1f;
				float ranx = si + (float)(Random.Range(0,20)-10)/60;
				float y;
				if(ball.transform.position.y > 5) {
					y = -0.4f;
					ran -= 0.4f;
				}else {
					y = 1.4f + Random.Range(5,14)/10*2;
					if(y - 1.4f > 1.3f) {
						ran = 0;
					}
					ran += (y-1.4f)/12;
				}
				movePower = new Vector3(ranx,y,0.4f-ran);
				//movePower = new Vector3(0,1.4f,0.4f);
				
			}
		}
	}
/***************************************
	着地まで何回処理をするかをカウント
 **************************************/
	float landingCount() {
		int cnt = 0;
		float boundVectorY = movePower.y;
		float boundHeight = ball.transform.position.y;
		float boundtim = 0;
		while(!(boundHeight - ball.transform.localScale.y/2 < bord.transform.position.y
		        + bord.transform.localScale.y/2)) {
			boundHeight += boundVectorY;
			boundVectorY -=  0.01f * 9.8f / 30.0f;
			cnt++;
			boundtim+=0.01f;
		}
		return boundtim;
	}
}

