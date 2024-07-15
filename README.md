This program is used to receive data via a serial port and to do something with it defined in an ini file

The idea was that a Barcode or QR-Code Scanner scans URLs. The program listens on that port and opens the default webbrowser with received data.

The command and the serial port can be configured in an ini-file.

Program starts minimized in systray and logs received data in textbox in gui.+

Uses .net 4.8. Compatibility begins with version 3.5 SP1

Codes is probably not that great but it's working. Have fun with it.

Possible Usecases:
1. Opening a Covid Test Certificate in the browser.
2. Handing over received data on a serial port to any desired program as start parameter.
3. Be creative. Do whatever you please.
