from cx_Freeze import setup, Executable
from pathlib import Path
# from setuptools import Command
# import zipfile

from ManaTracker import __about__

name = __about__.__name__
exe_name = name + '.exe'

# Path mapping
_src_dir = Path(__file__).parent
_pkg_dir = _src_dir / name
_root_dir = _src_dir.parent
_build_dir = _root_dir / 'build'
_cfg_dir = _root_dir / 'cfg'

main_script = _pkg_dir / '__main__.py'
build_dir = _build_dir # ManaTracker/build

excludes = []

include_files = [
    (_cfg_dir, 'cfg'), 
]

packages = [
    __about__.__name__,
]

executables = [
    Executable(
        main_script,
        base=None,
        target_name=exe_name
    )
]

requirements = []
with open(_src_dir / 'requirements.txt', 'r') as f:
    requirements += [line for line in f]

setup(
    name=name,
    version=__about__.__version__,
    description=__about__.__description__,
    author=__about__.__author__,
    author_email=__about__.__author_email__,
    url=__about__.__url__,
    license=__about__.__license__,
    options={
        'build_exe': {
            'packages': packages,
            'excludes': excludes,
            'include_files': include_files,
            'build_exe': build_dir
        },
    },
    executables=executables,
    install_requires=requirements,
)
