using UnityEngine;
using System.IO.Ports;

public class ArduinoCubeController : MonoBehaviour
{
    [Header("Arduino Settins")]//Inspector에서 시각화 가능한 변수
    public string portName = "COM4";//시리얼 포트 이름
    public int baudRate = 9600;// Serial.begin()에서 사용되는 보드레이트

    [Header("Movement Settings)")]
    public float moveSpeed = 5.0f;//이동 속도
    public float sensitivity = 0.3f;//민감도
    public Rigidbody rb;

    private SerialPort serialPort;//시리얼 포트 객체
    private float joyX, joyY;//조이스틱 입력 값
    private int btn1, btn2;//버튼 입력 값

    private void Start()//게임 시작 시 첫 1회 호출되는 시스템메서드.
    {
        rb = GetComponent<Rigidbody>();//Rigidbody 컴포넌트 가져오기
        if (rb == null)//Rigidbody가 없다면
        {
            rb = gameObject.AddComponent<Rigidbody>();//Rigidbody 컴포넌트를 추가
        }
        rb.useGravity = false; // 중력 비활성화

        try
        {
            serialPort = new SerialPort(portName, baudRate);//시리얼 포트 초기화
            serialPort.ReadTimeout = 50;//읽기 타임아웃 설정
            serialPort.Open();//시리얼 포트 열기
            Debug.Log("Serial Port Opened: " + portName);//콘솔 로그에 출력

        }
        catch (System.Exception e)//try-catch 블록을 사용하여 예외 처리
        {
            Debug.LogError("Failed to open serial port: " + e.Message);//시리얼 포트 열기 실패 시 에러 메시지 출력
        }
    }

    private void Update()// 게임 시작 후 매 프레임마다 호출되는 시스템 메서드.
    {
        ReadArduinoData();// 유니티의 Update()메서드는 매 프레임마다 불규칙 호출되기 때문에, 입력을 통한 오브젝트의 이동, 물리 계산 등이 필요한 기능을 넣게되면 성능 저하, 프레임 드랍 등이 발생될 수 있음.
        SendFeedback();//Update()에서는 아두이노 데이터 읽기만 처리하고, 읽은 데이터를 실제 오브젝트 입력으로 처리하는 기능은 FixedUpdate()에서 처리
         MoveObject();//아두이노 데이터를 읽어와 오브젝트를 이동시키는 메서드 호출
    }

    private void FixedUpdate()//유니티의 FixedUpdate()는 Update()와 달리 프레임 기반 호출이 아니라, 고정된 타임스텝에 설정된 값에 따라 일정 간격으로 호출되는 시스템메서드.
    {
       
    }

    private void ReadArduinoData()
    {
        if (serialPort != null && serialPort.IsOpen)//시리얼 포트가 열려 있는지 확인
        {
            try
            {
                if (serialPort.BytesToRead > 0)//시리얼 포트에 읽을 데이터가 존재하는 지 확인
                {
                    string data = serialPort.ReadLine().Trim();//시리얼 포트에서 한 줄 읽기

                    if (string.IsNullOrEmpty(data))//읽은 데이터가 비어있는지 확인
                        return;//비어있다면 아무것도 하지 않음

                    string[] values = data.Split(',');//아두이노의 시리얼 포트로 들어오느 센서 데이터를 쉼표로 분리하여 배열로 저장

                    if (values.Length >= 4)
                    {
                        if (int.TryParse(values[0], out int xRaw) &&
                            int.TryParse(values[1], out int yRaw) &&
                            int.TryParse(values[2], out int b1) &&
                            int.TryParse(values[3], out int b2))
                        {
                            //부드러운 움직임을 위한 보간 처리
                            float newJoyX = -(xRaw - 512f) / 512f;
                            float newJoyY = (yRaw - 512f) / 512f;

                            //데드존 적용
                            if (Mathf.Abs(newJoyX) < sensitivity)
                            {
                                newJoyX = 0f;//조이스틱 X축 입력이 민감도 이하라면 0으로 설정
                            }
                            if (Mathf.Abs(newJoyY) < sensitivity)
                            {
                                newJoyY = 0f;//조이스틱 Y축 입력이 민감도 이하라면 0으로 설정
                            }

                            //부드러운 전환을 위한 Lerp 적용
                            joyX = Mathf.Lerp(joyX, newJoyX, Time.deltaTime * 10.0f);
                            joyY = Mathf.Lerp(joyY, newJoyY, Time.deltaTime * 10.0f);

                            btn1 = b1;//버튼 1 입력 값 저장
                            btn2 = b2;//버튼 2 입력 값 저장
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error reading from serial port: " + e.Message);//시리얼 포트 읽기 중 에러 발생 시 에러 메시지 출력
            }
        }
    }

    private void MoveObject()//아두이노 데이터를 읽어와 오브젝트를 이동시키는 메서드
    {
        Vector3 movement = new Vector3(joyX, 0, joyY) * moveSpeed * Time.fixedDeltaTime;//조이스틱 입력에 따라 이동 벡터 계산
        //transform.Translate(movement);//오브젝트 이동
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);//Rigidbody를 사용하여 오브젝트 이동
        

        if (btn1 == 1)// 버튼 1 입력 처리
        {

        }
        if (btn2 == 2)// 버튼 2 입력 처리
        {

        }
    }

    private void SendFeedback()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            if (Mathf.Abs(joyX) > 0.01f || Mathf.Abs(joyY) > 0.01f || btn1 == 1 || btn2 == 1)//조이스틱 입력이나 버튼 입력이 있는 경우에만 피드백 전송
            {
                string feedback = $"{joyX},{joyY},{btn1},{btn2}\n"; //아두이노로 피드백 전송
                serialPort.Write(feedback);//시리얼 포트로 피드백 전송
                serialPort.Write("L"); // LED 켜기
            }
            else
            {
                serialPort.Write("l"); // LED 끄기
            }
            
            if (btn1 == 1 || btn2 == 1) // 버튼 1 또는 버튼 2가 눌렸을 때
            {
                serialPort.Write("B"); // 부저 켜기
            }
        }
    }

    private void OnDestroy()//종료 시 호출되는 메서드
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();//시리얼 포트 닫기
            Debug.Log("Serial Port Closed: " + portName);//콘솔 로그에 출력
        }
    }

    private void OnApplicationQuit()//애플리케이션 종료 시 호출되는 메서드
    {
        OnDestroy();//종료 시 시리얼 포트 닫기
    }
}
