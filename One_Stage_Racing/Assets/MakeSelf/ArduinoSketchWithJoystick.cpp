// �� ����
const int JOY_X = A0;
const int JOY_Y = A1;
const int BTN1 = 2;
const int BTN2 = 3;
const int LED = 4;
const int BUZZER = 5;

// ���̽�ƽ �߾Ӱ� ������ ���� ����
int centerX = 512;
int centerY = 512;
bool calibrated = false;

void setup() {
  pinMode(JOY_X, INPUT);
  pinMode(JOY_Y, INPUT);
  pinMode(BTN1, INPUT_PULLUP);
  pinMode(BTN2, INPUT_PULLUP);
  pinMode(LED, OUTPUT);
  pinMode(BUZZER, OUTPUT);
  Serial.begin(9600);
  
  // ���� �� �߾Ӱ� ����
  delay(1000);
  calibrateJoystick();
  
  Serial.println("ARDUINO_READY");
}

void calibrateJoystick() {
  // ���� �� ���̽�ƽ�� �߾Ӱ��� ����
  long sumX = 0, sumY = 0;
  for(int i = 0; i < 10; i++) {
    sumX += analogRead(JOY_X);
    sumY += analogRead(JOY_Y);
    delay(10);
  }
  centerX = sumX / 10;
  centerY = sumY / 10;
  calibrated = true;
}

void loop() {
  // �Ƴ��α� �� �б�
  int xVal = analogRead(JOY_X);
  int yVal = analogRead(JOY_Y);
  int btn1State = !digitalRead(BTN1);
  int btn2State = !digitalRead(BTN2);

  // �߾Ӱ� �������� ���� �� ������ ����
  int deadZone = 30; // ������ ũ��
  
  if (abs(xVal - centerX) < deadZone) {
    xVal = centerX; // �߾Ӱ����� ����
  }
  
  if (abs(yVal - centerY) < deadZone) {
    yVal = centerY; // �߾Ӱ����� ����
  }

  // ������ ����
  Serial.print(xVal);
  Serial.print(",");
  Serial.print(yVal);
  Serial.print(",");
  Serial.print(btn1State);
  Serial.print(",");
  Serial.print(btn2State);
  Serial.println();

  // Unity���� ��� ����
  if (Serial.available()) {
    char cmd = Serial.read();
    switch(cmd) {
      case 'L':
        digitalWrite(LED, HIGH);
        break;
      case 'l':
        digitalWrite(LED, LOW);
        break;
      case 'B':
        digitalWrite(BUZZER, HIGH);
        delay(100);
        digitalWrite(BUZZER, LOW);
        break;
    }
  }

  delay(50);
}
