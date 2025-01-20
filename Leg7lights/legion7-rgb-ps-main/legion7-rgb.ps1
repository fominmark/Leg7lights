<#
.SYNOPSIS
  Script for controlling LED lights on Lenovo Legion 7
.DESCRIPTION
  Make sure that iCue is not running. No need to kill it, just exit the application by right clicking
  the icon in the tray and selecting Quit.
  Make sure that led-settings.ps1 is in your path.
  Run script
  legion7-rgb.ps1 [-all <hex color code>] [-logo <hex color code>] [-keys <hex color code>] [-vents <hex color code>] [-neon <hex color code>]
.PARAMETER <Parameter_Name>
  -keys <ffffff> (optional) sets color of all keyboard LEDs to ffffff
  -logo <ffffff> (optional) sets color of Legion logo LEDs to ffffff
  -vents <ffffff> (optional) sets color of vent LEDs to ffffff
  -neon <ffffff> (optional) sets color of neon (front and side rim) LEDs to ffffff
  -stop (optional) attempt to gracefully stop iCUE software
  -kill (optional) kill iCUE process tree
  -test enumerate all LED IDs for a devices identified by VID, PID, Usage Page and Usage
        Change settings for test using following parameters:
        -v <0xFFFF>   - VID
        -p <0xFFFF>   - PID
        -up <0xFFFF>  - Usage Page
        -u <0xFF>     - Usage
        -r <0xFF>     - Report ID
        -c <0xFF>     - Classification Code
        -start <0xFF> - LED ID at which to start enumeration
        -stop <0xFF>  - LED ID at which to stop enumeration
.INPUTS
  None
.OUTPUTS
  None
.NOTES
  Version:        1.1
  Author:         Peter Vazny
  Creation Date:  12/31/2021
  Purpose/Change: Initial script development

.EXAMPLE
  # Set keyboard to ff0000, logo to ffffff, vents to 00ff00 and turn off neon LEDs
  legion7-rgb.ps1 -keys ff0000 -logo ffffff -vents 00ff00 -neon 000000
  # turn off all LEDs
  legion7-rgb.ps1 -all 000000
  # enumerate all LED IDs for HID\VID_048D&PID_C935 with HID_DEVICE_UP:FF89_U:0007
  # use default report ID 0x7 and default classification code 0xa1.
  legion7-rgb.ps1 -test -v 0x048D -p 0xC935 -up 0xFF89 -u 0x07
#>



param(
    [Parameter(Mandatory = $false, HelpMessage = "Enter a valid color in hex, for example F0F0F0")]
    [ValidatePattern("^[A-Fa-f0-9]{6}$")] [string] $keys,
    [Parameter(Mandatory = $false, HelpMessage = "Enter a valid color in hex, for example F0F0F0")]
    [ValidatePattern("^[A-Fa-f0-9]{6}$")] [string] $logo,
    [Parameter(Mandatory = $false, HelpMessage = "Enter a valid color in hex, for example F0F0F0")]
    [ValidatePattern("^[A-Fa-f0-9]{6}$")] [string] $vents,
    [Parameter(Mandatory = $false, HelpMessage = "Enter a valid color in hex, for example F0F0F0")]
    [ValidatePattern("^[A-Fa-f0-9]{6}$")] [string] $neon,
    [Parameter(Mandatory = $false, HelpMessage = "Enter a valid color in hex, for example F0F0F0")]
    [ValidatePattern("^[A-Fa-f0-9]{6}$")] [string] $all,
    [Parameter(Mandatory = $false)] [switch] $end=$false,
    [Parameter(Mandatory = $false)] [switch] $kill=$false,

    # parameters for testing LEDs. Used for key mapping and LED id enumeration
    [Parameter(Mandatory = $false, HelpMessage = "Enumerates LED IDs for device identified by VID,PID,Usage Page and Usage")]
    [switch] $test=$false,
    [Parameter(Mandatory = $false, HelpMessage = "VID (Default 0x048D)")]
    [uint16] $v = 0x048D,
    [Parameter(Mandatory = $false, HelpMessage = "PID (Default 0xC968)")]
    [uint16] $p = 0xC968,
    [Parameter(Mandatory = $false, HelpMessage = "Usage Page (Default 0xFF89)")]
    [uint16] $up = 0xFF89,
    [Parameter(Mandatory = $false, HelpMessage = "Usage (Default 0x07)")]
    [byte] $u = 0x07,
    [Parameter(Mandatory = $false, HelpMessage = "Request ID (Default 0x07)")]
    [byte] $r = 0x07,
    [Parameter(Mandatory = $false, HelpMessage = "Classification Code (Default 0xa1)")]
    [byte] $c = 0xa1,
    [Parameter(Mandatory = $false, HelpMessage = "Start at LED ID (Default 0x01)")]
    [byte] $start = 0x01,
    [Parameter(Mandatory = $false, HelpMessage = "Stop at LED ID (Default 0xaa)")]
    [byte] $stop = 0xaa
)


