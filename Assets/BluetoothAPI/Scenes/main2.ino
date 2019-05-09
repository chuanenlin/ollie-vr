//Connect TX pin of the HC-05 to RX pin of the Arduino
//Connect RX pin of the HC-05 to TX pin of the Arduino
//You can use SoftwareSerial Library, but i dont recommend it for fast and long data transmission
//Otherwise you have to check Serial.available() > excepted number of bytes sent before reading the message
//There's no problem with hardware serial that comes with the arduino. It's perfect
//

void sendBT(const byte * data, int l)
{
  byte len[4];
  len[0] = 85;//preamble
  len[1] = 85;//preamble
  len[2] = (l >> 8) & 0x000000FF;
  len[3] = (l & 0x000000FF);
  Serial.write(len, 4);
  Serial.flush();
  Serial.write(data, l);
  Serial.flush();
}
void setup() {
  pinMode(2, OUTPUT);
  pinMode(3, OUTPUT);
  pinMode(4, OUTPUT);
  pinMode(5, OUTPUT);
  pinMode(6, OUTPUT);
  pinMode(7, OUTPUT);
  pinMode(8, OUTPUT);
  pinMode(9, OUTPUT);
  pinMode(10, OUTPUT);
  pinMode(11, OUTPUT);
  pinMode(12, OUTPUT);
  pinMode(13, OUTPUT);
  
  Serial.begin(9600);
  
    digitalWrite(2, HIGH); 
    delay(1000);
    digitalWrite(2, LOW); 
  }

char* data;
int data_length;
int i=0;
int timeout;
void loop() {
  readBT();
}


void readBT()
{
  if(Serial.available() >= 2)
  {
    timeout=0;
    data_length = 0;
    byte pre1 = Serial.read();
    byte pre2 = Serial.read();
    if(pre1 != 85 || pre2 != 85) return;
    while(Serial.available() < 2) continue;
    byte x1 = Serial.read();
    byte x2 = Serial.read();

    data_length = x1 << 8 | x2;
    
    data = new byte[data_length];
    i=0;
    while(i<data_length)
    {
        
      if(Serial.available()==0){
        if(++timeout == 1000)
          goto FreeData;
        continue;
      }
      timeout=0;
      data[i++] = Serial.read();
    }

    byte* send_data = new byte[data_length+1];
    send_data[0] = 'S';
    for(byte i=0; i<2;i++)
      send_data[i+1] = data[i];

    int x = data[0] == 'E' ? HIGH : LOW;

    digitalWrite(data[1], x);

    sendBT(send_data, data_length+1);
    delete [] send_data;

    FreeData:
    delete[] data;
  }
}