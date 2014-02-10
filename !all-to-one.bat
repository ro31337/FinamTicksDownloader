@echo off
for %%a in (*.txt) do (
echo "Processing %%a"
type "%%a" >> "!data.txt"
)
