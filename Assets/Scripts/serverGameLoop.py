import random
import socket
import time
import threading


class boop:
    lol = time.time()
main_alive = boop()

UDP_IP = "192.168.86.61"
UDP_PORT_SEND = 5005
UDP_PORT_RECIEVE = 5006

sock_receieve = socket.socket(socket.AF_INET, # Internet
                     socket.SOCK_DGRAM) # UDP


sock_receieve.bind((UDP_IP, UDP_PORT_RECIEVE))
sock_receieve.settimeout(1);


sock_send = socket.socket(socket.AF_INET, # Internet
                     socket.SOCK_DGRAM) # UDP


class SendStuff(threading.Thread):
    def __init__(self, main_alive):
        super(SendStuff, self).__init__()
        self.main_alive = main_alive

    def run(self):
        while True:
            if main_alive.lol < (time.time() - 5):
                print('inner thread exiting')
                exit()
            time.sleep(.2)
            the_string_thing = str(random.randint(1, 5))
            print('sent', the_string_thing)
            sock_send.sendto(str.encode(the_string_thing), (UDP_IP, UDP_PORT_SEND))


# x = SendStuff(main_alive)
# x.start()

addrs = {}
def update_addrs(new_addr):
    to_del = []
    for a, val in addrs.items():
        if (time.time() - 5) > val:
            to_del.append(a)
    
    for remover in to_del:
        del addrs[remover]

    if new_addr not in addrs:
        print('added {}'.format(new_addr))
    addrs[new_addr] = time.time()

while True:
    try:
        main_alive.lol = time.time()
        data, new_addr = sock_receieve.recvfrom(1024) # buffer size is 1024 bytes

        if data:
            # print("received message: {}, updating addrs, now sending back out to {}".format(data, addrs))
            update_addrs(new_addr[0])

            for addr in addrs:
                # if addr != new_addr:
                sock_send.sendto(data, (addr, UDP_PORT_SEND))
            # print("sent out")
        else:
            print('receieved nothing, trying again')
    except KeyboardInterrupt:
        print('should be killing')
        x.join()
    except:
        pass
