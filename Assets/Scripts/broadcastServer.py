import signal
import socket
import sys
import threading

TCP_PORT = 7777

subscribers = set()
mutex = threading.Lock()

def serverStartup():
    s_tcp = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s_tcp.bind(('', TCP_PORT))
    s_tcp.listen()

    while True:
        connection, client_addr = s_tcp.accept()
        mutex.acquire()
        subscribers.add(connection)
        mutex.release()
        t = threading.Thread(target=spawnTCPConnection, args=(connection,))
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

        mutex.acquire()
        for s in subscribers:
            s.sendall(tcp_data)
        mutex.release()

    mutex.acquire()
    subscribers.remove(connection)
    mutex.release()
    connection.close()
    sys.exit(0)

def signal_handler(sig, frame):
    sys.exit(0)

signal.signal(signal.SIGINT, signal_handler)
signal.signal(signal.SIGTERM, signal_handler)

serverStartup()

s_tcp.close()
