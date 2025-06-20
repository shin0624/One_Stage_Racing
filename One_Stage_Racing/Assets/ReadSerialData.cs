using UnityEngine;
using System.IO.Ports;
using System.Collections.Generic;
using CarControllerwithShooting;

public class ReadSerialData : MonoBehaviour
{
    // 키 상태를 추적하기 위한 딕셔너리
    private Dictionary<KeyCode, bool> simulatedKeys = new Dictionary<KeyCode, bool>();
    private Dictionary<int, bool> simulatedMouseButtons = new Dictionary<int, bool>();
    SerialPort sp;
    public string portName = "COM4";
    public int baudRate = 9600;
    
    float joyX, joyY;
    int btn1, btn2;

    void Start()
    {
        // 시뮬레이션할 키들 초기화
        simulatedKeys[KeyCode.W] = false;
        simulatedKeys[KeyCode.A] = false;
        simulatedKeys[KeyCode.S] = false;
        simulatedKeys[KeyCode.D] = false;
        simulatedKeys[KeyCode.Space] = false;

        simulatedMouseButtons[0] = false; // 좌클릭
        simulatedMouseButtons[1] = false; // 우클릭

        sp = new SerialPort(portName, baudRate);
        if (!sp.IsOpen)
        {
            sp.Open();
            sp.ReadTimeout = 20;
        }
        else
        {
            Debug.LogWarning("Serial port already open: " + portName);
        }
    }
    
    void Update()
    {
        // 아두이노 데이터 읽기
        try
        {
            string data = sp.ReadLine();
            string[] vals = data.Split(',');
            if (vals.Length >= 4)
            {
                joyX = (float.Parse(vals[0]) - 512) / 512f; // -1~1
                joyY = (float.Parse(vals[1]) - 512) / 512f;
                btn1 = int.Parse(vals[2]);
                btn2 = int.Parse(vals[3]);
            }
        }
        catch { }

        // 입력 매핑 및 시뮬레이션
        // 조이스틱 Y축 → W/S 키
        if (joyY > 0.2f) 
        {
            SimulateKey(KeyCode.W, true);
            SimulateKey(KeyCode.S, false);
        }
        else if (joyY < -0.2f) 
        {
            SimulateKey(KeyCode.S, true);
            SimulateKey(KeyCode.W, false);
        }
        else 
        {
            SimulateKey(KeyCode.W, false);
            SimulateKey(KeyCode.S, false);
        }

        // 조이스틱 X축 → A/D 키
        if (joyX > 0.2f) 
        {
            SimulateKey(KeyCode.D, true);
            SimulateKey(KeyCode.A, false);
        }
        else if (joyX < -0.2f) 
        {
            SimulateKey(KeyCode.A, true);
            SimulateKey(KeyCode.D, false);
        }
        else 
        {
            SimulateKey(KeyCode.A, false);
            SimulateKey(KeyCode.D, false);
        }

        // 버튼1 → Space(핸드브레이크)
        SimulateKey(KeyCode.Space, btn1 == 1);

        // 버튼2 → 마우스 좌클릭(총알 발사)
        SimulateMouseClick(0, btn2 == 1);
        
        // 아두이노로 피드백 전송
        SendFeedbackToArduino();
    }

    // 키 입력 시뮬레이션 메서드
    void SimulateKey(KeyCode key, bool isPressed)
    {
        simulatedKeys[key] = isPressed;
    }

    // 마우스 클릭 시뮬레이션 메서드
    void SimulateMouseClick(int button, bool isPressed)
    {
        simulatedMouseButtons[button] = isPressed;
    }

    // Input.GetKey() 대체 메서드
    public bool GetSimulatedKey(KeyCode key)
    {
        return simulatedKeys.ContainsKey(key) ? simulatedKeys[key] : Input.GetKey(key);
    }

    // Input.GetKeyDown() 대체 메서드
    public bool GetSimulatedKeyDown(KeyCode key)
    {
        // 실제 구현에서는 이전 프레임 상태와 비교 필요
        return GetSimulatedKey(key) || Input.GetKeyDown(key);
    }

    // Input.GetMouseButton() 대체 메서드
    public bool GetSimulatedMouseButton(int button)
    {
        return simulatedMouseButtons.ContainsKey(button) ? simulatedMouseButtons[button] : Input.GetMouseButton(button);
    }

    // 아두이노로 피드백 전송
    void SendFeedbackToArduino()
    {
        if (sp != null && sp.IsOpen)
        {
            // 속도에 따른 LED 제어
            if (CarController.Instance != null)
            {
                float speed = CarController.Instance.CurrentSpeed;
                if (speed > 50f)
                {
                    sp.Write("L"); // LED 켜기
                }
                else
                {
                    sp.Write("l"); // LED 끄기
                }

                // 충돌이나 특정 이벤트 시 부저 울리기
                if (CarController.Instance.Health < 50)
                {
                    sp.Write("B"); // 부저 울리기
                }
            }
        }
    }

    void OnDestroy()
    {
        if (sp != null && sp.IsOpen) sp.Close();
    }
}
