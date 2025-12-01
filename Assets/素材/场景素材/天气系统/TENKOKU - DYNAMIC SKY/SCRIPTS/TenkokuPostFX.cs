using System;
using UnityEngine;

namespace Tenkoku.Effects
{
    //[ExecuteInEditMode]
    [RequireComponent (typeof(Camera))]
    public class TenkokuPostFX : MonoBehaviour
	{

	//PUBLIC VARIABLES
	public Shader useShader;

	//PRIVATE VARIABLES
	private Material useMat;
	//private Camera CamInfo;
		
		
	void Start () {
		//setup material
		useMat = new Material(useShader);

	}


	void OnRenderImage (RenderTexture source, RenderTexture destination){
		Graphics.Blit(source,destination,useMat);
	}


}
}