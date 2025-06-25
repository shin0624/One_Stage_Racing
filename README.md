# Arduino DIY Controller Unity 3D Racing Game

![Image](https://github.com/user-attachments/assets/e8313bb9-00c8-470f-b94e-f1fbb8883850)

<div align="center">
  <img src="https://img.shields.io/badge/Arduino-00979D?style=for-the-badge&logo=Arduino&logoColor=white" />
  <img src="https://img.shields.io/badge/Unity-100000?style=for-the-badge&logo=unity&logoColor=white" />
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" />
  <img src="https://img.shields.io/badge/C++-00599C?style=for-the-badge&logo=c%2B%2B&logoColor=white" />
</div>
<div align="center">
  <h3>🎮 Arduino 기반 DIY 게임 컨트롤러와 Unity 3D 레이싱 게임 연동 프로젝트</h3>
  <p>조선대학교 컴퓨터공학과 IoT 시스템 기초 최종 프로젝트 - Team Unite</p>
</div>

## 📖 프로젝트 개요

본 프로젝트는 **Arduino Uno 마이크로컨트롤러**를 기반으로 한 DIY 게임 컨트롤러를 제작하고, **Unity 엔진**과 실시간 시리얼 통신을 통해 몰입감 있는 레이싱 게임을 구현하는 IoT 프로젝트임.

게임 소프트웨어 개발을 넘어, 플레이를 위한 콘솔 기기를 직접 설계하고 제작하는 과정에서 사용자 편의성을 고려하는 자세를 배울 수 있었고, **센서 데이터 수집**, **실시간 통신**, **하드웨어-소프트웨어 융합**을 실습하는 과정을 통해 게임기를 조작하는 행동이 어떤 과정을 거쳐 실제 게임에 적용되는지 알 수 있었음.

### 🎯 주요 목표

- Arduino 기반 커스텀 게임 컨트롤러 제작 및 Unity 3D와의 실시간 연동
- USB 시리얼 통신을 통한 양방향 데이터 송수신 구현
- LED, 부저를 활용한 물리적 피드백 시스템 구축
- 조이스틱 드리프트 보정 및 데이터 노이즈 필터링 기술 적용
  

## 🏗️ 시스템 아키텍처

![Image](https://github.com/user-attachments/assets/f0269167-06ed-4e0b-9b54-18b47b3e0c6b)


## 🏗️ 통신 흐름도

![Image](https://github.com/user-attachments/assets/dd4029ab-262a-4a1f-8f79-e3cad015356d)


## 🏗️ 데이터 명세

![Image](https://github.com/user-attachments/assets/0fa03fae-0981-4b73-a036-e184f696ab6f)


## 🔧 하드웨어 구성

### 필요한 부품

- **Arduino Uno 보드** × 1
- **브레드보드** × 1
- **아날로그 조이스틱 모듈** × 1
- **푸시 버튼** × 2 (머신건/미사일 발사)
- **LED (빨강)** × 1
- **부저** × 1
- **220Ω 저항** × 3
- **점퍼선** 다수


### 회로 연결

![Image](https://github.com/user-attachments/assets/34c2c7ba-1694-4740-9ed2-d0578d061d5b)


![Image](https://github.com/user-attachments/assets/044a6c46-228f-4947-a350-0e8f8bae9e41)


## 💻 소프트웨어 구성

### 사용 에셋
- **Queen - Car Controller with Shooting Capabilities for Both Mobile and PC**


### Arduino 코드 주요 기능

- **조이스틱 자동 보정**: 시작 시 중앙값 자동 측정으로 드리프트 방지
- **데드존 처리**: 미세한 움직임으로 인한 노이즈 제거
- **실시간 데이터 전송**: CSV 형식으로 Unity에 센서 데이터 전송
- **피드백 제어**: Unity에서 받은 명령으로 LED/부저 제어


### Unity 게임 기능

- **실시간 차량 제어**: 조이스틱 입력으로 레이싱 카 조작
- **무기 시스템**: 머신건/미사일 발사 기능
- **적 AI**: 자동으로 움직이는 적 차량
- **아이템 시스템**: 탄약/가스 획득 기능
- **물리적 피드백**: 게임 상태에 따른 LED/부저 제어


## 📊 통신 프로토콜

### Arduino → Unity 데이터 형식

```
"xVal,yVal,btn1State,btn2State\n"
예시: "501,1023,0,1"
```


### Unity → Arduino 명령 형식

- `'L'`: LED 켜기 (고속 주행/활성 상태)
- `'l'`: LED 끄기 (정지/비활성 상태)
- `'B'`: 부저 울리기 (충돌/경고)
- `'b'`: 부저 끄기


## 🚀 설치 및 실행 방법

### 1. 하드웨어 설정

1. 회로 연결표에 따라 Arduino와 센서들을 브레드보드에 연결
2. Arduino를 USB로 PC에 연결

### 2. Arduino 코드 업로드

1. Arduino IDE 설치
2. `Arduino/` 폴더의 `.ino` 파일을 Arduino IDE로 열기
3. 보드와 포트 설정 후 업로드

### 3. Unity 프로젝트 실행

1. Unity Hub에서 프로젝트 열기
2. `Assets/Scripts/ArduinoController02.cs`에서 포트명 확인/수정
3. **중요**: Arduino IDE 시리얼 모니터 완전히 닫기 -> 시리얼 포트를 유니티에서 점유해야 함
4. Unity에서 Play 버튼 클릭

## 🎮 게임 플레이 방법

### 조작법

- **조이스틱**: 차량 조향 (좌/우/전진/후진)
- **버튼1**: 머신건 발사
- **버튼2**: 미사일 발사
- **LED**: 게임 상태 표시 (속도/활동 상태)
- **부저**: 게임 이벤트 사운드 (발사/충돌)


### 게임 목표

- 조이스틱으로 레이싱 카를 운전하며 장애물과 적 AI를 피해 목적지 도달
- 미사일/머신건으로 장애물과 적 차량 파괴
- 맵 곳곳의 탄약과 가스를 획득하여 게임 진행


## 📈 프로젝트 성과

### 달성된 기능 ✅

- Arduino DIY 컨트롤러 하드웨어 제작 (100%)
- 실시간 Unity 게임과의 완벽한 연동 (100%)
- 조이스틱 기반 차량 조작 시스템 (100%)
- 버튼 기반 무기 발사 시스템 (100%)
- LED/부저를 통한 실시간 피드백 (100%)


### 성능 지표

- **통신 지연시간**: 20ms 이하 (50Hz 업데이트)
- **데이터 정확도**: 99% 이상 (파싱 성공률)
- **시스템 안정성**: 장시간 연속 동작 테스트 통과
- **전체 완성도**: 95%


## 🔧 문제 해결 및 최적화

### 주요 해결 과제

1. **시리얼 포트 액세스 오류**: 포트 상태 확인 및 예외 처리 로직 구현
![Image](https://github.com/user-attachments/assets/26760ea3-761a-4e66-85fe-f6ea59fab352)
 
2. **조이스틱 드리프트**: 자동 보정 알고리즘 및 데드존 처리
![Image](https://github.com/user-attachments/assets/7a2660a8-8b76-44a6-9beb-e03bfe9f4c06)

4. **게임 오브젝트 떨림**: FixedUpdate 활용 및 부드러운 보간 적용

### 고급 기능

- **조이스틱 자동 보정 시스템**: 시작 시 중앙값 자동 측정
- **고급 데이터 필터링**: 데드존 처리, 노이즈 제거
- **부드러운 움직임 시스템**: Lerp 보간을 통한 자연스러운 조작감


## 🔮 향후 발전 방향

### 단기 계획

- **무선 통신 모듈 적용**: Bluetooth/WiFi를 통한 무선 연결
- **추가 센서 통합**: 가속도계, 자이로스코프, 압력센서
- **멀티플레이어 지원**: 다중 컨트롤러 동시 연결


### 장기 계획

- **교육용 키트 개발**: STEM 교육용 DIY IoT 게임 컨트롤러 패키지
- **재활 치료 응용**: 물리치료용 게임 인터페이스
- **스마트홈 연동**: IoT 기기 제어용 범용 컨트롤러


## 📄 라이선스

이 프로젝트는 교육 목적으로 제작되었으며, MIT 라이선스 하에 배포됩니다.
