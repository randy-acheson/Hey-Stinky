import random
import socket
import time
import threading


class boop:
    lol = time.time()
main_alive = boop()

UDP_IP = "127.0.0.1"
UDP_PORT_SEND = 5005
UDP_PORT_RECIEVE = 5006

sock_receieve = socket.socket(socket.AF_INET, # Internet
                     socket.SOCK_DGRAM) # UDP


sock_receieve.bind((UDP_IP, UDP_PORT_RECIEVE))
sock_receieve.settimeout(1);


sock_send = socket.socket(socket.AF_INET, # Internet
                     socket.SOCK_DGRAM) # UDP


sock_send.bind((UDP_IP, UDP_PORT_SEND))
sock_send.settimeout(1);


class SendStuff(threading.Thread):
    def __init__(self, main_alive):
        super(SendStuff, self).__init__()
        self.main_alive = main_alive

    def run(self):
        while True:
            if main_alive.lol < (time.time() - 5):
                exit()
            time.sleep(.2)
            the_string_thing = str(random.randint(1, 5))
            print('sent', the_string_thing)
            sock_send.sendto(str.encode(the_string_thing), (UDP_IP, UDP_PORT_SEND))


x = SendStuff(main_alive)
x.start()


while True:
    try:
        main_alive.lol = time.time()
        data, addr = sock_receieve.recvfrom(1024) # buffer size is 1024 bytes
        print("received message: %s" % data)
    except KeyboardInterrupt:
        print('should be killing')
        x.join()
    except:
        pass
