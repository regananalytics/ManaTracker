import os
import shutil
import zipfile
from cx_Freeze import setup, Executable
from setuptools import Command


target_name = "ManaTracker_RE1_Layout"
version = "0.0.2"


build_exe_options = {
    "packages": ["ManaTracker"],
    "excludes": [],
    "include_files": [
        ("ManaTracker/assets", "assets"),
        ("config.yaml", "config.yaml")
    ]
}

base = None

class BuildAndZip(Command):
    description = "Build the project and zip the output directory."
    user_options = []

    def initialize_options(self):
        pass

    def finalize_options(self):
        pass

    def run(self):
        # Run the original build command
        self.run_command('build_exe')
        
        # Zip the build directory
        build_dir = os.path.join(os.path.dirname(__file__), 'build', 'exe.win-amd64-3.x')  # Adjust the path as necessary
        zip_file = os.path.join(os.path.dirname(__file__), f'{target_name}_{version.replace(".","_")}.zip')
        
        with zipfile.ZipFile(zip_file, 'w', zipfile.ZIP_DEFLATED) as zf:
            for root, _, files in os.walk(build_dir):
                for file in files:
                    file_path = os.path.join(root, file)
                    arcname = os.path.relpath(file_path, build_dir)
                    zf.write(file_path, arcname)
        
        print(f"Build directory '{build_dir}' has been zipped to '{zip_file}'.")



setup(
    name = "ManaTracker",
    version = version,
    description = "Randomizer Item Tracker Layout Demo for RE1 HD Remake",
    options = {"build_exe": build_exe_options},
    executables = [Executable(
        "ManaTracker/__main__.py", 
        base=base,
        target_name=target_name
    )],
    cmdclass={
        'build_and_zip': BuildAndZip
    }
)

