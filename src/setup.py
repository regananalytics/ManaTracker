from cx_Freeze import setup, Executable
from pathlib import Path

from ManaTracker import __about__

name = __about__.__name__

# Path mapping
src_dir = Path(__file__).parent
pkg_dir = src_dir / name
root_dir = src_dir.parent
build_dir = root_dir / 'build' / 'ManaTracker'
cfg_dir = root_dir / 'cfg'

main_script = pkg_dir / '__main__.py'
launcher_script = pkg_dir / 'launcher.py'

# Dependencies
requirements = []
with open(src_dir / 'requirements.txt', 'r') as f:
    requirements += [line for line in f]


# Build
excludes = []

include_files = [
    (cfg_dir, 'cfg'), 
    (root_dir / 'LICENSE', ''),
]

packages = [
    __about__.__name__,
]

executables = [
    Executable(
        main_script,
        base=None,
        target_name=name + "UI.exe"
    ),
    Executable(
        launcher_script,
        base=None,
        target_name=name + ".exe"
    )
]


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
