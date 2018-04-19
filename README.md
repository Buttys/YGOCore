This client is no longer supported by the latest version of YGOPro. As I no longer work on this project I would recommend you check out the one currently managed by my friend IceYGO!

- https://github.com/IceYGO/ygosharp

Thanks for the support, the code here is available for anyone interested learning the basics of ygopro client-server communication.

YGOCore
===================
YGOCore is a duel server for the popular game YGOPro written in C#.

## Details ##

The server runs on windows and linux systems when compiled with mono and supplyed with the native ocgcore.dll/so for that system.

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
