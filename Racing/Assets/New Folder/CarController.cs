using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//汽车驱动类型
public enum CarDriveType
{
    //四驱
    FrontWheelDrive,
    //后驱
    RearWheelDrive,
    //前驱
    FourWheelDrive
}
//速度类型
public enum SpeedType
{
    //英里每小时
    MPH,
    //千米每小时
    KPH
}
public class CarController : MonoBehaviour {

    private CarDriveType m_CarDriveType=CarDriveType.FourWheelDrive;
    public WheelCollider[] m_WheelColliders = new WheelCollider[4];
    public GameObject[] m_WheelMeshes = new GameObject[4];

    private Rigidbody CarRigidbody;
    //重心位置
    private Vector3 m_CenterOfMass;
    //最大控制角度
    private float m_MaxSteerAngle = 22f;
    //最小控制角度
    private float m_MinSteerAngle = 10f;
    //转向角
    private float m_SteerAngle;
    //角度辅助助手
    private float m_SteerHelper = 0.85f;
    //牵引力
    private float m_TractionControl =0.55f;
    //所有轮胎的扭矩
    private float m_FullTorqueAllWheels = 2000f;
    //反向扭矩
    private float m_ReverseTorque = 400f;
    //最大下压力
    private float m_Downforce = 250f;
    //速度类型
    private SpeedType m_SpeedType;
    //最高速度
    private float m_Topspeed = 5000f;
    //挡位总数
    private static int GearsNum = 7;
    //最大滑动距离
    private float m_SlipLimit;
    //最大手刹扭矩
    private float m_MaxHandbrakeTorque;
    //刹车扭矩/制动扭矩
    private float m_BrakeTorque = 2000f;
    //边界范围
    private float m_RevRangeBoundary = 1f;
    //前转向角
    private float m_OldRotation;
    //当前扭矩
    private float m_CurrentTorque;
    //当前挡位
    private float m_GearNum;
    //挡位因子/齿轮因素
    private float m_GearFactor;
    private float Revs;

    public float CurrentSpeed { get { return CarRigidbody.velocity.magnitude * 2.23f; } }
    public float MaxSpeed { get { return m_Topspeed; } }
    private static float CurveFactor(float factor)
    {
        return 1 - (1 - factor) * (1 - factor);
    }
    private static float ULerp(float from, float to, float value)
    {
        return (1.0f - value) * from + value * to;
    }

	// Use this for initialization
	void Start () {
        //设置汽车重心
        m_CenterOfMass = new Vector3(0, 0, 0);
        CarRigidbody = GetComponent<Rigidbody>();
        CarRigidbody.centerOfMass = m_CenterOfMass;

        //扭矩初始化
        m_MaxHandbrakeTorque = float.MaxValue;
        m_CurrentTorque = m_FullTorqueAllWheels - (m_TractionControl * m_FullTorqueAllWheels);
	}

    //挡位切换
    private void GearChanging()
    {
        float f = Mathf.Abs(CurrentSpeed / MaxSpeed);
        float upgearlimit = (1 / (float)GearsNum) * (m_GearNum + 1);
        float downgearlimit = (1 / (float)GearsNum) * m_GearNum;

        if (m_GearNum > 0 && f < downgearlimit)
        {
            m_GearNum--;
        }

        if (f > upgearlimit && (m_GearNum < (GearsNum - 1)))
        {
            m_GearNum++;
        }
    }

    //计算档位因子
    private void CalculateGearFactor()
    {
        float f = (1 / (float)GearsNum);
        //我们要让值平滑地想着目标移动，以保证转速不会在变换档位时突然地上高或者降低
        //反向差值，通过当前速度的比例值，找当前速度在当前档位的比例位置，得到的值将是一个0~1范围内的值。
        var targetGearFactor = Mathf.InverseLerp(f * m_GearNum, f * (m_GearNum + 1), Mathf.Abs(CurrentSpeed / MaxSpeed));
        //从当前档位因子向目标档位因子做平滑差值
        m_GearFactor = Mathf.Lerp(m_GearFactor, targetGearFactor, Time.deltaTime * 5f);
    }

    //计算转速
    private void CalculateRevs()
    {
        //计算在当前档位上的转速因子（决定在当前档位上的转速）
        CalculateGearFactor();
        //档位因子：当前档位/总档位数
        var gearNumFactor = m_GearNum / (float)GearsNum;
        //计算在当前档位下的最小转速
        var revsRangeMin = ULerp(0f, m_RevRangeBoundary, CurveFactor(gearNumFactor));
        //计算在当前档位下的最大转速
        var revsRangeMax = ULerp(m_RevRangeBoundary, 1f, gearNumFactor);
        //根据当前的转速因子，计算当前的转速
        Revs = ULerp(revsRangeMin, revsRangeMax, m_GearFactor);
    }

    //外部调用的汽车移动控制函数
    public void CarMove(float steer, float accel, float footbrake, float handbrake)
    {
        //保持当前的轮胎网格跟随WheelCollider转动
        for (int i = 0; i < 4; i++)
        {
            Quaternion quat;
            Vector3 position;
            m_WheelColliders[i].GetWorldPose(out position, out quat);
            m_WheelMeshes[i].transform.position = position;
            m_WheelMeshes[i].transform.rotation = quat;
        }
        //限定输入值范围
        steer = Mathf.Clamp(steer, -0.5f, 0.5f);
        accel = Mathf.Clamp(accel, 0, 1);
        footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);
        handbrake = Mathf.Clamp(handbrake, 0, 1);

        
        //设置前轮转角 wheels下标为0、1的就是前轮
        m_SteerAngle = steer * m_MaxSteerAngle;
        m_WheelColliders[0].steerAngle = m_SteerAngle;
        m_WheelColliders[1].steerAngle = m_SteerAngle;
        //调用角度辅助助手，
        SteerHelper();
        //设置加速/刹车信息到WheelCollider
        ApplyDrive(accel, footbrake);

