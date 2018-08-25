void setup() {
  // put your setup code here, to run once:
  // COM5 according to everything ..
  analogWriteResolution(12); // that resolution!!
  Serial.begin(9600);
}

void loop() {
  // put your main code here, to run repeatedly:
  if (Serial.available() > 0) {
    // read the incoming byte:
    
    String s = Serial.readStringUntil(';');
    if (getValue(s, ',', 1).length() < 1) 
      return;
    // say what you got:
    // Serial.print("I received: ");
    // Serial.println(incomingByte, DEC);
    // Serial.println(s);

    float X = getValue(s, ',', 0).toFloat();
    float Y = getValue(s, ',', 1).toFloat();
    // convert X signal from [-180, 180] to [0, 4095]
    int X_converted = (int) ((4096.0 / 360.0)*(X+180.0));
    if (X_converted < 0)
      X_converted = 0;
    if (X_converted > 4095)
      X_converted = 4095;
    analogWrite(DAC0, X_converted);
    // convert Y signal from [-90, 90] to [0, 4095]
    int Y_converted = (int) ((4096.0 / 180.0)*(Y+90.0));
    if (Y_converted < 0)
      Y_converted = 0;
    if (Y_converted > 4095)
      Y_converted = 4095;
    analogWrite(DAC1, Y_converted);
    //Serial.println((int) ((4096.0 / 360.0)*(X+180.0)), DEC);
    //Serial.println((int) ((4096.0 / 180.0)*(Y+90.0)), DEC);
  }
}

String getValue(String data, char separator, int index) {
  // copied from https://arduino.stackexchange.com/a/1237
  // splits string into two halves
    int found = 0;
    int strIndex[] = { 0, -1 };
    int maxIndex = data.length() - 1;

    for (int i = 0; i <= maxIndex && found <= index; i++) {
        if (data.charAt(i) == separator || i == maxIndex) {
            found++;
            strIndex[0] = strIndex[1] + 1;
            strIndex[1] = (i == maxIndex) ? i+1 : i;
        }
    }
    return found > index ? data.substring(strIndex[0], strIndex[1]) : "";
}

