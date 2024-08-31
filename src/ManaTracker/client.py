import asyncio
from typing import Callable
import zmq
from zmq.asyncio import Context, Socket

from ManaTracker.config import CONFIG
from ManaTracker.state import State


if asyncio.get_event_loop_policy().__class__.__name__ == 'WindowsProactorEventLoopPolicy':
    asyncio.set_event_loop_policy(asyncio.WindowsSelectorEventLoopPolicy())


class Client:

    def __init__(self, callback: Callable, address: str = None, verbose: bool = False):
        self.address: str = address or "tcp://localhost:5556"
        self.verbose: bool = verbose

        self.ctx: Context = Context.instance()
        self.sub: Socket = self.ctx.socket(zmq.SUB)

        self.callback: Callable = callback

    async def __aenter__(self):
        self._connect()
        return self
    
    async def __aexit__(self, exc_type, exc, tb):
        self._disconnect()

    async def listen(self):
        print('Connected to publisher. Listening for mesages...')
        try:
            while True:
                message = await self.sub.recv_json()
                if self.verbose:
                    print(f'Received message: {message}')
                if self.callback is not None:
                    await self.callback(message)
        except asyncio.CancelledError:
            print("Listener cancelled.")

    def _connect(self):
        self.sub.connect(self.address)
        self.sub.setsockopt(zmq.SUBSCRIBE, b"")
        print(f'Connected to {self.address}')

    def _disconnect(self):
        self.sub.close()
        self.ctx.term()


async def start(mem_client: Client):
    async with mem_client as client:
        await client.listen()


def run_client(state: State):
    client = Client(state.callback, verbose=False)
    asyncio.run(start(client))
    

if __name__ == "__main__":
    run_client(State())
