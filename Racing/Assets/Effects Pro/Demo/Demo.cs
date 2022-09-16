// License: https://en.wikipedia.org/wiki/MIT_License
// The code in this script is written by Arnab Raha
// Code may be redistributed in source form, provided all the comments at the top here are kept intact

using UnityEngine;
using UnityEngine.UI;

public class Demo : MonoBehaviour {

	public Text fxName;				// the name of the effect
	public Text str;				// effect strength shower Text
	public Text contStr;			// contrast value
	public Text brtStr;				// brightness value

	public Slider strength;			// effect strength controller
	public Slider contrast;			// contrast controller
	public Slider bright;			// brightness controller

	public Toggle on;				// contrast and brightness

	private string[] effectNames = { "Greyscale", "Sepia", "Negative" };

	public Effects fx;

	int index = 0;

	public void Left () {
		index--;
		if (index < 0) {
			index = effectNames.Length - 1;
		}
		fxName.text = effectNames[index];
		SetEffext (index);
	}

	public void Right () {
		index++;
		if (index > effectNames.Length - 1) {
			index = 0;
		}
		fxName.text = effectNames[index];
		SetEffext (index);
	}

	public void Original () {
		fx.DisableFx ();
	}

	public void Effect () {
		fx.EnableFx ();
	}

	public void ValueChange () {
		fx.styleStrength = strength.value;
		str.text = "Strength : " + (strength.value * 100).ToString ("0");
	}

	public void Contrast () {
		fx.contrast = contrast.value;
		contStr.text = "Contrast : " + contrast.value.ToString ("F2");
	}

	public void Bright () {
		fx.brightness = bright.value;
		brtStr.text = "Brightness : " + bright.value.ToString ("F2");
	}

	public void ToggleOn () {
		fx.on = on.isOn;
	}

	private void SetEffext (int ind) {
		switch (ind) {
			case 0:
				fx.SetFx (Fx.greyscale);
				break;
			case 1:
				fx.SetFx (Fx.sepia);
				break;
			case 2:
				fx.SetFx (Fx.negative);
				break;
		}
	}
}
