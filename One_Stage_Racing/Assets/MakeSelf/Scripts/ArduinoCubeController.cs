using UnityEngine;
using System.IO.Ports;

public class ArduinoCubeController : MonoBehaviour
{
    [Header("Arduino Settins")]//Inspector���� �ð�ȭ ������ ����
    public string portName = "COM4";//�ø��� ��Ʈ �̸�
    public int baudRate = 9600;// Serial.begin()���� ���Ǵ� ���巹��Ʈ

    [Header("Movement Settings)")]
    public float moveSpeed = 5.0f;//�̵� �ӵ�
    public float sensitivity = 0.3f;//�ΰ���
    public Rigidbody rb;

    private SerialPort serialPort;//�ø��� ��Ʈ ��ü
    private float joyX, joyY;//���̽�ƽ �Է� ��
    private int btn1, btn2;//��ư �Է� ��

    private void Start()//���� ���� �� ù 1ȸ ȣ��Ǵ� �ý��۸޼���.
    {
        rb = GetComponent<Rigidbody>();//Rigidbody ������Ʈ ��������
        if (rb == null)//Rigidbody�� ���ٸ�
        {
            rb = gameObject.AddComponent<Rigidbody>();//Rigidbody ������Ʈ�� �߰�
        }
        rb.useGravity = false; // �߷� ��Ȱ��ȭ

        try
        {
            serialPort = new SerialPort(portName, baudRate);//�ø��� ��Ʈ �ʱ�ȭ
            serialPort.ReadTimeout = 50;//�б� Ÿ�Ӿƿ� ����
            serialPort.Open();//�ø��� ��Ʈ ����
            Debug.Log("Serial Port Opened: " + portName);//�ܼ� �α׿� ���

        }
        catch (System.Exception e)//try-catch ����� ����Ͽ� ���� ó��
        {
            Debug.LogError("Failed to open serial port: " + e.Message);//�ø��� ��Ʈ ���� ���� �� ���� �޽��� ���
        }
    }

    private void Update()// ���� ���� �� �� �����Ӹ��� ȣ��Ǵ� �ý��� �޼���.
    {
        ReadArduinoData();// ����Ƽ�� Update()�޼���� �� �����Ӹ��� �ұ�Ģ ȣ��Ǳ� ������, �Է��� ���� ������Ʈ�� �̵�, ���� ��� ���� �ʿ��� ����� �ְԵǸ� ���� ����, ������ ��� ���� �߻��� �� ����.
        SendFeedback();//Update()������ �Ƶ��̳� ������ �б⸸ ó���ϰ�, ���� �����͸� ���� ������Ʈ �Է����� ó���ϴ� ����� FixedUpdate()���� ó��
         MoveObject();//�Ƶ��̳� �����͸� �о�� ������Ʈ�� �̵���Ű�� �޼��� ȣ��
    }

    private void FixedUpdate()//����Ƽ�� FixedUpdate()�� Update()�� �޸� ������ ��� ȣ���� �ƴ϶�, ������ Ÿ�ӽ��ܿ� ������ ���� ���� ���� �������� ȣ��Ǵ� �ý��۸޼���.
    {
       
    }

    private void ReadArduinoData()
    {
        if (serialPort != null && serialPort.IsOpen)//�ø��� ��Ʈ�� ���� �ִ��� Ȯ��
        {
            try
            {
                if (serialPort.BytesToRead > 0)//�ø��� ��Ʈ�� ���� �����Ͱ� �����ϴ� �� Ȯ��
                {
                    string data = serialPort.ReadLine().Trim();//�ø��� ��Ʈ���� �� �� �б�

                    if (string.IsNullOrEmpty(data))//���� �����Ͱ� ����ִ��� Ȯ��
                        return;//����ִٸ� �ƹ��͵� ���� ����

                    string[] values = data.Split(',');//�Ƶ��̳��� �ø��� ��Ʈ�� ������ ���� �����͸� ��ǥ�� �и��Ͽ� �迭�� ����

                    if (values.Length >= 4)
                    {
                        if (int.TryParse(values[0], out int xRaw) &&
                            int.TryParse(values[1], out int yRaw) &&
                            int.TryParse(values[2], out int b1) &&
                            int.TryParse(values[3], out int b2))
                        {
                            //�ε巯�� �������� ���� ���� ó��
                            float newJoyX = -(xRaw - 512f) / 512f;
                            float newJoyY = (yRaw - 512f) / 512f;

                            //������ ����
                            if (Mathf.Abs(newJoyX) < sensitivity)
                            {
                                newJoyX = 0f;//���̽�ƽ X�� �Է��� �ΰ��� ���϶�� 0���� ����
                            }
                            if (Mathf.Abs(newJoyY) < sensitivity)
                            {
                                newJoyY = 0f;//���̽�ƽ Y�� �Է��� �ΰ��� ���϶�� 0���� ����
                            }

                            //�ε巯�� ��ȯ�� ���� Lerp ����
                            joyX = Mathf.Lerp(joyX, newJoyX, Time.deltaTime * 10.0f);
                            joyY = Mathf.Lerp(joyY, newJoyY, Time.deltaTime * 10.0f);

                            btn1 = b1;//��ư 1 �Է� �� ����
                            btn2 = b2;//��ư 2 �Է� �� ����
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error reading from serial port: " + e.Message);//�ø��� ��Ʈ �б� �� ���� �߻� �� ���� �޽��� ���
            }
        }
    }

    private void MoveObject()//�Ƶ��̳� �����͸� �о�� ������Ʈ�� �̵���Ű�� �޼���
    {
        Vector3 movement = new Vector3(joyX, 0, joyY) * moveSpeed * Time.fixedDeltaTime;//���̽�ƽ �Է¿� ���� �̵� ���� ���
        //transform.Translate(movement);//������Ʈ �̵�
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);//Rigidbody�� ����Ͽ� ������Ʈ �̵�
        

        if (btn1 == 1)// ��ư 1 �Է� ó��
        {

        }
        if (btn2 == 2)// ��ư 2 �Է� ó��
        {

        }
    }

    private void SendFeedback()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            if (Mathf.Abs(joyX) > 0.01f || Mathf.Abs(joyY) > 0.01f || btn1 == 1 || btn2 == 1)//���̽�ƽ �Է��̳� ��ư �Է��� �ִ� ��쿡�� �ǵ�� ����
            {
                string feedback = $"{joyX},{joyY},{btn1},{btn2}\n"; //�Ƶ��̳�� �ǵ�� ����
                serialPort.Write(feedback);//�ø��� ��Ʈ�� �ǵ�� ����
                serialPort.Write("L"); // LED �ѱ�
            }
            else
            {
                serialPort.Write("l"); // LED ����
            }
            
            if (btn1 == 1 || btn2 == 1) // ��ư 1 �Ǵ� ��ư 2�� ������ ��
            {
                serialPort.Write("B"); // ���� �ѱ�
            }
        }
    }

    private void OnDestroy()//���� �� ȣ��Ǵ� �޼���
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();//�ø��� ��Ʈ �ݱ�
            Debug.Log("Serial Port Closed: " + portName);//�ܼ� �α׿� ���
        }
    }

    private void OnApplicationQuit()//���ø����̼� ���� �� ȣ��Ǵ� �޼���
    {
        OnDestroy();//���� �� �ø��� ��Ʈ �ݱ�
    }
}
