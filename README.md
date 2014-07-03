YGOCore
===================
YGOCore is a duel server for the popular game YGOPro written in C#.

## Beta Details ##

The main goal of this project is to lower the entry level for new and experienced programmers, who just don't have the time required or understanding into something quite complex.

This server currently only runs on windows, but will fully support Linux using Mono when fully released.

A sample client to work with the server will be provided in the near future. 

## Supported Features ##

Single, Match and Tag duelling modes.
Joining games in progress.
Semi-automatic match making system.
Duel setting customization. 

## How to use ##

The server is designed in such a way that when new card effects and rulings are added to the game just recompiling ocgcore.dll from https://github.com/Fluorohydride/ygopro will continue keep the server compatible with all YGOPro clients.

In order to run the server you will require the card scripts, database and the banlist, check the following file/folder structure for a better understanding:

scripts/
YGOCore.exe
cards.cdb
lflist.conf
OcgWrapper.dll
System.Data.SQLite.dll
ocgcore.dll