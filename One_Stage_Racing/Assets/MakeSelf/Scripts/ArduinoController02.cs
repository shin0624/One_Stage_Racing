using UnityEngine;
using System.IO.Ports;
using CarControllerwithShooting;

public class ArduinoController02 : MonoBehaviour
{
    [Header("Arduino Settings")]
    public string portName = "COM4"; // 실제 포트로 변경
    public int baudRate = 9600;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float maxSpeed = 20f; // 최대 속도
    public float acceleration = 2f; // 가속도
    public float deceleration = 8f; // 감속도
    public float sensitivity = 0.3f; // 조이스틱 민감도
    
    [Header("Car Settings")]
    public bool keepYPosition = true; // Y축 위치 고정 여부
    public float groundY = 0f; // 지면 높이 설정
    [Header("Smoothing Settings")]
    public float smoothingFactor = 0.1f; // 움직임 부드럽게 만들기
    public int dataBufferSize = 3; // 데이터 평균화를 위한 버퍼 크기

    private SerialPort serialPort;
    private float joyX, joyY;
    private int btn1, btn2;
    
    // 부드러운 움직임을 위한 변수들
    private float smoothedJoyX, smoothedJoyY;
    private float[] joyXBuffer, joyYBuffer;
    private int bufferIndex = 0;
    
    // 가속도 시스템을 위한 변수들
    public float currentSpeedX = 0f;
    public float currentSpeedY = 0f;
    private Vector3 lastDirection = Vector3.zero;
    
    // 데이터 검증을 위한 변수들
    private float lastValidJoyX, lastValidJoyY;
    private float maxJoyChange = 0.5f; // 한 프레임당 최대 변화량 제한

