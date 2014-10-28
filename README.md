YGOCore
===================
YGOCore is a duel server for the popular game YGOPro written in C#.

## Beta Details ##

The main goal of this project is to lower the entry level for new and experienced programmers. The server now runs on windows and linux systems when compiled with mono and supplyed with the native ocgcore.dll/so for that system. A sample client to work with the server can be found [here](https://github.com/Buttys/ygopro).

## Supported Features ##

* Single, Match and Tag dueling modes.
* Joining games in progress.
* Semi-automatic match making system.
* Duel setting customization. 

## How to use ##

To run the server you will require the card scripts, database and the banlist, check the following file/folder structure for a better understanding:

![alt text](https://raw.githubusercontent.com/SalvationDevelopment/YGOCore/master/filestructure.png "File Structure")

In order for users out side your LAN to connect to the server you will need to port forward the relevant port, in this case port `8911` is used by default.

The server is designed in such a way that when new card effects and rulings are added to the game just recompiling `ocgcore.dll` from [Fluorohydride/ygopro](https://github.com/Fluorohydride/ygopro) will continue keep the server compatible with all YGOPro clients. You can download a precompiled version of ocgcore from the [release section](https://github.com/Buttys/YGOCore/releases/) but this version might not be the latest version available, if possible always recompile. If you have handshuffling enabled you will need a `ocgcore.dll` from [Buttys/ygopro](https://github.com/Buttys/ygopro) after the relivant and latest changes have been merged down.

##Configuration##
`ygoserver.exe` takes two parameters, a port, and a `*.ini` format configuration file with these configurations. Without parameters YGOCore will look for the above file structure after tying to load config.txt. For example `c:\ygocore\ygoserver.exe 9101 alternative-configuration.ini`. Will start the server on port `9101` ignoring the configuration files `serverport` and load `alternative-configuration.ini` and not `config.txt`.

* `serverport` what server port to listen on, default `8911`
* `path` default realitive path, default '.'.
* `scriptfolder` path to the script folder default `script'.
* `cardcdb` path to the card database default `cards.cdb`.
* `banlist` path to the banlist file default `lflist.conf`.
* `errorlog` enable logging to file on fatal error, default true
* `consolelog` enable console logging, default true
* `handshuffle` enable disable hand shuffling, do not use with out the Buttys/ygopro `ocgcore.dll` and not Fluorohydride/ygopro
* `autoendturn` enable auto ending the turn of players that run over time.
* `clientversion` set the version of the core is expecting
* `splashscreen` enable disable the startup art, default enabled. Requires `consolelog`
* `stdoutsupport` enable additional standard streams API support for [MyCard](https://github.com/mycard/ygopro-web) and [SalvationDevelopment](https://github.com/SalvationDevelopment/YGOPro-Support-System), and new programmers that do not wish to develop against YGOPro's TCP network protocols.

##Standard Streams API##
The ygocore and the management software communicate via standard streams or TCP network. The core signals the management software its current state via a specific API, standard out stream comes via the console/terminal, these signals should not be confused with debug messages. Each call starts with `::::` to check for if you are programming against it, this is an indication of a standard stream API call and not a debug message.

* `::::network-ready` signal that the core has loaded and is listening on its given port
* `::::network-end` signal that the game has ended,  ie kill core request.
* `::::join-slot|#|PlayerName` PlayerName has joined the duel in slot #.
* `::::left-slot|#|PlayerName` PlayerName has left the duel in slot #.
* `::::spectator|#` number of spectators where # is an integer.
* `::::lock-slot|#|bool` slot #'s deck is locked in/out, and `bool` is True or `False`.
* `::::startduel` RPS has started, this signals that the game has started.
* `::::endduel|WinningPlayerSlot#|Reason` the winning player slot integer, and how they won.
* `::::chat|PlayerName|msg` PlayerName sent a message containing the text of `msg`. If the server speaks PlayerName is `[Server]`.

By default these commands are off. Requires `stdoutsupport = true` in the config.txt be set to true. For more information see [the wikipedia article about it](http://en.wikipedia.org/wiki/Standard_streams).

##Request Strings##
When YGOCore recieves a game request command from a connecting client it will parse and handle the request in one of 3 ways. First it will see if a game using that sting is currently active and then conncect the requesting client to that game. If not it will parse the request

###Percy/FH/TDOANE Notation###
`nnOOOnnnn,n,n,xxxxx`

###DevPro Notation###
`nnOOOnnnn,n,n,nN,xxxxx`

###'MyCard #M/F Notation### 