        if (accel == 1)
           CarRigidbody.AddForce(transform.forward * 100);

        CapSpeed();

        //设置手刹 Wheel下标是2、3就是后轮
        if (handbrake > 0f)
        {
            //设置手刹值到后轮，达到减速目的
            var handbrakeTorque = handbrake * m_MaxHandbrakeTorque;
            m_WheelColliders[2].brakeTorque = handbrakeTorque;
            m_WheelColliders[3].brakeTorque = handbrakeTorque;
        }

        //计算转速，用来供外部调用转速属性Revs来播放引擎声音等
        CalculateRevs();
        GearChanging();
        AddDownForce();
        TractionControl();
    }

    //控制车速
    private void CapSpeed()
    {
        float speed = CarRigidbody.velocity.magnitude;
        switch (m_SpeedType)
        {
            case SpeedType.MPH:
                speed *= 2.23f;
                if (speed > m_Topspeed)
                    CarRigidbody.velocity = (m_Topspeed / 2.23693629f) * CarRigidbody.velocity.normalized;
                break;

            case SpeedType.KPH:
                speed *= 3.6f;
                if (speed > m_Topspeed)
                    CarRigidbody.velocity = (m_Topspeed / 3.6f) * CarRigidbody.velocity.normalized;
                break;
        }
    }

    private void ApplyDrive(float accel, float footbrake)
    {
        float thrustTorque;
        //根据赛车驱动类型添加驱动力
        switch (m_CarDriveType)
        {
            case CarDriveType.FourWheelDrive:
                thrustTorque = accel * (m_FullTorqueAllWheels / 4);
                for (int i = 0; i < 4; i++)
                {
                    m_WheelColliders[i].motorTorque = thrustTorque;
                }
                break;
            case CarDriveType.FrontWheelDrive:
                thrustTorque = accel * (m_FullTorqueAllWheels / 2);
                m_WheelColliders[0].motorTorque = thrustTorque;
                m_WheelColliders[1].motorTorque = thrustTorque;
                break;
            case CarDriveType.RearWheelDrive:
                thrustTorque = accel * (m_FullTorqueAllWheels / 2);
                m_WheelColliders[2].motorTorque = thrustTorque;
                m_WheelColliders[3].motorTorque = thrustTorque;
                break;
        }
        for (int i = 0; i < 4; i++)
        {
            if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, CarRigidbody.velocity) < 50f)
            {
                m_WheelColliders[i].brakeTorque = m_BrakeTorque * footbrake;
            }
            else if (footbrake > 0)
            {
                m_WheelColliders[i].brakeTorque = 0f;
                m_WheelColliders[i].motorTorque = -m_ReverseTorque * footbrake;
            }
        }
    }

    private void SteerHelper()
    {
        for (int i = 0; i < 4; i++)
        {
            WheelHit wheelhit;
            m_WheelColliders[i].GetGroundHit(out wheelhit);
            if (wheelhit.normal == Vector3.zero)
                return;
            //假如轮子离地，就不用调整汽车角度了
        }

        //下面这个If函数的效果就是：假如上一次车体Y方向角度比这次小于十度，就根据相差的度数乘以系数m_SteerHelper，得出需要旋转的度数
        //根据这个度数算出四元数，然后将刚体速度直接旋转这个偏移度数，
        //根据代码开头m_SteerHelper的定义，这个做法相当于做了一个角度辅助，不完全凭借WheelCollider物理效果
        //而直接操控速度方向，对车角度进行调整。
        //现在来看，如果m_SteerHelper越小，则调整的角度越小，如果m_SteerHelper为0，则调整的角度为0,。
        if (Mathf.Abs(m_OldRotation - transform.eulerAngles.y) < 10f)
        {
            var turnadjust = (transform.eulerAngles.y - m_OldRotation) * m_SteerHelper;
            Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
            CarRigidbody.velocity = velRotation * CarRigidbody.velocity;
        }
        m_OldRotation = transform.eulerAngles.y;
    }

    //给车体增加一个向下的力，稳定车身，增大惯性
    private void AddDownForce()
    {
        CarRigidbody.AddForce(-Vector3.up * m_Downforce * CarRigidbody.velocity.magnitude);
    }

    //如果汽车轮胎过度滑转，牵引力系统可以控制减少轮胎动力
    private void TractionControl()
    {
        WheelHit wheelHit;
        switch (m_CarDriveType)
        {
            //四驱
            case CarDriveType.FourWheelDrive:
                // loop through all wheels
                for (int i = 0; i < 4; i++)
                {
                    m_WheelColliders[i].GetGroundHit(out wheelHit);

                    AdjustTorque(wheelHit.forwardSlip);
                }
                break;
            //后驱
            case CarDriveType.RearWheelDrive:
                m_WheelColliders[2].GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);

                m_WheelColliders[3].GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);
                break;
            //前驱
            case CarDriveType.FrontWheelDrive:
                m_WheelColliders[0].GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);

                m_WheelColliders[1].GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);
                break;
        }
    }
    private void AdjustTorque(float forwardSlip)
    {
        //当向前滑动距离超过阈值后，就说明轮胎过度滑转，则减少牵引力，以降低转速。
        if (forwardSlip >= m_SlipLimit && m_CurrentTorque >= 0)
        {
            m_CurrentTorque -= 10 * m_TractionControl;
        }
        else
        {
            m_CurrentTorque += 10 * m_TractionControl;
            if (m_CurrentTorque > m_FullTorqueAllWheels)
            {
                m_CurrentTorque = m_FullTorqueAllWheels;
            }
        }
    }
}