    void Start()
    {
        // 버퍼 초기화
        joyXBuffer = new float[dataBufferSize];
        joyYBuffer = new float[dataBufferSize];
        
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
                // 여러 데이터가 쌓여있을 수 있으므로 가장 최신 데이터만 사용
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

                Debug.Log("받은 원본 데이터: [" + latestData + "]");

                string[] values = latestData.Split(',');

                if (values.Length >= 4)
                {
                    if (int.TryParse(values[0], out int xRaw) &&
                        int.TryParse(values[1], out int yRaw) &&
                        int.TryParse(values[2], out int b1) &&
                        int.TryParse(values[3], out int b2))
                    {
                        // 조이스틱 값 계산
                        float newJoyX = -(xRaw - 512f) / 512f;
                        float newJoyY = (yRaw - 512f) / 512f;
                        
                        // 급격한 변화 제한 (노이즈 필터링)
                        newJoyX = Mathf.Clamp(newJoyX, lastValidJoyX - maxJoyChange, lastValidJoyX + maxJoyChange);
                        newJoyY = Mathf.Clamp(newJoyY, lastValidJoyY - maxJoyChange, lastValidJoyY + maxJoyChange);
                        
                        // 버퍼에 값 저장
                        joyXBuffer[bufferIndex] = newJoyX;
                        joyYBuffer[bufferIndex] = newJoyY;
                        bufferIndex = (bufferIndex + 1) % dataBufferSize;
                        
                        // 평균값 계산
                        float avgJoyX = 0f, avgJoyY = 0f;
                        for (int i = 0; i < dataBufferSize; i++)
                        {
                            avgJoyX += joyXBuffer[i];
                            avgJoyY += joyYBuffer[i];
                        }
                        avgJoyX /= dataBufferSize;
                        avgJoyY /= dataBufferSize;
                        
                        // 데드존 적용
                        if (Mathf.Abs(avgJoyX) < sensitivity) avgJoyX = 0f;
                        if (Mathf.Abs(avgJoyY) < sensitivity) avgJoyY = 0f;
                        
                        // 최종 값 저장
                        joyX = avgJoyX;
                        joyY = avgJoyY;
                        lastValidJoyX = avgJoyX;
                        lastValidJoyY = avgJoyY;
                        
                        btn1 = b1;
                        btn2 = b2;

                        Debug.Log($"파싱 성공 - 조이스틱: ({joyX:F2}, {joyY:F2}), 버튼: ({btn1}, {btn2})");
                    }
                    else
                    {
                        Debug.LogWarning("숫자 변환 실패: " + latestData);
                    }
                }
                else
                {
                    Debug.LogWarning("데이터 형식 오류 (길이: " + values.Length + "): " + latestData);
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

    void MoveCar()
    {
        // 부드러운 움직임 적용
        smoothedJoyX = Mathf.Lerp(smoothedJoyX, joyX, smoothingFactor);
        smoothedJoyY = Mathf.Lerp(smoothedJoyY, joyY, smoothingFactor);
        
        // 매우 작은 값은 0으로 처리 (미세한 떨림 방지)
        if (Mathf.Abs(smoothedJoyX) < 0.01f) smoothedJoyX = 0f;
        if (Mathf.Abs(smoothedJoyY) < 0.01f) smoothedJoyY = 0f;
        
        // 현재 입력 방향 계산
        Vector3 inputDirection = new Vector3(smoothedJoyX, 0, smoothedJoyY).normalized;
        bool hasInput = smoothedJoyX != 0f || smoothedJoyY != 0f;
        
        if (hasInput)
        {
            // 입력이 있을 때 - 가속
            // 같은 방향으로 계속 움직이는지 확인
            bool sameDirection = Vector3.Dot(inputDirection, lastDirection) > 0.8f;
            
            if (sameDirection)
            {
                // 같은 방향이면 가속
                currentSpeedX = Mathf.Min(currentSpeedX + acceleration * Time.deltaTime, maxSpeed);
                currentSpeedY = Mathf.Min(currentSpeedY + acceleration * Time.deltaTime, maxSpeed);
            }
            else
            {
                // 방향이 바뀌면 기본 속도로 리셋
                currentSpeedX = moveSpeed;
                currentSpeedY = moveSpeed;
            }
            lastDirection = inputDirection;
        }
        else
        {
            // 입력이 없을 때 - 감속
            currentSpeedX = Mathf.Max(currentSpeedX - deceleration * Time.deltaTime, 0f);
            currentSpeedY = Mathf.Max(currentSpeedY - deceleration * Time.deltaTime, 0f);
        }
        
        // 실제 이동 계산 (각 축별로 다른 속도 적용)
        float moveX = smoothedJoyX * currentSpeedX * Time.deltaTime;
        float moveZ = smoothedJoyY * currentSpeedY * Time.deltaTime;
        
        Vector3 movement = new Vector3(moveX, 0, moveZ);
        
        if (keepYPosition)
        {
            // Y축 위치를 지면 높이로 고정
            Vector3 currentPos = transform.position;
            Vector3 newPos = currentPos + movement;
            newPos.y = groundY; // 지면 높이로 고정
            transform.position = newPos;
        }
        else
        {
            // 일반적인 이동 (Y축 변화 허용)
            transform.Translate(movement);
        }
        // 아두이노로 피드백 전송
        SendFeedback();
        
        // 디버그 정보 출력
        if (hasInput)
        {
            Debug.Log($"현재 속도: X={currentSpeedX:F1}, Y={currentSpeedY:F1}, 입력: ({smoothedJoyX:F2}, {smoothedJoyY:F2})");
        }
    }

    void SendFeedback()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            // 버튼이 눌리면 LED 켜기
            if (btn1 == 1 || btn2 == 0)
            {
                serialPort.Write("L"); // LED 켜기
                Debug.Log("LED 켜기");
                serialPort.Write("B");
                Debug.Log("부저 울리기");
            }
            else
            {
                serialPort.Write("l"); // LED 끄기
                Debug.Log("LED 끄기");
                serialPort.Write("b");
                Debug.Log("부저 끄기");
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
            Debug.Log("Arduino 연결 종료");
        }
    }

    void OnApplicationQuit()
    {
        OnDestroy();
    }
}