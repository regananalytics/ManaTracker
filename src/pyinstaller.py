import PyInstaller.__main__

PyInstaller.__main__.run([
    "src/ManaTracker/__main__.py",
    "-y", "--clean",
    "--name=ManaTracker",
    "--distpath=build/",
    "--workpath=pybuild/",
    "--add-data=build/ManaServer/:ManaServer"
])