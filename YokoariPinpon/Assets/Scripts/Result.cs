using UnityEngine;
using System.Collections;

public class Result : MonoBehaviour {
	public GUIText reslut;
	public GUIText reslut2;
	public GUIText serihu;
	float tim;
	string[] rariserihu = {"すごい!!!","おめでとう!","がんばったね!","やったー!!","だいせいこう!"};
	// Use this for initialization
	void Start () {

		tim = 0;

		if (GM.mode > 0) {
			if (GM.plscore >= 7) {
				reslut.text = "かち‼";
				reslut.color = new Color(255,0,0);
				reslut2.text = "かち‼";
					if(GM.enscore == 0) {
						if(GM.mode == 1) {
							serihu.text = "エクセレント";
						}
						else {
							serihu.text = "Fantastic!!";
						}
					}else {
						serihu.text = "おめでとう";
					}
			} else {
				reslut.text = "負け…";
				reslut.color = new Color(0,0,255);
				reslut2.text = "負け…";
				serihu.text = "またあそぼうね";
			}
		} else {
				int rand = Random.Range(0,5);
			reslut.color = new Color(255,255,0);
			reslut.text = "クリア";
			reslut2.text = "クリア";
			serihu.text = rariserihu[rand];
		}
	}
	
	// Update is called once per frame
	void Update () {
	if (tim > 5) {
			TitleM.titlestart = true;
			tim = 0;
			Application.LoadLevel ("title");

		} else {
			tim += Time.deltaTime;
		}
	}
}
