@echo off
for %%a in (*.txt) do (
echo "Processing %aa"
type "%%a" >> "!data.txt"
)
