using UnityEngine;
using System.IO.Ports;

public class ArduinoCubeController : MonoBehaviour
{
    [Header("Arduino Settings")]
    public string portName = "COM4"; // 실제 포트로 변경
    public int baudRate = 9600;
    
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sensitivity = 0.3f; // 조이스틱 민감도
    
    private SerialPort serialPort;
    private float joyX, joyY;
    private int btn1, btn2;
    
    void Start()
    {
        // 시리얼 포트 초기화
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 50;
            serialPort.Open();
            Debug.Log("Arduino 연결 성공: " + portName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Arduino 연결 실패: " + e.Message);
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
                
                Debug.Log("받은 원본 데이터: [" + data + "]");
                
                string[] values = data.Split(',');
                
                if (values.Length >= 4)
                {
                    if (int.TryParse(values[0], out int xRaw) &&
                        int.TryParse(values[1], out int yRaw) &&
                        int.TryParse(values[2], out int b1) &&
                        int.TryParse(values[3], out int b2))
                    {
                        // X축 반전 (좌/우 반대로)
                        joyX = -(xRaw - 512f) / 512f; // 마이너스 부호 추가
                        joyY = (yRaw - 512f) / 512f;
                        btn1 = b1;
                        btn2 = b2;
                        
                        // 데드존 적용 (더 넓게 설정)
                        if (Mathf.Abs(joyX) < sensitivity) joyX = 0f;
                        if (Mathf.Abs(joyY) < sensitivity) joyY = 0f;
                        
                        Debug.Log($"파싱 성공 - 조이스틱: ({joyX:F2}, {joyY:F2}), 버튼: ({btn1}, {btn2})");
                    }
                    else
                    {
                        Debug.LogWarning("숫자 변환 실패: " + data);
                    }
                }
                else
                {
                    Debug.LogWarning("데이터 형식 오류 (길이: " + values.Length + "): " + data);
                }
            }
        }
        catch (System.TimeoutException)
        {
            // 타임아웃은 정상적인 상황이므로 무시
        }
        catch (System.Exception e)
        {
            Debug.LogError("데이터 읽기 오류: " + e.Message);
        }
    }
}

void MoveCube()
{
    // 조이스틱 입력으로 큐브 이동 (X축이 이제 반전됨)
    Vector3 movement = new Vector3(joyX, 0, joyY) * moveSpeed * Time.deltaTime;
    transform.Translate(movement);
    
    // 버튼1: 위로 이동
    if (btn1 == 1)
    {
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
    }
    
    // 버튼2: 아래로 이동
    if (btn2 == 1)
    {
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
    }
    
    // 아두이노로 피드백 전송 (움직일 때만)
    SendFeedback();
}

void SendFeedback()
{
    if (serialPort != null && serialPort.IsOpen)
    {
        // 큐브가 움직이고 있으면 LED 켜기 (조이스틱이나 버튼 입력이 있을 때)
        if (Mathf.Abs(joyX) > 0.01f || Mathf.Abs(joyY) > 0.01f || btn1 == 1 || btn2 == 1)
        {
            serialPort.Write("L"); // LED 켜기
        }
        else
        {
            serialPort.Write("l"); // LED 끄기
        }
        
        // 버튼이 눌리면 부저 울리기
        if (btn1 == 1 || btn2 == 1)
        {
            serialPort.Write("B"); // 부저 울리기
        }
    }
}
    
    void OnDestroy()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("Arduino 연결 종료");
        }
    }
    
    void OnApplicationQuit()
    {
        OnDestroy();
    }
}
