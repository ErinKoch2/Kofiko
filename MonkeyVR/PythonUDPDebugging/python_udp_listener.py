import socket
import random


UDP_IP = "127.0.0.1"
UDP_PORT = 12345
UDP_SEND = 1111

print "reading IP address: " + UDP_IP + " port: " + str(UDP_PORT)

sock = socket.socket(socket.AF_INET, # Internet
                     socket.SOCK_DGRAM) # UDP
sock.bind((UDP_IP, UDP_PORT))

while True:
    data, addr = sock.recvfrom(1024) # buffer size is 1024 bytes
    if data == "getAnalog":
        message = \
              str(random.random()) + "," + str(random.random()) + "," + \
              str(random.random()) + "," + str(random.random()) 
        sock.sendto(message, (UDP_IP, UDP_PORT))
        print "getAnalog", message
    #else:
    #    print "received message:", data
        
