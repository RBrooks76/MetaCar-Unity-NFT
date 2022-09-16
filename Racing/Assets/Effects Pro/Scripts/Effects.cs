// License: https://en.wikipedia.org/wiki/MIT_License
// The code in this script is written by Arnab Raha
// Code may be redistributed in source form, provided all the comments at the top here are kept intact

using UnityEngine;

public enum Fx {greyscale, sepia, negative};

[ExecuteInEditMode]
[RequireComponent(typeof (Camera))]
public class Effects : MonoBehaviour {

	[Header ("Contrast & Brightness")]

	[Tooltip ("Turn on/off contrast and brightness adjustments")]
	public bool on = true;
	
	[Range (-50.0f, 100.0f)]
	public float contrast = 0.0f;

	[Range (-100.0f, 100.0f)]
	public float brightness = 0.0f;
	
	[Header ("Color Adjustments")]

	[Tooltip ("Choose the effect type to apply it on the renderer")]
	public Fx effectType = Fx.greyscale;
    
    [Range (0.0f, 1.0f)]
    [Tooltip ("Style strength for the image effect (not applicable for 'Negative')")]
	public float styleStrength = 0.0f;

	private Shader shader;
	private Shader cAndB;

	private Material gMat;
	private Material cb;

	void Start () {
		cAndB = Shader.Find ("Hidden/Contrast&Brightness");
		cb = new Material (cAndB);
		cb.hideFlags = HideFlags.HideAndDontSave;
		CheckHWSupport ();
	}

	Material material {
		get {
			if (gMat == null) {
                gMat = new Material (shader);
                gMat.hideFlags = HideFlags.HideAndDontSave;
            }
            return gMat;
		}
	}

    void OnDisable () {
    	if (gMat) {
	        DestroyImmediate (gMat);
	    }
    }

    void OnRenderImage (RenderTexture source, RenderTexture destination) {
		SetFx (effectType);
		if (on) {
			RenderTexture renderTemp = RenderTexture.GetTemporary(source.width, source.height);
			float brig = brightness / 175;
			float cont = contrast * 2.0f;

			cb.SetFloat ("_Cont", cont);
			cb.SetFloat ("_Bright", brig);

			Graphics.Blit (source, renderTemp, cb);

	        material.SetTexture ("_MainTex", renderTemp);
	        material.SetFloat ("_Strength", styleStrength);

	        Graphics.Blit (renderTemp, destination, material);
			RenderTexture.ReleaseTemporary (renderTemp);
		} else {
			material.SetFloat ("_Strength", styleStrength);
			Graphics.Blit (source, destination, material);
		}
	}

	/// <summary>
	/// Set image effect to apply it on the renderer.
	/// </summary>
	/// <param name="effect">Effect.</param>
    public void SetFx (Fx effect) {
    	effectType = effect;
		if (effect == Fx.greyscale) {
			shader = Shader.Find ("Hidden/Greyscale");
		} else if (effect == Fx.sepia) {
			shader = Shader.Find ("Hidden/Sepia");
		} else {
			shader = Shader.Find ("Hidden/Negative");
		}
		material.shader = shader;
    }

	/// <summary>
	/// Disables all the image effects associated with Effects.
	/// </summary>
    public void DisableFx () {
		if (enabled)
			this.enabled = false;
    }

	/// <summary>
	/// Enables all the image effects associated with Effects,
	/// </summary>
    public void EnableFx () {
		if (!enabled)
			this.enabled = true;
    }

    private bool CheckHWSupport () {
        if (!cAndB || !cAndB.isSupported) {
        	enabled = false;
        	Debug.LogError ("Some of the shaders are not supproted on this platform");
        	return false;
        }
        return true;
    }
}