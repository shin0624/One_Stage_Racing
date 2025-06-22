using UnityEngine;
using System.IO.Ports;
using CarControllerwithShooting;

public class ArduinoController02 : MonoBehaviour
{
    [Header("Arduino Settings")]
    public string portName = "COM4"; // ���� ��Ʈ�� ����
    public int baudRate = 9600;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float maxSpeed = 20f; // �ִ� �ӵ�
    public float acceleration = 2f; // ���ӵ�
    public float deceleration = 8f; // ���ӵ�
    public float sensitivity = 0.3f; // ���̽�ƽ �ΰ���
    
    [Header("Car Settings")]
    public bool keepYPosition = true; // Y�� ��ġ ���� ����
    public float groundY = 0f; // ���� ���� ����
    [Header("Smoothing Settings")]
    public float smoothingFactor = 0.1f; // ������ �ε巴�� �����
    public int dataBufferSize = 3; // ������ ���ȭ�� ���� ���� ũ��

    private SerialPort serialPort;
    private float joyX, joyY;
    private int btn1, btn2;
    
    // �ε巯�� �������� ���� ������
    private float smoothedJoyX, smoothedJoyY;
    private float[] joyXBuffer, joyYBuffer;
    private int bufferIndex = 0;
    
    // ���ӵ� �ý����� ���� ������
    public float currentSpeedX = 0f;
    public float currentSpeedY = 0f;
    private Vector3 lastDirection = Vector3.zero;
    
    // ������ ������ ���� ������
    private float lastValidJoyX, lastValidJoyY;
    private float maxJoyChange = 0.5f; // �� �����Ӵ� �ִ� ��ȭ�� ����

    void Start()
    {
        // ���� �ʱ�ȭ
        joyXBuffer = new float[dataBufferSize];
        joyYBuffer = new float[dataBufferSize];
        
        // �ø��� ��Ʈ �ʱ�ȭ
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 50;
            serialPort.Open();
            Debug.Log("Arduino ���� ����: " + portName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Arduino ���� ����: " + e.Message);
        }
    }

    void Update()
    {
        ReadArduinoData();
        MoveCar();
        PushButton01();
        PushButton02();
    }

    void ReadArduinoData()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                // ���� �����Ͱ� �׿����� �� �����Ƿ� ���� �ֽ� �����͸� ���
                string latestData = "";
                while (serialPort.BytesToRead > 0)
                {
                    string data = serialPort.ReadLine().Trim();
                    if (!string.IsNullOrEmpty(data))
                    {
                        latestData = data;
                    }
                }
                
                if (string.IsNullOrEmpty(latestData))
                    return;

                Debug.Log("���� ���� ������: [" + latestData + "]");

                string[] values = latestData.Split(',');

