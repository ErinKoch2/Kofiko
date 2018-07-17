# sends UDP packets at regular intervals
# for orientations, and (soon) different positions for object grating
# code mostly copied from https://stackoverflow.com/questions/18743962/python-send-udp-packet

import socket
import time
import random


UDP_IP = "127.0.0.1"
UDP_PORT = 12345

print "UDP target IP:", UDP_IP
print "UDP target port:", UDP_PORT

i = 0
while True:
    i += 1
    time.sleep(1)
    message = str(i % 2 == 0) + "," + \
              str(random.random()) + "," + str(random.random()) + "," + \
              str(random.random()) + "," + str(random.random()) 

    sock = socket.socket(socket.AF_INET, # Internet
                 socket.SOCK_DGRAM) # UDP
    sock.sendto(message, (UDP_IP, UDP_PORT))

    print message
