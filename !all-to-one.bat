@echo off
for %%a in (*.txt) do (
type "%%a" >> "!data.txt"
)