#---------------------------------------------------------[Initialisations]--------------------------------------------------------

. ("$($PSScriptRoot)\led-settings.ps1")

Add-Type -AssemblyName System.Runtime.WindowsRuntime

[void][Windows.Foundation.IAsyncOperation`1,    Windows.Foundation,    ContentType=WindowsRuntime]
[void][Windows.Devices.Enumeration.DeviceInformation,Windows.Devices.Enumeration,ContentType = WindowsRuntime]
[void][Windows.Devices.HumanInterfaceDevice.HidDevice, Windows.Devices.HumanInterfaceDevice, ContentType = WindowsRuntime]
[void][Windows.Devices.Enumeration.DeviceInformationCollection, Windows.Devices.Enumeration, ContentType = WindowsRuntime]

$_taskMethods = [System.WindowsRuntimeSystemExtensions].GetMethods() | ? {
    $_.Name -eq 'AsTask' -and $_.GetParameters().Count -eq 1
}

$asTaskGeneric = ($_taskMethods | ? { $_.GetParameters()[0].ParameterType.Name -eq 'IAsyncOperation`1' })[0];

#-----------------------------------------------------------[Functions]------------------------------------------------------------

Function Await($WinRtTask, $ResultType) {
    $asTask = $asTaskGeneric.MakeGenericMethod($ResultType)
    $netTask = $asTask.Invoke($null, @($WinRtTask))
    $netTask.Wait(-1) | Out-Null
    $netTask.Result
}

Function ParseHexColor([string] $hexColor){
    $hexBytes = [byte[]] -split ($hexColor -replace '..', '0x$& ')
    return $hexBytes
}

Function GetDevice([uint16] $vendorId, [uint16] $productId, [uint16] $usagePage, [uint16] $usage){
    $selector = [Windows.Devices.HumanInterfaceDevice.HidDevice]::GetDeviceSelector($usagePage, $usage, $vendorId, $productId)
    $devices = Await ([Windows.Devices.Enumeration.DeviceInformation]::FindAllAsync($selector)) ([Windows.Devices.Enumeration.DeviceInformationCollection])

    if($devices){
        $device = Await ([Windows.Devices.HumanInterfaceDevice.HidDevice]::FromIdAsync($devices[0].Id, [Windows.Storage.FileAccessMode]::Read)) ([Windows.Devices.HumanInterfaceDevice.HidDevice])
        return $device
    }
}

Function SendFeatureReport([Windows.Devices.HumanInterfaceDevice.HidDevice] $device, [byte] $reportId, [byte] $instruction, [byte[]] $data, [uint16] $maxPacketSize = 192) {

    $chunkSize = $maxPacketSize - 4

    # Break into chunks
    $counter = [pscustomobject] @{ Value = 0 }
    $chunks = $data | Group-Object -Propert { [math]::Floor($counter.Value++ / $chunkSize)}

    foreach ($chunk in $chunks) {
        [byte]$numberOfGroups = [math]::Ceiling($chunk.Count / 4)

        [byte[]] $bytes = @( $reportId, $instruction, $numberOfGroups, 0x00 ) + $chunk.Group

        #pad chunk to max size with 0s
        if($bytes.Length -lt $maxPacketSize) {$bytes += (1..($maxPacketSize - $bytes.Length) | ForEach-Object {[byte]0})}

        $featureReport = $device.CreateFeatureReport()
        [Windows.Storage.Streams.IBuffer]$buffer = [System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions]::AsBuffer($bytes)
        $featureReport.Data = $buffer
        $ret = Await ($device.SendFeatureReportAsync($featureReport)) ([uint32])
    }

}

Function GenerateRgbData([byte[]] $ledIds, [byte[]] $rgb){
    $data = 1..($ledIds.Length * 4) | ForEach-Object { [byte]0 }
    for($i=0;$i -lt $ledIds.Length;$i++){
        $j = $i * 4
        $data[$j] = $ledIds[$i]
        $data[++$j] = $rgb[0]
        $data[++$j] = $rgb[1]
        $data[++$j] = $rgb[2]
    }
    return $data
}

function Kill-Tree {
    Param([int]$ppid)
    Get-CimInstance Win32_Process | Where-Object { $_.ParentProcessId -eq $ppid } | ForEach-Object { Kill-Tree $_.ProcessId }
    Stop-Process -Id $ppid
}


function Enumerate {
    $device = GetDevice $v $p $up $u
    if($device) {
        #unlock controller for editing
        SendFeatureReport $device $r 0xb2 @(0x00)

        #turn off all LEDs
        $leds = GenerateRgbData @(0..255) @(0,0,0)
        SendFeatureReport $device $r $c $leds

        $changes = @{}
        $description=""
        #cycle throuh each LED id 0 through 255
        for ([Int32]$i=$start; $i -le $stop -and $description -ine "stop" ; $i++){
            #turn LED ON
            SendFeatureReport $device $r $c @($i, 0xFF, 0xFF, 0xFF)

            $description=""
            if ($c -eq 0xa1 -and $enISOKeyboardMap.ContainsKey($i)) {
                $existingDescription =  $enISOKeyboardMap[$i]
            } else {
                $existingDescription = "Undefined"
            }
            $description = Read-host "Enter description for LED id " $i.ToString("X2") " (type stop to end) [" $existingDescription "]"

            if ($description -ne "" -and $description -ine "stop") {
                $changes.Add($i.ToString("X2"),$description)
            }

            #turn LED OFF
            SendFeatureReport $device $r $c @($i, 0x00, 0x00, 0x00)
        }

        Write-Host "`n list of changes: `n"
        ConvertTo-Json $changes

    } else {
        Write-Host "HID Device not found. Below is a list of all HID devices that have usage page and usage defined."

        Get-PnpDevice -Class 'HIDClass' | ForEach-Object { [PSCustomObject]@{Name = $_.FriendlyName; InstanceId = $_.InstanceId; HardwareId = ($_.HardwareId | Where-Object {$_ -like 'HID_DEVICE_UP:*' })}} | Where-Object { $_.HardwareId -ne $null } | Sort-Object InstanceId | Format-Table Name, @{ Label = "VID"; Expression={[regex]::match($_.InstanceId , '^.*VID_([a-fA-F0-9]{4}).*$').Groups[1].Value}}, @{ Label = "PID"; Expression={[regex]::match($_.InstanceId , '^.*PID_([a-fA-F0-9]{4}).*$').Groups[1].Value}}, @{ Label = 'UP'; Expression={[regex]::match($_.HardwareId,'^.*_UP:([a-fA-F0-9]{4}).*$').Groups[1].Value} }, @{ Label = 'U'; Expression={[regex]::match($_.HardwareId ,'_U:([a-fA-F0-9]{4})').Groups[1].Value} }, InstanceId
    }
}

#-----------------------------------------------------------[Execution]------------------------------------------------------------

$iCue = Get-Process iCUE -ErrorAction SilentlyContinue
if($iCue) {
    if($end) {
        $iCue | Stop-Process
    } elseif ($kill) {
        Kill-Tree $iCue.Id
    } else {
        #iCUE running and neither kill or end selected
        Write-Host "iCUE is running. Please exit iCUE or use -end/-kill parameter to stop it."
        $host.SetShouldExit(2)
        return
    }

    $iCue | Wait-Process -Timeout 5 -ErrorAction SilentlyContinue
    if(!$iCue.HasExited){
        Write-Host "iCUE did not end in timely fasion. Exiting."
        $host.SetShouldExit(2)
        return
    }

    #give it a moment to flush commands
    if(!$kill) {Start-Sleep 1}
}

if($test) {Enumerate}

if($all) {
    if(!$keys) {$keys = $all}
    if(!$logo) {$logo = $all}
    if(!$vents) {$vents = $all}
    if(!$neon) {$neon = $all}
}

foreach ($legionDevice in $legion_devices) {
    $device = GetDevice $legionDevice.VendorId $legionDevice.ProductId $legionDevice.UsagePage $legionDevice.Usage
    if($device) {
        Write-Host "Found: $($legionDevice.Name)"
        #unlock controller for editing
        SendFeatureReport $device 0x07 0xb2 @(0x00)

        #change key colors
        if($keys){
            $ledGroup = $legionDevice.LedGroups["keys"]
            $ledData = GenerateRgbData $ledGroup.LedIds (parseHexColor $keys)
            SendFeatureReport $device 0x07 $ledGroup.Bank $ledData
        }

        #change key colors
        if($logo){
            $ledGroup = $legionDevice.LedGroups["logo"]
            $ledData = GenerateRgbData $ledGroup.LedIds (parseHexColor $logo)
            SendFeatureReport $device 0x07 $ledGroup.Bank $ledData
        }

        #change key colors
        if($vents){
            $ledGroup = $legionDevice.LedGroups["vents"]
            $ledData = GenerateRgbData $ledGroup.LedIds (parseHexColor $vents)
            SendFeatureReport $device 0x07 $ledGroup.Bank $ledData
        }

        #change key colors
        if($neon){
            $ledGroup = $legionDevice.LedGroups["neon"]
            $ledData = GenerateRgbData $ledGroup.LedIds (parseHexColor $neon)
            SendFeatureReport $device 0x07 $ledGroup.Bank $ledData
        }

    }
}


