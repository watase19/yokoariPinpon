using UnityEngine;
using System.Collections;
/**************************************
	タイトルでのボールを動かす

	残像を表示し、移動量分移動させる
 **************************************/
public class BallTitle : MonoBehaviour {
	public static Vector3 vect;						//移動量
	public static Vector3 pos;						//座標

	public GameObject[] zanzou = new GameObject[4];	//残像

	Vector3[] zanzoupos = new Vector3[4];			//残像の座標
	Vector3 zpos;									//前の座標
	Vector3 sca;									//当たり判定のサイズ

	float tim;										//残像に使用する時間

	int zanzounum = 4;								//残像の表示する数
	int zancnt;										//残像をずらした番号
	// Use this for initialization
	void Start () {
		//初期化
		tim = 0;
		pos = transform.position;
		for (int i = 0; i < zanzounum; i++) {
			zanzoupos[i] = transform.position;
		}
		zancnt = 0;
	}
	
	// Update is called once per frame
	void Update () {
		//移動
		pos += vect;
		transform.position = pos;

		//残像の移動
		zanzouProcess ();
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