                if (values.Length >= 4)
                {
                    if (int.TryParse(values[0], out int xRaw) &&
                        int.TryParse(values[1], out int yRaw) &&
                        int.TryParse(values[2], out int b1) &&
                        int.TryParse(values[3], out int b2))
                    {
                        // ���̽�ƽ �� ���
                        float newJoyX = -(xRaw - 512f) / 512f;
                        float newJoyY = (yRaw - 512f) / 512f;
                        
                        // �ް��� ��ȭ ���� (������ ���͸�)
                        newJoyX = Mathf.Clamp(newJoyX, lastValidJoyX - maxJoyChange, lastValidJoyX + maxJoyChange);
                        newJoyY = Mathf.Clamp(newJoyY, lastValidJoyY - maxJoyChange, lastValidJoyY + maxJoyChange);
                        
                        // ���ۿ� �� ����
                        joyXBuffer[bufferIndex] = newJoyX;
                        joyYBuffer[bufferIndex] = newJoyY;
                        bufferIndex = (bufferIndex + 1) % dataBufferSize;
                        
                        // ��հ� ���
                        float avgJoyX = 0f, avgJoyY = 0f;
                        for (int i = 0; i < dataBufferSize; i++)
                        {
                            avgJoyX += joyXBuffer[i];
                            avgJoyY += joyYBuffer[i];
                        }
                        avgJoyX /= dataBufferSize;
                        avgJoyY /= dataBufferSize;
                        
                        // ������ ����
                        if (Mathf.Abs(avgJoyX) < sensitivity) avgJoyX = 0f;
                        if (Mathf.Abs(avgJoyY) < sensitivity) avgJoyY = 0f;
                        
                        // ���� �� ����
                        joyX = avgJoyX;
                        joyY = avgJoyY;
                        lastValidJoyX = avgJoyX;
                        lastValidJoyY = avgJoyY;
                        
                        btn1 = b1;
                        btn2 = b2;

                        Debug.Log($"�Ľ� ���� - ���̽�ƽ: ({joyX:F2}, {joyY:F2}), ��ư: ({btn1}, {btn2})");
                    }
                    else
                    {
                        Debug.LogWarning("���� ��ȯ ����: " + latestData);
                    }
                }
                else
                {
                    Debug.LogWarning("������ ���� ���� (����: " + values.Length + "): " + latestData);
                }
            }
            catch (System.TimeoutException)
            {
                // Ÿ�Ӿƿ��� �������� ��Ȳ�̹Ƿ� ����
            }
            catch (System.Exception e)
            {
                Debug.LogError("������ �б� ����: " + e.Message);
            }
        }
    }

    void MoveCar()
    {
        // �ε巯�� ������ ����
        smoothedJoyX = Mathf.Lerp(smoothedJoyX, joyX, smoothingFactor);
        smoothedJoyY = Mathf.Lerp(smoothedJoyY, joyY, smoothingFactor);
        
        // �ſ� ���� ���� 0���� ó�� (�̼��� ���� ����)
        if (Mathf.Abs(smoothedJoyX) < 0.01f) smoothedJoyX = 0f;
        if (Mathf.Abs(smoothedJoyY) < 0.01f) smoothedJoyY = 0f;
        
        // ���� �Է� ���� ���
        Vector3 inputDirection = new Vector3(smoothedJoyX, 0, smoothedJoyY).normalized;
        bool hasInput = smoothedJoyX != 0f || smoothedJoyY != 0f;
        
        if (hasInput)
        {
            // �Է��� ���� �� - ����
            // ���� �������� ��� �����̴��� Ȯ��
            bool sameDirection = Vector3.Dot(inputDirection, lastDirection) > 0.8f;
            
            if (sameDirection)
            {
                // ���� �����̸� ����
                currentSpeedX = Mathf.Min(currentSpeedX + acceleration * Time.deltaTime, maxSpeed);
                currentSpeedY = Mathf.Min(currentSpeedY + acceleration * Time.deltaTime, maxSpeed);
            }
            else
            {
                // ������ �ٲ�� �⺻ �ӵ��� ����
                currentSpeedX = moveSpeed;
                currentSpeedY = moveSpeed;
            }
            lastDirection = inputDirection;
        }
        else
        {
            // �Է��� ���� �� - ����
            currentSpeedX = Mathf.Max(currentSpeedX - deceleration * Time.deltaTime, 0f);
            currentSpeedY = Mathf.Max(currentSpeedY - deceleration * Time.deltaTime, 0f);
        }
        
        // ���� �̵� ��� (�� �ະ�� �ٸ� �ӵ� ����)
        float moveX = smoothedJoyX * currentSpeedX * Time.deltaTime;
        float moveZ = smoothedJoyY * currentSpeedY * Time.deltaTime;
        
        Vector3 movement = new Vector3(moveX, 0, moveZ);
        
        if (keepYPosition)
        {
            // Y�� ��ġ�� ���� ���̷� ����
            Vector3 currentPos = transform.position;
            Vector3 newPos = currentPos + movement;
            newPos.y = groundY; // ���� ���̷� ����
            transform.position = newPos;
        }
        else
        {
            // �Ϲ����� �̵� (Y�� ��ȭ ���)
            transform.Translate(movement);
        }
        // �Ƶ��̳�� �ǵ�� ����
        SendFeedback();
        
        // ����� ���� ���
        if (hasInput)
        {
            Debug.Log($"���� �ӵ�: X={currentSpeedX:F1}, Y={currentSpeedY:F1}, �Է�: ({smoothedJoyX:F2}, {smoothedJoyY:F2})");
        }
    }

    void SendFeedback()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            // ��ư�� ������ LED �ѱ�
            if (btn1 == 1 || btn2 == 0)
            {
                serialPort.Write("L"); // LED �ѱ�
                Debug.Log("LED �ѱ�");
                serialPort.Write("B");
                Debug.Log("���� �︮��");
            }
            else
            {
                serialPort.Write("l"); // LED ����
                Debug.Log("LED ����");
                serialPort.Write("b");
                Debug.Log("���� ����");
            }
        }
    }

    private void PushButton01()
    {
        if (btn1 == 1)
        {
            GunController.Instance.Fire_MachineGun();

        }
    }

    private void PushButton02()
    {
        if (btn2 == 0)
        {
            GunController.Instance.Fire_Missile();
        }
    }

    void OnDestroy()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("Arduino ���� ����");
        }
    }

    void OnApplicationQuit()
    {
        OnDestroy();
    }
}