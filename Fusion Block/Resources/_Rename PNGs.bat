@echo off
if exist *.mtl (
  echo Removing .mtl files
  del *.mtl >nul
)
if exist *._png (
  ren *._png *.png
  echo Set .png files for edit
  timeout 2 >nul
  exit 0
)
ren *.png *._png
echo Set ._png files for compile
timeout 2 >nul
exit 0