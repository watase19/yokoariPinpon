using UnityEngine;
using System.Collections;
/***************************************
	得点をめくる処理
	
	Ballクラスから呼ばれるとマテリアルを変え、表のスコアをめくる
****************************************/
public class Mekuru : MonoBehaviour {
	public static bool mekf;							//プレイヤーの得点をめくるかどうか
	public static bool mekf2;							//ヨコアリくんの得点をめくるかどうか

	public Material mat;								//現在表示されている得点のマテリアル
	public Material[] matNumbermat = new Material[10];	//得点のマテリアルの参照元
	public bool right;									//プレイヤーの得点かヨコアリくんの得点か
	public int mode;									//めくられるボードか裏に移っているボードか(0 表 2　裏)

	Vector3 stapos;										//初期位置
	Vector3 rot;										//角度
	Vector3 pos;										//座標
	int matNumber = 0;									//スコア	


	// Use this for initialization
	void Start () {
		//初期化
		rot = transform.localEulerAngles;
		pos = transform.position;
		stapos = pos;
		mat.CopyPropertiesFromMaterial (matNumbermat[mode/2]);
		mekf = false;
		mekf2 = false;
	}
	
	// Update is called once per frame
	void Update () {
		//表か裏か
		if (mode == 0) {
			//自分がめくられるかどうか
			if ((right&& mekf) || (right == false && mekf2)) {

				if (pos.x >= 4+stapos.x) {
					//一定距離めくられたらマテリアルを現在のスコアに変えて最初の位置へ
					pos = stapos;
					rot = new Vector3(0,0,180);
					mekf = false;
					mekf2 = false;
					matNumber++;
					mat.CopyPropertiesFromMaterial (matNumbermat [matNumber]);
				}else {
					//スコアをめくる
					rot.x += 2f;
					rot.y -= 2;
					pos.y += 0.8f / 45f;
					pos.x += 1.35f / 45f;

				}
				//座標更新
				transform.localEulerAngles = rot;
				transform.position = pos;
			}


		} else if (mode == 2) {
			//自分がめくられるかどうか
			if ((right&& mekf) || (right == false && mekf2)) {
				//表のスコアの座標
				pos.x += 1.35f / 45f;
				
				if (pos.x >= 4.1f+stapos.x) {
					//表のスコアがめくられ切ったら次のスコアのマテリアルにして座標を最初の場所に戻す
					matNumber++;
					mat.CopyPropertiesFromMaterial (matNumbermat [matNumber+1]);
					pos = stapos;
				}
			}
		} else {

		}
	}
/**********************************
	めくられるかどうかのフラグを変更する
 **********************************/
	public static void sta(bool flg) {
		if (flg == true) {
			mekf = true;
		} else {
			mekf2 = true;
		}
	}
}
