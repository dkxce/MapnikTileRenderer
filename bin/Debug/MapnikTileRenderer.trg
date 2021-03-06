Windows Registry Editor Version 5.00


[HKEY_CLASSES_ROOT\.mtr_project]
@="mtr_projectfile"

[HKEY_CLASSES_ROOT\mtr_projectfile]
@="Mapnik Tile Renderer Project"

[HKEY_CLASSES_ROOT\mtr_projectfile\DefaultIcon]
@="{APPDIR}\\icons\\mtr_project.ico,0"

[HKEY_CLASSES_ROOT\mtr_projectfile\shell]

[HKEY_CLASSES_ROOT\mtr_projectfile\shell\open]
@="Open project in Desktop App"

[HKEY_CLASSES_ROOT\mtr_projectfile\shell\open\command]
@="{APPDIR}\\MapnikTileRenderer.exe \"%1\""

[HKEY_CLASSES_ROOT\mtr_projectfile\shell\standalone]
@="Start rendering standalone"

[HKEY_CLASSES_ROOT\mtr_projectfile\shell\standalone\command]
@="{APPDIR}\\mtrc.exe \"%1\" -wait"

[HKEY_CLASSES_ROOT\mtr_projectfile\shell\server]
@="Start rendering server"

[HKEY_CLASSES_ROOT\mtr_projectfile\shell\server\command]
@="{APPDIR}\\mtrc.exe \"%1\" /server 9666 -cc -dump"

[HKEY_CLASSES_ROOT\mtr_projectfile\shell\client]
@="Start rendering client"

[HKEY_CLASSES_ROOT\mtr_projectfile\shell\client\command]
@="{APPDIR}\\mtrc.exe \"%1\" /client 127.0.0.1:9666"

[HKEY_CLASSES_ROOT\.errorlog]
@="mtr_errorlog"

[HKEY_CLASSES_ROOT\mtr_errorlog]
@="Mapnik Tile Renderer Error Log"

[HKEY_CLASSES_ROOT\mtr_errorlog\DefaultIcon]
@="{APPDIR}\\icons\\errorlog.ico,0"

[HKEY_CLASSES_ROOT\mtr_errorlog\shell]

[HKEY_CLASSES_ROOT\mtr_errorlog\shell\open]
@="Open in AkelPad"

[HKEY_CLASSES_ROOT\mtr_errorlog\shell\open\command]
@="{APPDIR}\\plugins\\AkelPad.exe \"%1\""

[HKEY_CLASSES_ROOT\mtr_errorlog\shell\notepad]
@="Open in Notepad++"

[HKEY_CLASSES_ROOT\mtr_errorlog\shell\notepad\command]
@="C:\\Program Files\\Notepad++\\notepad++.exe \"%1\""

[HKEY_CLASSES_ROOT\.holes]
@="mtr_holes"

[HKEY_CLASSES_ROOT\mtr_holes]
@="Mapnik Tile Renderer Holes File"

[HKEY_CLASSES_ROOT\mtr_holes\DefaultIcon]
@="{APPDIR}\\icons\\holes.ico,0"

[HKEY_CLASSES_ROOT\mtr_holes\shell]


[HKEY_CLASSES_ROOT\mtr_holes\shell\open]
@="Open in AkelPad"

[HKEY_CLASSES_ROOT\mtr_holes\shell\open\command]
@="{APPDIR}\\plugins\\AkelPad.exe \"%1\""

[HKEY_CLASSES_ROOT\mtr_holes\shell\notepad]
@="Open in Notepad++"

[HKEY_CLASSES_ROOT\mtr_holes\shell\notepad\command]
@="C:\\Program Files\\Notepad++\\notepad++.exe \"%1\""

[HKEY_CLASSES_ROOT\mtr_dump]
@="Mapnik Tile Renderer Dump File"

[HKEY_CLASSES_ROOT\mtr_dump\DefaultIcon]
@="E:\\__MAPNIK\\app\\icons\\dump.ico,0"

[HKEY_CLASSES_ROOT\mtr_dump\shell]

[HKEY_CLASSES_ROOT\mtr_dump\shell\open]
@="Open in AkelPad"

[HKEY_CLASSES_ROOT\mtr_dump\shell\open\command]
@="{APPDIR}\\app\\plugins\\AkelPad.exe \"%1\""

[HKEY_CLASSES_ROOT\mtr_dump\shell\notepad]
@="Open in Notepad++"

[HKEY_CLASSES_ROOT\mtr_dump\shell\notepad\command]
@="C:\\Program Files\\Notepad++\\notepad++.exe \"%1\""

[HKEY_CLASSES_ROOT\mtr_dump\shell\openwh]
@="Open in WinHex"

[HKEY_CLASSES_ROOT\mtr_dump\shell\openwh\command]
@="C:\\Program Files\\WinHex\\WinHex.exe \"%1\""
