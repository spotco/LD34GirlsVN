import sys
import re

for line in open(sys.argv[1],'r').read().split("\n"):
    line = line.strip()
    if len(line) == 0:
        print("\n")
        continue
    name = None
    colon_index = line.find(":")
    if colon_index != -1:
        name = line[0:colon_index]
        line = line[colon_index+1:]
        line = line.strip()

    if name == None:
        print("{\"type\":\"dialogue\",\"text\":\"%s\",\"xpos\":0,\"ypos\":0},"%(line))

    else:
        character_name = "UNKNOWN"
        if name == "K":
            character_name = "Kurumi"

        elif name == "Y":
            character_name = "Yuuto"

        elif name == "N":
            character_name = "Naoko"

        elif name == "M":
            character_name = "Manami"

        elif name == "A1":
            character_name = "Agent 1"

        elif name == "A2":
            character_name = "Agent 2"

        else:
            raise("UNKNOWN CHARACTER(%s)"%(name))

        print("{\"type\":\"dialogue\",\"character\":\"%s\",\"text\":\"%s\"},"%(character_name,line))
