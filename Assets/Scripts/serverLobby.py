import threading



class HandleClient(threading.Thread):
    def __init__(self, conn, addr):
        super(HandleClient, self).__init__()
        self.conn = conn
        self.addr = addr

    def run(self):
        with self.conn:
            print('Connected by', self.addr)
            while True:
                data = self.conn.recv(1024)
                print('recieved', data)
                if not data:
                    break
                my_str = 'im the server bitch'
                print('sending', my_str)
                self.conn.sendall(str.encode(my_str))



import socket

HOST = '127.0.0.1'  # Standard loopback interface address (localhost)
PORT = 5004        # Port to listen on (non-privileged ports are > 1023)

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.bind((HOST, PORT))
    s.listen()
    s.settimeout(10)


    while True:
        print('waiting on new connection...')
        conn, addr = s.accept()
        print('got!')
        x = HandleClient(conn, addr)
        x.start()
