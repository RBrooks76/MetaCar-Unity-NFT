using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityStandardAssets.CrossPlatformInput;
public class CarUserInput : MonoBehaviour {

    private CarController _car;
    void Awake()
    {
        _car = GetComponent<CarController>();
    }
    void FixedUpdate()
    {
        //玩家控制键盘输入 AD键控制赛车方向，WS键分别控制赛车前进、后退，空格键控制手闸
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float handbrake = Input.GetAxis("Jump");

        //将玩家输入的参数值传入CarMove函数
        _car.CarMove(h, v, v, handbrake);
    }
}
