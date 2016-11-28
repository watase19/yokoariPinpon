using UnityEngine;
using System.Collections;

/*******************************************************************
ボールを動かすクラス

ボールの移動量に応じてボールを動かします

バウンドする動きやすり抜け時の対応を行います

場外に出たとき、同じ陣地で2回バウンドした時にアウトの処理を行います

アウトの時は初期化を行いサーブを打ち出します
 *******************************************************************/
public class Ball : MonoBehaviour {

	public static Vector3 vect;						//移動量
	public static float gra;						//重力
	public static bool raketHitFlag;				//ラケットに当たったかどうか
	public static float cnt;							//デバッグ用*****************************
	public static int raricnt;						//ラリーを行えた回数

	public GameObject[] zanzou = new GameObject[4];	//残像
	public AudioClip daihansha;						//台に当たった時の音
	public AudioClip rakhansha;						//ラケットに当たった時の音
	public AudioClip maiSco;						//点を取られた時の音
	public AudioClip pluSco;						//点を取った時の音

	GameObject dai;									//卓球台のゲームオブジェクト
	GameObject net;									//ネットのゲームオブジェクト

	AudioSource audioSource;						//音を流す

	Vector3[] zanzoupos = new Vector3[4];			//残像の座標
	Vector3 pos;									//座標
	Vector3 zpos;									//前フレームでの座標
	Vector3 sca;									//当たり判定の大きさ
	float tim;										//時間			

	bool hif;										//台に当たった時
	bool fallf = false;								//すり抜け防止のフラグ

	int sta;										//スリープ中か判定するフラグ
	int zanzounum = 4;								//残像の表示する数
	int zancnt;										//残像のずらした番号


	// Use this for initialization
	void Start () {
		//モデル・コンポーネントの読み込み
		audioSource = gameObject.GetComponent<AudioSource>();
		dai = GameObject.Find ("dai");
		net = GameObject.Find ("net");

		//初期化
		tim = 0;
		auto ();
		cnt = 0;
		pos = transform.position;
		for (int i = 0; i < zanzounum; i++) {
			zanzoupos[i] = pos;
		}
		zancnt = 0;
		raricnt = 0;
	}
	
