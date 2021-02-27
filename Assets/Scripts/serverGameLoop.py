import random
import socket
import time
import threading


UDP_PORT = 5006

socket_server = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
socket_server.bind(('', UDP_PORT))
socket_server.settimeout(1)

addrs = {}


def update_addrs(new_addr):
    to_del = []
    for a, val in addrs.items():
        if (time.time() - 5) > val:
            to_del.append(a)

    for remover in to_del:
        print('removed {}'.format(remover))
        del addrs[remover]

    if new_addr not in addrs:
        print('added {}'.format(new_addr))
    addrs[new_addr] = time.time()


while True:
    try:
        data, new_addr = socket_server.recvfrom(1024) # buffer size is 1024 bytes

        if data:
            update_addrs(new_addr)
            for addr in addrs:
                if addr != new_addr :
                    socket_server.sendto(data, addr)
        else:
            print('receieved nothing, trying again')
    except KeyboardInterrupt:
        socket_server.close()
        break
    except:
        pass
