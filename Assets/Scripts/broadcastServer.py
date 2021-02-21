import signal
import socket
import sys
import threading

HOST_ADDR = "192.168.86.61"
TCP_PORT = 7777

subscribers = set()
mutex = threading.Lock()

def serverStartup():
    s_tcp = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s_tcp.bind((HOST_ADDR, TCP_PORT))
    s_tcp.listen()

    while True:
        connection, client_addr = s_tcp.accept()
        subscribers.add(connection)
        t = threading.Thread(target=spawnTCPConnection, args=(connection,), daemon=True)
        t.start()

def spawnTCPConnection(connection):
    while True:
        try:
            tcp_data = connection.recv(4096)
        except:
            break

        print("TCP Received: ", tcp_data)
        if (not tcp_data or tcp_data==b'exit'):
            print("Connection Killed")
            break

        mutex.acquire();
        for s in subscribers:
            s.sendall(tcp_data)
        mutex.release();

    subscribers.remove(connection)
    connection.close()
    sys.exit(0)

def signal_handler(sig, frame):
    sys.exit(0)

signal.signal(signal.SIGINT, signal_handler)
signal.signal(signal.SIGTERM, signal_handler)

serverStartup()