	// Update is called once per frame
	void Update () {
		if (sta == 0) {
			//座標変数を増減
			pos += vect;

			//場外に出たらアウト処理
			if (pos.x > 15 || pos.x < -15 ||
				pos.z > 25 || pos.z < -15
		    ) {
				auto (true);

			}
			//ボールが止まったらアウト処理
			if (vect == Vector3.zero) {
				auto (true);
			}
			//座標が台をすり抜けたとき一度だけ台の上に戻す
			if (pos.y + sca.y / 2 < 0f) {
				if (fallf == false) {
					pos.y = dai.transform.position.y;
					fallf = true;
				}
			}

			//床についたときアウト処理
			if (pos.y + sca.y / 2 < -5) {
				auto (true);
			}
			//台に当たった時
			if (hit (dai.transform.position, dai.transform.localScale)) {
				Debug.Log(gra);
				//台に当たった時の音を流す
				audioSource.PlayOneShot (daihansha, 0.3f);

				//ラケットを当たっていない状態にする
				Raket.ball_hitflg = false;

				//すり抜けをしていない状態に
				fallf = false;

				//初めて当たった時反射の処理を行う
				if (hif == false) {
					vect.y = -vect.y * 23 / 30;// 現在の速度の向きを逆にしピンポン玉の反発係数をかける
					pos.y = dai.transform.position.y + sca.y / 2 + dai.transform.localScale.y / 2 + 0.1f;

					hif = true;
				}

				//バウンドした陣地を確認し,2回バウンドしたらSEを流し、スコアを足してからサーブへ
				if (pos.z > dai.transform.position.z) {

					//Debug.Log ("ballcnt:" + cnt + "gra;" + gra);
					if (GM.bounds == false) {
						audioSource.PlayOneShot (pluSco, 0.3f);
						GM.plscore ++;
						Mekuru.sta (false);
						auto ();
					
					} else {
						GM.bounds = false;
					}
				} else {
				
					if (GM.bounds) {
						audioSource.PlayOneShot (maiSco, 0.3f);
						GM.enscore ++;
						Mekuru.sta (true);
						auto ();
					
					} else {
						GM.bounds = true;
					}
				}

				//重力を初期化
				gra = 0;
				cnt = 0;
			} else {

				//空中にいるなら重力を足す
				hif = false;
				gra += Time.deltaTime;
				vect.y -= Time.deltaTime * 9.8f / 30;
				//gra += 0.00008f;
				//
			}
			//ネットに当たった移動量を0に
			if (hit (net.transform.position, net.transform.localScale)) {
				vect.x = 0;
				vect.z = 0;
				vect.y = 0;
			}
			//ラケットに当たった時,音を流す
			if (raketHitFlag) {
				audioSource.PlayOneShot (rakhansha, 0.3f);
				raketHitFlag = false;
			}

			//残像の処理
			zanzouProcess();

			//座標を更新
			transform.position = pos;





		} else if (sta == 1) {
			//スリープの実行
			if (stopTim (2)) {
				//得点が一定値を超えたらリザルトへ
				if(GM.plscore >= 7 || 
				   GM.enscore >= 7) {

					//Debug.Log (GM.plscore + ":" + GM.enscore);
					Application.LoadLevel("Rally");
				}else {

					sta = 0;
				}
			}
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

	//アウト時の処理
	void auto(bool sc = false) {
			
			//残像の座標を初期化
			for (int i = 0; i < zanzounum; i++) {
				zanzoupos [i] = new Vector3 (0, 0, -90);
			}
			
			//得点の加算とSE再生
			if (sc == true && GM.mode > 0) {
				if (!GM.bounds) {
				Mekuru.sta (false);
					audioSource.PlayOneShot (pluSco, 0.3f);
					GM.plscore++;
				} else {
				Mekuru.sta (true);
					audioSource.PlayOneShot (maiSco, 0.3f);
					GM.enscore++;
				}
			}
		//初期化
		tim = 0;
		sta = 1;
		pos = new Vector3 (0, 4, 6);
		//vect = new Vector3 (0, 0, 0);
		vect = new Vector3 (0, 0.4f / 2, -0.0375f * 2);
		gra = 0;
		cnt = 0;
		sta = 1;
		
		fallf = false;
		//座標を更新
		transform.position = pos;

		GM.bounds = false;

	}

	//スリープのための関数
	bool stopTim(int stop) {
		bool ret = false;
		if (tim > stop) {
			ret = true;

			tim = 0;
		} else {
				tim += Time.deltaTime;
		}

		return ret;
	}

	void zanzouProcess() {
		//残像を表示
		if (zancnt == 0) {
			//一定時間ごとに座標を変え、残像をずらす
			if ((int)(tim * 100) % (10f / 4) == 0) {
				zpos = pos;
			}
			if (tim > 0.4f / 4) {
				zanzoupos [3] = zanzoupos [2];
				zancnt = 1;
			}
			if (tim > 0.3f / 4) {
				zanzoupos [2] = zanzoupos [1];
			}
			if (tim > 0.2f / 4) {
				zanzoupos [1] = zanzoupos [0];
			}
			if (tim > 0.1f / 4) {
				zanzoupos [0] = zpos;
			}
			
			tim += Time.deltaTime;
		} else if (zancnt == 1) {
			//一定時間ごとに残像の座標を移動
			if (tim > 0.1f / 4) {
				zanzoupos [3] = zanzoupos [2];
				zanzoupos [2] = zanzoupos [1];
				zanzoupos [1] = zanzoupos [0];
				zanzoupos [0] = pos;
				tim = 0;
			} else {
				tim += Time.deltaTime;
			}
		}
		//残像の座標を更新
		for (int i = 0; i < zanzounum; i++) {
			zanzou [i].transform.position = zanzoupos [i];
		}
	}
}
