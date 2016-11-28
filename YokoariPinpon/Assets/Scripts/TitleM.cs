using UnityEngine;
using System.Collections;

/****************************************
	Kinectで認証してプレイヤーを決定し、利き手を選択する

	Kinectを扱うViewクラスから手の高さをもらい、
	一定時間あげた方の手を利き手としてラケットを合わせます
	
*****************************************/
public class TitleM : MonoBehaviour {
	public static bool handSelect;							//利き手が選択できているかどうか
	public static AudioSource audio;						//タイトルのBGMを流す
	public static bool start = true;						//ゲーム起動時に一度だけ実行
	public static bool titlestart = true;							//タイトル開始時に一度だけ実行

	public AudioClip bgm;									//BGM

	public GameObject rhp;									//右手のゲージ
	public GameObject lhp;									//左手のゲージ

	public GameObject[] modesentaku = new GameObject[3];	//モード選択を行うゲームオブジェクトへの参照

	public int vec;											//ゲージの角度


	// Use this for initialization
	void Start () {							
	}
	
	// Update is called once per frame
	void Update () {
		//タイトルが始まった時に初期化とBGMを再生
		if (titlestart) {
			GM.raketnum = 0;
			GM.raricnt = 0;
			GM.plscore = 0;
			GM.enscore = 0;
			Application.targetFrameRate = 150;
			handSelect = false;
			vec = 0;
			audio = GetComponent<AudioSource> ();
			audio.PlayOneShot (bgm);
			
			titlestart = false;
		}
		//利き手選択のゲージが参照できていないなら探す
		if(!rhp)
			rhp = GameObject.Find ("Sphere (4)");
		if(!lhp)lhp = GameObject.Find ("Sphere (3)");
		if(!modesentaku[0])modesentaku[0] = GameObject.Find ("Sphere (1)");
		if(!modesentaku[1])modesentaku[1] = GameObject.Find ("Sphere (2)");
			
		if(!modesentaku[2])modesentaku[2] = GameObject.Find ("Sphere");
			//titlestart = false;

		/*ゲーム開始時に最初にできたGM(ゲームマスターオブジェクト）を残し
		、タイトルに戻ってきたときに作られる新しいGMを削除する          */
		if (start) {
			GameObject.Find ("GM").name = "GM1";

			start = false;
		} else {
			if(GameObject.Find ("GM")) {
				GameObject.Destroy (GameObject.Find ("GM"));
			}
		}

		//ゲージオブジェクトが存在していたら
		if (rhp) {
			//Kinectが人を認識していたら
			if (View.active) {
				//利き手を選択していなかったら
				if (handSelect == false) {
					//ゲージオブジェクトが非表示の場合表示する
					if (rhp.activeSelf == false) {
						rhp.SetActive (true);
					}
					if (lhp.activeSelf == false) {
						lhp.SetActive (true);
					}


					if (View.rhandheight > 6) {//右手を挙げていたら

						//ゲージを回す
						vec++;
						
						rhp.transform.FindChild ("meta").transform.position = rhp.transform.position + 
							new Vector3 (Mathf.Sin (vec * 3.14f / 180) * rhp.transform.localScale.x / 2, Mathf.Cos (vec * 3.14f / 180) * rhp.transform.localScale.x / 2, 0);
						rhp.transform.FindChild ("meta").transform.localEulerAngles = new Vector3 (0, 0, -vec);
						//ゲージが一定量回転したら利き手を右手にする
						if (vec > 360) {
							handSelect = true;
							View.kikite = true;
						}
					}else  if (View.lhandheight > 6) {//左手を挙げていたら
						//ゲージを回す
						vec--;
						lhp.transform.FindChild ("meta").transform.position = lhp.transform.position + 
							new Vector3 (Mathf.Sin (vec * 3.14f / 180) * lhp.transform.localScale.x / 2, Mathf.Cos (vec * 3.14f / 180) * lhp.transform.localScale.x / 2, 0);
						lhp.transform.FindChild ("meta").transform.localEulerAngles = new Vector3 (0, 0, -vec);
						//ゲージが一定量回転したら利き手を左手にする
						if (vec < -360) {
							handSelect = true;
							View.kikite = false;
						}

					} else {//両手を挙げていなかったら
						//角度を0にしてゲージを戻す
						vec = 0;
						rhp.transform.FindChild ("meta").transform.position = rhp.transform.position + 
							new Vector3 (Mathf.Sin (vec * 3.14f / 180) * rhp.transform.localScale.x / 2, Mathf.Cos (vec * 3.14f / 180) * rhp.transform.localScale.x / 2, 0);
						rhp.transform.FindChild ("meta").transform.localEulerAngles = new Vector3 (0, 0, -vec);
						lhp.transform.FindChild ("meta").transform.position = lhp.transform.position + 
							new Vector3 (Mathf.Sin (vec * 3.14f / 180) * lhp.transform.localScale.x / 2, Mathf.Cos (vec * 3.14f / 180) * lhp.transform.localScale.x / 2, 0);
						lhp.transform.FindChild ("meta").transform.localEulerAngles = new Vector3 (0, 0, -vec);
					}

				} else {
					//利き手を選択出来ているならゲージオブジェクトを表示しない
					if (rhp) {
						rhp.SetActive (false);
						lhp.SetActive (false);
					}

				}
			} else {
				//Kinectが認識していないならゲージオブジェクトを表示しない
				if (rhp) {
					rhp.SetActive (false);
					lhp.SetActive (false);
				}
				//認識していて外れた場合、利き手を選択していない状態にする
				handSelect = false;
			}
		}
		//Escapeで終了
		if (Input.GetKey (KeyCode.Escape))
			Application.Quit ();
	}
}
