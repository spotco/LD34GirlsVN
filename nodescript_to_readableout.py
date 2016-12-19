import sys
import os
import json

def filename_to_outstr(filename):
    jsobj = None
    try:
        jsobj = json.loads(open(filename,'r').read())
    except Exception as e:
        print "ERR file(%s)"%(filename)
        print(e)
        return

    rtv = ""
    for evt in jsobj["event"]:
        if not "text" in evt:
            continue
        if "character" in evt:
            rtv = rtv + "%s: %s" % (evt["character"],evt["text"])
        else:
            rtv = rtv + "%s" % (evt["text"])
        rtv = rtv + "\n"

    return rtv


out_folder = os.path.join(os.getcwd(),"node_scripts_preview")

os.chdir("Assets/Resources/nodescripts")
files = [f for f in os.listdir('.') if os.path.isfile(f)]

for itr_file in files:
    name,ext = os.path.splitext(itr_file)
    if ext != ".txt":
        continue
    if "TEST" in name:
        continue
    fulldir = os.path.join(os.getcwd(),itr_file)
    outstr = filename_to_outstr(fulldir)
    outfilename = os.path.join(out_folder,name + ".txt")

    f = open(outfilename,'w+')
    try:
        f.write(outstr.encode('utf-8'))
    except Exception as e:
        print "ERR file2(%s)"%(fulldir)
        print(outstr)
        print e
    f.close()
