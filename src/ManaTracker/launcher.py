import atexit
from pathlib import Path
import subprocess
import time

process1 = None
process2 = None

def start_processes():
    global process1, process2
    process1 = subprocess.Popen(["ManaTrackerUI.exe"])
    process2 = subprocess.Popen(["../ManaServer/ManaServer.exe"])

def cleanup():
    if process1 and process1.poll() is None:
        process1.terminate()
    if process2 and process2.poll() is None:
        process2.terminate()
    print("Processes Terminated.")

atexit.register(cleanup)

if __name__ == "__main__":
    start_processes()
    try:
        print("Press Ctrl+C to exit")
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        pass
