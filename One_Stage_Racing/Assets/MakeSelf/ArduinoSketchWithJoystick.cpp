// 핀 정의
const int JOY_X = A0;
const int JOY_Y = A1;
const int BTN1 = 2;
const int BTN2 = 3;
const int LED = 4;
const int BUZZER = 5;

// 조이스틱 중앙값 보정을 위한 변수
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
  
  // 시작 시 중앙값 보정
  delay(1000);
  calibrateJoystick();
  
  Serial.println("ARDUINO_READY");
}

void calibrateJoystick() {
  // 시작 시 조이스틱의 중앙값을 측정
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
  // 아날로그 값 읽기
  int xVal = analogRead(JOY_X);
  int yVal = analogRead(JOY_Y);
  int btn1State = !digitalRead(BTN1);
  int btn2State = !digitalRead(BTN2);

  // 중앙값 기준으로 보정 및 데드존 적용
  int deadZone = 30; // 데드존 크기
  
  if (abs(xVal - centerX) < deadZone) {
    xVal = centerX; // 중앙값으로 설정
  }
  
  if (abs(yVal - centerY) < deadZone) {
    yVal = centerY; // 중앙값으로 설정
  }

  // 데이터 전송
  Serial.print(xVal);
  Serial.print(",");
  Serial.print(yVal);
  Serial.print(",");
  Serial.print(btn1State);
  Serial.print(",");
  Serial.print(btn2State);
  Serial.println();

  // Unity에서 명령 수신
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
