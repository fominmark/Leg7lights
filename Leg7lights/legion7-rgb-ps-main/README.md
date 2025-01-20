# Legion 7 RGB PowerShell Script
## *** Run at your own risk, I am not responsible for any problems that this script might cause ***

PowerShell script for controlling LED lights on Legion 7 Gen6.

## Installation
Clone the repo or download both ps1 files to a folder on your computer

## Usage
Make sure iCue software is not running.
Open PowerShell and navigate to folder where you saved the scripts.

Execute following  

    .\legion7-rgb.ps1 -all 070707

LEDs should be set to dim white color.

To contorol LEDs by groups, use `-keys`, `-logo`, `-vents`, or `-neon` 

### Command line

To run it from command line

    powershell -executionPolicy bypass -file ".\legion7-rgb.ps1" -vents 00FF00

### Windows shortcut

Create a shortcut and paste the below as command. Make sure to adjust `<path to script>` to where your script is saved 

    cmd.exe /c start /min "" powershell WindowStyle -Hidden -ExecutionPolicy Bypass -Command ". '<path to script>\legion7-rgb.ps1' -all 070707"

To add shortcut key, right click the shortcut and select properties. Modify Shortcut key.

## Support
This was tested on my Legion 7i Gen6, It is possible that the VID, PID, UP, and U values differ for your specific model. If you want to add functionality for your model, please open an issue in issue tracker and include the output from:

    Get-PnpDevice -Class 'HIDClass' | ForEach-Object { [PSCustomObject]@{Name = $_.FriendlyName; InstanceId = $_.InstanceId; HardwareId = ($_.HardwareId | Where-Object {$_ -like 'HID_DEVICE_UP:*' })}} | Where-Object { $_.HardwareId -ne $null } | Sort-Object InstanceId

## Roadmap
Add functionality for other Legion 7 models

## Contributing
Anybody is welcome to contribute

## Authors and acknowledgment
Inspired by https://github.com/arcinxe/legion7-rgb by @marcinxe

## License
Attributions appreciated but otherwise you may do what you will with the code. 

## Project status
Alpha
