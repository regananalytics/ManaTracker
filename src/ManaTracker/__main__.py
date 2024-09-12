import argparse
import atexit
from pathlib import Path
import subprocess
import traceback

from ManaTracker.app import ManaTracker

server_process = None

default_config = (Path(__file__) / "../../../cfg").resolve()

def start_mana_server(args):
    global server_process
    exe_path = Path(__file__).parent / "ManaServer" / "ManaServer.exe"
    print(f'Starting ManaServer')
    cmd = [str(exe_path.resolve()), args.game, "-c", str(Path(args.cfg).resolve())]
    if args.verbose:
        cmd += ["--verbose"]
        print(' '.join(cmd))
    server_process = subprocess.Popen(cmd)

def cleanup():
    if server_process and server_process.poll() is None:
        server_process.terminate()
    print("ManaServer Terminated.")

atexit.register(cleanup)


def parse_args():
    parser = argparse.ArgumentParser(description="ManaTracker command-line options")
    
    # Required game argument
    parser.add_argument("--game", default="re1", type=str, help="The name of the game config to use")

    # Options
    parser.add_argument('-c', '--cfg', '--config', default=default_config, type=str, help='Path to the tracker config file')
    parser.add_argument('-H', '--host', default='localhost', type=str, help='The host address')
    parser.add_argument('-P', '--port', default='8080', type=int, help='The port number')
    parser.add_argument('-v', '--verbose', action='store_true', help='Verbose output')
    parser.add_argument('--debug', action='store_true', help="Enable debug mode")

    return parser.parse_args()


if __name__ == '__main__':
    try:
        args = parse_args()
        app = ManaTracker(args)
        start_mana_server(args)
        app.run()
    except Exception as e:
        print("An error occurred:")
        traceback.print_exc()  # Print the error stack trace
        input("Press Enter to exit...")  # Wait for user input before closing
