import serial
import socket

# Arduinoのシリアル設定
SERIAL_PORT = '/dev/cu.usbserial-0001'  # Macの場合
BAUD_RATE = 9600

# UDP送信先設定
UDP_IP = '127.0.0.1'
UDP_PORT = 5005

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Arduino接続
ser = serial.Serial(SERIAL_PORT, BAUD_RATE, timeout=0.1)

def send_udp(message):
    sock.sendto(message.encode('utf-8'), (UDP_IP, UDP_PORT))
    print(f"Sent UDP: {message}")  # ターミナル表示

while True:
    try:
        line = ser.readline().decode('utf-8').strip()
        if not line:
            continue

        # Arduinoからのメッセージ処理
        if line.startswith("Angle:"):
            angle_value = line.split(":")[1].strip()
            send_udp(f"Angle:{angle_value}")  # 常に角度送信
        elif line == "Rr":
            # 右回転検知
            send_udp("Angle:0.0")  # 角度0も送信
            send_udp("Rr")
        elif line == "Lr":
            # 左回転検知
            send_udp("Angle:0.0")  # 角度0も送信
            send_udp("Lr")

    except Exception as e:
        print("Error:", e)
