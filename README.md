YGOCore
===================
YGOCore is a duel server for the popular game YGOPro written in C#.

## Beta Details ##

The main goal of this project is to lower the entry level for new and experienced programmers.

The server now runs on windows and linux systems when compiled with mono and supplyed with the native ocgcore.dll/so for that system.

A sample client to work with the server can be found here: https://github.com/Buttys/ygopro

## Supported Features ##

* Single, Match and Tag dueling modes.
* Joining games in progress.
* Semi-automatic match making system.
* Duel setting customization. 

## How to use ##

In order to run the server you will require the card scripts, database and the banlist, check the following file/folder structure for a better understanding:

* scripts/
* YGOCore.exe
* cards.cdb
* lflist.conf
* OcgWrapper.dll
* System.Data.SQLite.dll
* ocgcore.dll

In order for users to connect to the server you will need to port forward the relevant port, in this case 8911 is used by default.

The server is designed in such a way that when new card effects and rulings are added to the game just recompiling ocgcore.dll from https://github.com/Fluorohydride/ygopro will continue keep the server compatible with all YGOPro clients.

you can download a precompiled version of ocgcore from the release section https://github.com/Buttys/YGOCore/releases/
but this version might not be the latest version avliable.

##Configuration##
`ygoserver.exe` takes two parameters, a port, and a `*.INI` format configuration file with these configurations. Without parameters YGOCore will look for the above file structure after tying to load config.txt

* `serverport` what server port to listen on, default `8911`
* `path` default realitive path, default '.'.
* `scriptfolder` path to the script folder default `script'.
* `cardcdb` path to the card database default `cards.cdb`.
* `banlist` path to the banlist file default `lflist.conf`.
* `consolelog` enable console logging
* `handshuffle` enable disable hand shuffling, do not sue with out the Buttys/ygopro `ocgcore.dll`
* `autoendturn` enable auto ending the turn of players that run over time.
* `clientversion` set the version of the core is expecting
* `splashscreen` enable disable the startup art, default enabled. Requires `consolelog`
* `stdoutsupport` enable additional standard out API support for MyCard and SalvationDevelopment.

##Standard Out API##
The ygocore and the management software communicate via standard out or TCP network. The core signals the management software its current state via a specific API, standard out comes via the console/terminal, these signals should not be confused with debug messages. Each call starts with `::::`, this is a standard pipe signal to tell the core it is an API call and not a debug message.

* `::::network-ready` signal that the core has loaded and is listening on its given port
* `::::network-end` signal that the game has ended,  ie kill core request.
* `::::join-slot|#|PlayerName` PlayerName has joined the duel in slot #.
* `::::left-slot|#|PlayerName` PlayerName has left the duel in slot #.
* `::::spectator|#` number of spectators where # is an integer.
* `::::lock-slot|#|bool` slot #'s deck is locked in/out, and `bool` is True or `False`.
* `::::startduel` RPS has started, this signals that the game has started.
* `::::endduel|WinningPlayerSlot#|Reason` the winning player slot integer, and how they won.
* `::::chat|PlayerName|msg` PlayerName sent a message containing the text of `msg`. If the server speaks PlayerName is `[Server]`.

By default these commands are off. Requires `stdoutsupport = true` in the config.txt be set to true.