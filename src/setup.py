from setuptools import setup, find_packages

setup(
    name='ManaTracker',
    version='0.1.0',
    author='Dan Regan',
    author_email='dan@regan-analytics.com',
    description='Randomizer Tracker App built on C# and Python',
    url='https://gitlab.com/regananalytics/ManaTracker',
    license='MIT',
    packages=find_packages(),
    include_package_data=True,
    install_requires=[
        'dash',
        'pillow',
        "pyyaml",
        "pyzmq",
        "watchdog"
    ],
    entry_points={
        'console_scripts': [
            'ManaTracker = ManaTracker.app:main'
        ]
    }
)
