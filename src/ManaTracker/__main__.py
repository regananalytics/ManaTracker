import argparse

from ManaTracker.app import ManaTracker

def parse_args():
    parser = argparse.ArgumentParser(description="ManaTracker command-line options")
    
    # Required game argument
    parser.add_argument("--game", default="re1", type=str, help="The name of the game config to use")

    # Options
    parser.add_argument('-c', '--cfg', '--config', type=str, help='Path to the tracker config file')
    parser.add_argument('-m', '--mem', '--memconf', type=str, help='Path to the memory config file')
    parser.add_argument('-H', '--host', default='localhost', type=str, help='The host address')
    parser.add_argument('-P', '--port', default='8080', type=int, help='The port number')
    parser.add_argument('--debug', action='store_true', help="Enable debug mode")

    return parser.parse_args()

if __name__ == '__main__':
    args = parse_args()
    app = ManaTracker(args)
    app.run()
