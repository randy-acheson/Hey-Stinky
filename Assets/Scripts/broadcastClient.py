
import socket
import threading

HOST_ADDR = "192.168.86.46"
TCP_PORT = 7777

def createServerConnections():
    s_tcp = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s_tcp.connect((HOST_ADDR, TCP_PORT))

    t = threading.Thread(target=spawnTCPListener, args=(s_tcp,), daemon=True)
    t.start()

    while True:
        user_input = input("Message to send: ")
        s_tcp.sendall(user_input.encode())


def spawnTCPListener(socket):
    while True:
        tcp_data = socket.recv(4096)
        print("TCP Received:", tcp_data)
        if not tcp_data:
            print("Connection Killed")
            break
    socket.close()
    exit()

createServerConnections()