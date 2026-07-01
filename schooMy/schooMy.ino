　// KY-040 Rotary Encoder
const int pinCLK = 11;
const int pinDT  = 13;
const int pinSW  = 12;

int lastCLK = LOW;
int pulseCount = 0;
const int pulsesPerRevolution = 40; // 9° x 40 = 360°

void setup() {
  Serial.begin(9600);
  pinMode(pinCLK, INPUT);
  pinMode(pinDT, INPUT);
  pinMode(pinSW, INPUT_PULLUP);

  lastCLK = digitalRead(pinCLK);
}

void loop() {
  // SWで角度リセット
  if (digitalRead(pinSW) == LOW) {
    pulseCount = 0;
    Serial.println("Angle: 0.0");
    delay(300);
  }

  int currentCLK = digitalRead(pinCLK);
  if (currentCLK != lastCLK) {
    int direction = (digitalRead(pinDT) != currentCLK) ? 1 : -1;
    pulseCount += direction;

    // 角度計算
    float angle = fmod((pulseCount * 9.0) + 360.0, 360.0);
    Serial.print("Angle: ");
    Serial.println(angle);

    // 回転判定
    if (pulseCount >= pulsesPerRevolution) {
      Serial.println("Rr");
      pulseCount = 0;
    } else if (pulseCount <= -pulsesPerRevolution) {
      Serial.println("Lr");
      pulseCount = 0;
    }

    lastCLK = currentCLK;
  }
}