YGOCore
===================
YGOCore is a duel server for the popular game YGOPro written in C#.

## Beta Details ##

The main goal of this project is to lower the entry level for new and experienced programmers.

This server currently only runs on windows, but will fully support Linux using Mono when fully released.

A sample client to work with the server will be provided in the near future. 

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
