
import socket
import threading

HOST_ADDR = "192.168.86.46"
TCP_PORT = 7777

subscribers = set()

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
        tcp_data = connection.recv(4096)
        print("TCP Received: ", tcp_data)
        for s in subscribers:
            s.sendall(tcp_data)
        if not tcp_data:
            print("Connection Killed")
            break
    subscribers.remove(connection)
    connection.close()
    exit()

serverStartup()