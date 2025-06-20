using UnityEngine;
using System.IO.Ports;
using System.Collections.Generic;
using CarControllerwithShooting;

public class ReadSerialData : MonoBehaviour
{
    // Ű ���¸� �����ϱ� ���� ��ųʸ�
    private Dictionary<KeyCode, bool> simulatedKeys = new Dictionary<KeyCode, bool>();
    private Dictionary<int, bool> simulatedMouseButtons = new Dictionary<int, bool>();
    SerialPort sp;
    public string portName = "COM4";
    public int baudRate = 9600;
    
    float joyX, joyY;
    int btn1, btn2;

    void Start()
    {
        // �ùķ��̼��� Ű�� �ʱ�ȭ
        simulatedKeys[KeyCode.W] = false;
        simulatedKeys[KeyCode.A] = false;
        simulatedKeys[KeyCode.S] = false;
        simulatedKeys[KeyCode.D] = false;
        simulatedKeys[KeyCode.Space] = false;

        simulatedMouseButtons[0] = false; // ��Ŭ��
        simulatedMouseButtons[1] = false; // ��Ŭ��

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
        // �Ƶ��̳� ������ �б�
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

        // �Է� ���� �� �ùķ��̼�
        // ���̽�ƽ Y�� �� W/S Ű
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

        // ���̽�ƽ X�� �� A/D Ű
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

        // ��ư1 �� Space(�ڵ�극��ũ)
        SimulateKey(KeyCode.Space, btn1 == 1);

        // ��ư2 �� ���콺 ��Ŭ��(�Ѿ� �߻�)
        SimulateMouseClick(0, btn2 == 1);
        
        // �Ƶ��̳�� �ǵ�� ����
        SendFeedbackToArduino();
    }

    // Ű �Է� �ùķ��̼� �޼���
    void SimulateKey(KeyCode key, bool isPressed)
    {
        simulatedKeys[key] = isPressed;
    }

    // ���콺 Ŭ�� �ùķ��̼� �޼���
    void SimulateMouseClick(int button, bool isPressed)
    {
        simulatedMouseButtons[button] = isPressed;
    }

    // Input.GetKey() ��ü �޼���
    public bool GetSimulatedKey(KeyCode key)
    {
        return simulatedKeys.ContainsKey(key) ? simulatedKeys[key] : Input.GetKey(key);
    }

    // Input.GetKeyDown() ��ü �޼���
    public bool GetSimulatedKeyDown(KeyCode key)
    {
        // ���� ���������� ���� ������ ���¿� �� �ʿ�
        return GetSimulatedKey(key) || Input.GetKeyDown(key);
    }

    // Input.GetMouseButton() ��ü �޼���
    public bool GetSimulatedMouseButton(int button)
    {
        return simulatedMouseButtons.ContainsKey(button) ? simulatedMouseButtons[button] : Input.GetMouseButton(button);
    }

    // �Ƶ��̳�� �ǵ�� ����
    void SendFeedbackToArduino()
    {
        if (sp != null && sp.IsOpen)
        {
            // �ӵ��� ���� LED ����
            if (CarController.Instance != null)
            {
                float speed = CarController.Instance.CurrentSpeed;
                if (speed > 50f)
                {
                    sp.Write("L"); // LED �ѱ�
                }
                else
                {
                    sp.Write("l"); // LED ����
                }

                // �浹�̳� Ư�� �̺�Ʈ �� ���� �︮��
                if (CarController.Instance.Health < 50)
                {
                    sp.Write("B"); // ���� �︮��
                }
            }
        }
    }

    void OnDestroy()
    {
        if (sp != null && sp.IsOpen) sp.Close();
    }
}
