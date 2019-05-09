//Connect TX pin of the HC-05 to RX pin of the Arduino
//Connect RX pin of the HC-05 to TX pin of the Arduino
//You can use SoftwareSerial Library, but i dont recommend it for fast and long data transmission
//Otherwise you have to check Serial.available() > excepted number of bytes sent before reading the message
//There's no problem with hardware serial that comes with the arduino. It's perfect
//

void sendBT(const byte *data, int l)
{
	byte len[4];
	len[0] = 85; //preamble
	len[1] = 85; //preamble
	len[2] = (l >> 8) & 0x000000FF;
	len[3] = (l & 0x000000FF);
	Serial.write(len, 4);
	Serial.flush();
	Serial.write(data, l);
	Serial.flush();
}

void setup()
{
	Serial.begin(9600);
}

char *data;
int data_length;
int i = 0;
void loop()
{
	if (Serial.available() > 2)
	{
		data_length = 0;
		char p1 = Serial.read();
		char p2 = Serial.read();
		if(p1 != 85 || p2 != 85) return;
		while(Serial.available() < 2) continue;
		char x1 = Serial.read();
		char x2 = Serial.read();
		data_length = x1 << 8 | x2;
		data = new char[data_length];
		i = 0;
		while (i < data_length)
		{
			if (Serial.available() == 0)
				continue;

			data[i++] = Serial.read();
		}

		sendBT(data, data_length);

		delete[] data;
	}

	delay(100);
	sendBT("HELLOO", 6);
}

//PS char or byte ranges are -127 to 128, it you want to use 0 255 range for your binary data, simply cast it as unisgned char
// example:
// byte x = 129;
// if(x == 129) will return false
// if((unsigned char) x == 129 ) will return true
