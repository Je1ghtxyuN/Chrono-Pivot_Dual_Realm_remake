// Catalog Reference
// 10 00:07:18.20 -17:23:11.00 2000.00 -0.018 +0.036 6.19    256 #  A6Vn    BD-18 6428   RV-009

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Tenkoku/StarDataset")]
public class TenkokuStarData : ScriptableObject {

    [System.Serializable]
    public class StarElement{
        public string name = "";
        public int starIndex = 256;

        public float ascH = 0.0f;
        public float ascM = 7.0f;
        public float ascS = 18.20f;

        public float decH = -17.0f;
        public float decM = 23.0f;
        public float decS = 11.0f;

        public int color = 0;
        public float factor = 6.0f;

        public float magnitude = 6.19f;
        public int brightFactor = 4;
    }

	public TextAsset starFile;
	public bool populateStarData = false;
    public StarElement[] starElements;

    private string[] starDataArray;
	private string starDataString;
	private string workData;
	private string mod;
	private float h;
	private float m;
	private float s;
	private string starColor = "";




    void OnValidate(){

    	if (populateStarData){
    		Debug.Log("populate!");
    		PopulateStarData();
    	}
    }



	void PopulateStarData(){

		populateStarData = false;
		int recIdx = 0;

		if (starFile != null){

			PopulateStarDataString();
			starElements = new TenkokuStarData.StarElement[starDataArray.Length];

			
			for (int cnvX = 0; cnvX < starElements.Length; cnvX++){

				starElements[cnvX] = new TenkokuStarData.StarElement();

				if (starElements[cnvX] != null){

					//calculate Right Ascension
					workData = starDataArray[cnvX].Substring(6,11);
					h = float.Parse(workData.Substring(0,2));
					m = float.Parse(workData.Substring(3,2));
					s = float.Parse(workData.Substring(6,2));
					starElements[cnvX].ascH = h;
					starElements[cnvX].ascM = m;
					starElements[cnvX].ascS = s;

					//calculate Declination
					workData = starDataArray[cnvX].Substring(18,12);
					mod = workData.Substring(0,1);
					h = float.Parse(workData.Substring(1,2));
					m = float.Parse(workData.Substring(4,2));
					s = float.Parse(workData.Substring(7,5));
					if (mod=="-") h = 0f - h;;
					starElements[cnvX].decH = h;
					starElements[cnvX].decM = m;
					starElements[cnvX].decS = s;
					
				
					//set star colors
					workData = starDataArray[cnvX].Substring(67,8);
					starColor = workData.Substring(1,1);
					if (starColor == "O") starElements[cnvX].color = 0;
					if (starColor == "B") starElements[cnvX].color = 1;
					if (starColor == "A") starElements[cnvX].color = 2;
					if (starColor == "F") starElements[cnvX].color = 3;
					if (starColor == "G") starElements[cnvX].color = 4;
					if (starColor == "K") starElements[cnvX].color = 5;
					if (starColor == "M") starElements[cnvX].color = 6;

					//set star magnitude
					starElements[cnvX].factor = float.Parse(workData.Substring(2,1));

					//calculate magnitude
					workData = starDataArray[cnvX].Substring(53,4);
					starElements[cnvX].magnitude = float.Parse(workData);
					
					//calculate star index value
					starElements[cnvX].starIndex = int.Parse(starDataArray[cnvX].Substring(58,6));

					//calculate brightness factor
					workData = starDataArray[cnvX].Substring(67,8);
					if (workData.Contains("IV")){
						starElements[cnvX].brightFactor = 4;
					} else if (workData.Contains("V")){
						starElements[cnvX].brightFactor = 5;
					} else if (workData.Contains("III")){
						starElements[cnvX].brightFactor = 3;
					} else if (workData.Contains("II")){
						starElements[cnvX].brightFactor = 2;
					} else if (workData.Contains("I")){
						starElements[cnvX].brightFactor = 1;
					}

					recIdx++;

				}
			}
		}

		Debug.Log(recIdx);

	}




	void PopulateStarDataString(){
	    starDataString = starFile.text;
	    starDataArray = starDataString.Split("\n"[0]);

	}





}






	