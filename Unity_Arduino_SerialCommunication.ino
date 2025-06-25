// ==================== Arduino DIY 게임 컨트롤러 코드 ====================
// IoT 시스템 기초 최종 프로젝트 - Team Unite
// 작성자: 신범수 (20214930)
// 목적: Unity 3D 레이싱 게임과 연동되는 DIY 게임 컨트롤러 구현

// ==================== 핀 정의 ====================
const int JOY_X = A0;    // 조이스틱 X축 아날로그 입력 핀 (좌우 조향)
const int JOY_Y = A1;    // 조이스틱 Y축 아날로그 입력 핀 (전후 이동)
const int BTN1 = 4;      // 버튼1 - 머신건 발사 (디지털 입력, 풀업 저항 사용)
const int BTN2 = 3;      // 버튼2 - 미사일 발사 (디지털 입력, 풀업 저항 사용)
const int LED = 11;      // LED 출력 핀 (게임 상태 시각적 피드백)
const int BUZZER = 8;    // 부저 출력 핀 (게임 이벤트 청각적 피드백)

// ==================== 조이스틱 보정 변수 ====================
int centerX = 512;       // 조이스틱 X축 중앙값 (아날로그 읽기 범위 0-1023의 중간값)
int centerY = 512;       // 조이스틱 Y축 중앙값
bool calibrated = false; // 조이스틱 보정 완료 여부를 나타내는 플래그

// ==================== 초기화 함수 ====================
void setup() {
  // 각 핀의 입출력 모드 설정
  pinMode(JOY_X, INPUT);        // 조이스틱 X축을 아날로그 입력으로 설정
  pinMode(JOY_Y, INPUT);        // 조이스틱 Y축을 아날로그 입력으로 설정
  pinMode(BTN1, INPUT_PULLUP);  // 버튼1을 내부 풀업 저항 사용 입력으로 설정
  pinMode(BTN2, INPUT_PULLUP);  // 버튼2를 내부 풀업 저항 사용 입력으로 설정
  pinMode(LED, OUTPUT);         // LED를 디지털 출력으로 설정
  pinMode(BUZZER, OUTPUT);      // 부저를 디지털 출력으로 설정
  
  Serial.begin(9600);           // 시리얼 통신 초기화 (9600 baud rate)
  
  delay(1000);                  // 시스템 안정화를 위한 1초 대기
  calibrateJoystick();          // 조이스틱 중앙값 자동 보정 함수 호출
  
  // Unity에 컨트롤러 준비 완료 신호 전송
  Serial.println("RACING_CONTROLLER_READY");
}

// ==================== 조이스틱 보정 함수 ====================
void calibrateJoystick() {
  // 조이스틱의 현재 위치를 중앙값으로 설정하여 드리프트 현상 방지
  // 여러 번 측정하여 평균값을 구함으로써 노이즈 제거
  
  long sumX = 0, sumY = 0;      // X, Y축 값의 누적 합계를 저장할 변수
  
  // 20회 반복 측정하여 안정적인 중앙값 계산
  for(int i = 0; i < 20; i++) {
    sumX += analogRead(JOY_X);  // X축 아날로그 값 누적
    sumY += analogRead(JOY_Y);  // Y축 아날로그 값 누적
    delay(10);                  // 각 측정 간 10ms 대기 (안정적인 읽기)
  }
  
  // 평균값을 계산하여 중앙값으로 설정
  centerX = sumX / 20;          // X축 중앙값 = 20회 측정값의 평균
  centerY = sumY / 20;          // Y축 중앙값 = 20회 측정값의 평균
  calibrated = true;            // 보정 완료 플래그 설정
  
  // 보정 완료를 사용자에게 시각적으로 알리는 LED 신호
  digitalWrite(LED, HIGH);      // LED 켜기
  delay(500);                   // 0.5초 동안 LED 점등 유지
  digitalWrite(LED, LOW);       // LED 끄기 (보정 완료 신호 종료)
}

// ==================== 메인 루프 함수 ====================
void loop() {
  // ========== 센서 데이터 읽기 섹션 ==========
  int xVal = analogRead(JOY_X);        // 조이스틱 X축 값 읽기 (0-1023 범위)
  int yVal = analogRead(JOY_Y);        // 조이스틱 Y축 값 읽기 (0-1023 범위)
  int btn1State = digitalRead(BTN1);   // 버튼1 상태 읽기 (머신건 발사 버튼)
  int btn2State = !digitalRead(BTN2);  // 버튼2 상태 읽기 (미사일 발사, 논리 반전)
  
  // ========== 데드존 처리 섹션 ==========
  // 조이스틱의 미세한 움직임이나 하드웨어 노이즈로 인한 
  // 의도하지 않은 입력을 방지하기 위한 데드존 적용
  
  int deadZone = 40;                   // 데드존 크기 설정 (±40 범위)
  
  // X축 데드존 처리: 중앙값 기준으로 ±40 범위 내에서는 중앙값으로 고정
  if (abs(xVal - centerX) < deadZone) {
    xVal = centerX;                    // 데드존 내부면 중앙값으로 설정
  }
  
  // Y축 데드존 처리: 중앙값 기준으로 ±40 범위 내에서는 중앙값으로 고정
  if (abs(yVal - centerY) < deadZone) {
    yVal = centerY;                    // 데드존 내부면 중앙값으로 설정
  }

  // ========== Unity로 데이터 전송 섹션 ==========
  // CSV(Comma-Separated Values) 형식으로 데이터 전송
  // 형식: "X값,Y값,버튼1상태,버튼2상태\n"
  
  Serial.print(xVal);                  // X축 값 전송
  Serial.print(",");                   // CSV 구분자
  Serial.print(yVal);                  // Y축 값 전송
  Serial.print(",");                   // CSV 구분자
  Serial.print(btn1State);             // 버튼1 상태 전송 (0: 눌림, 1: 안눌림)
  Serial.print(",");                   // CSV 구분자
  Serial.print(btn2State);             // 버튼2 상태 전송 (1: 눌림, 0: 안눌림)
  Serial.println();                    // 줄바꿈 문자로 데이터 패킷 종료

  // ========== Unity에서 피드백 명령 수신 섹션 ==========
  // Unity에서 게임 상태에 따른 피드백 명령을 받아 LED/부저 제어
  
  if (Serial.available()) {            // 수신 버퍼에 데이터가 있는지 확인
    char cmd = Serial.read();          // 명령 문자 하나 읽기
    
    // 명령 문자에 따른 동작 수행
    switch(cmd) {
      case 'L':                        // LED 켜기 명령 (대문자 L)
        digitalWrite(LED, HIGH);       // LED 점등 (고속 주행/활성 상태 표시)
        break;
        
      case 'l':                        // LED 끄기 명령 (소문자 l)
        digitalWrite(LED, LOW);        // LED 소등 (정지/비활성 상태 표시)
        break;
        
      case 'B':                        // 부저 울리기 명령 (대문자 B)
        digitalWrite(BUZZER, HIGH);    // 부저 켜기
        delay(200);                    // 200ms 동안 소리 지속
        digitalWrite(BUZZER, LOW);     // 부저 끄기 (충돌/경고 효과)
        break;
        
      case 'b':                        // 부저 끄기 명령 (소문자 b)
        digitalWrite(BUZZER, LOW);     // 부저 강제 끄기
        break;
    }
  }

  // ========== 루프 주기 제어 ==========
  delay(20);                           // 20ms 대기 (50Hz 업데이트 주기)
                                       // 레이싱 게임에 적합한 빠른 응답성 보장
}