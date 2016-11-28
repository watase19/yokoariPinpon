using UnityEngine;
using System.Collections;

/************************************
	ゲームに関わる変数の宣言とゲームの終了
 ************************************/
public class GM : MonoBehaviour {
	public static bool bounds;					//最後にバウンドした陣地
	public static int plscore;					//プレイヤーの試合のスコア
	public static int enscore;					//ヨコアリくんの試合のスコア
	public static int raricnt;					//ラリーモードでの打ち返した回数
	public static int raketnum;					//現在使っているラケット(1:卓球のラケット 2:しゃもじ 3:スリッパ)
	public static int mode;						//ゲームのモード(0 練習 1簡単2難しい)

	// Update is called once per frame
	void Update () {
		//タイトルに戻る
		if(Input.GetKey(KeyCode.Escape)) {
			Application.LoadLevel("title");
		}
	}
}
