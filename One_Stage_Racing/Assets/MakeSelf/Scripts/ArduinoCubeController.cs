using UnityEngine;
using System.IO.Ports;

public class ArduinoCubeController : MonoBehaviour
{
    [Header("Arduino Settings")]
    public string portName = "COM4"; // ���� ��Ʈ�� ����
    public int baudRate = 9600;
    
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sensitivity = 0.3f; // ���̽�ƽ �ΰ���
    
    private SerialPort serialPort;
    private float joyX, joyY;
    private int btn1, btn2;
    
    void Start()
    {
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
        MoveCube();
    }
    
void ReadArduinoData()
{
    if (serialPort != null && serialPort.IsOpen)
    {
        try
        {
            if (serialPort.BytesToRead > 0)
            {
                string data = serialPort.ReadLine().Trim();
                
                if (string.IsNullOrEmpty(data))
                    return;
                
                Debug.Log("���� ���� ������: [" + data + "]");
                
                string[] values = data.Split(',');
                
                if (values.Length >= 4)
                {
                    if (int.TryParse(values[0], out int xRaw) &&
                        int.TryParse(values[1], out int yRaw) &&
                        int.TryParse(values[2], out int b1) &&
                        int.TryParse(values[3], out int b2))
                    {
                        // X�� ���� (��/�� �ݴ��)
                        joyX = -(xRaw - 512f) / 512f; // ���̳ʽ� ��ȣ �߰�
                        joyY = (yRaw - 512f) / 512f;
                        btn1 = b1;
                        btn2 = b2;
                        
                        // ������ ���� (�� �а� ����)
                        if (Mathf.Abs(joyX) < sensitivity) joyX = 0f;
                        if (Mathf.Abs(joyY) < sensitivity) joyY = 0f;
                        
                        Debug.Log($"�Ľ� ���� - ���̽�ƽ: ({joyX:F2}, {joyY:F2}), ��ư: ({btn1}, {btn2})");
                    }
                    else
                    {
                        Debug.LogWarning("���� ��ȯ ����: " + data);
                    }
                }
                else
                {
                    Debug.LogWarning("������ ���� ���� (����: " + values.Length + "): " + data);
                }
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

void MoveCube()
{
    // ���̽�ƽ �Է����� ť�� �̵� (X���� ���� ������)
    Vector3 movement = new Vector3(joyX, 0, joyY) * moveSpeed * Time.deltaTime;
    transform.Translate(movement);
    
    // ��ư1: ���� �̵�
    if (btn1 == 1)
    {
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
    }
    
    // ��ư2: �Ʒ��� �̵�
    if (btn2 == 1)
    {
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
    }
    
    // �Ƶ��̳�� �ǵ�� ���� (������ ����)
    SendFeedback();
}

void SendFeedback()
{
    if (serialPort != null && serialPort.IsOpen)
    {
        // ť�갡 �����̰� ������ LED �ѱ� (���̽�ƽ�̳� ��ư �Է��� ���� ��)
        if (Mathf.Abs(joyX) > 0.01f || Mathf.Abs(joyY) > 0.01f || btn1 == 1 || btn2 == 1)
        {
            serialPort.Write("L"); // LED �ѱ�
        }
        else
        {
            serialPort.Write("l"); // LED ����
        }
        
        // ��ư�� ������ ���� �︮��
        if (btn1 == 1 || btn2 == 1)
        {
            serialPort.Write("B"); // ���� �︮��
        }
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
