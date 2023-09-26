# Starfield Digipick-Locking Minigame Simulator

## Table of contents
1. [Link To Web Build](#linktowebbuild)
2. [Description](#description)
3. [Importing](#importing)
4. [Breakdown](#breakdown)
5. [Future Development](#future-development)
6. [Visuals](#visuals)

## Link To Web Build
https://bb-dev.itch.io/starfield-digipick-simulator


# v.2 notes
  -Made the webgl play nice(r) with mobile devices.

  -Added the Streak Manager/Daily Modes. Try a preseeded Master lock every day and keep your streak going.

  -Added audio (captured with a loud TV and a phone. Not the best but c'est la vie)

  -Undo/Auto buttons. Undo will roll back the last input, auto will attempt to align an available pick with the available holes.

  -Ring highlighting, shows which picks belong in which segments. Turned off for Daily Mode.
  
  -Probably a couple more things but I wasn't keeping notes, oops.

## Description

This is a recreation of the lock-picking "digipick" minigame from Bethesda's Starfield. Bethesda games always have great lockpicking minigames for some reason and Starfield is no exception to that rule. This project is made with C# and Unity 2022.1.23f1, though it should work with most versions of Unity.

## Importing

To use this project in your own Unity project, download the "StarfieldDigipickSimUnityPackage.unitypackage" and import it into your project. The rest of the files in this repo are simply for reference, the unity package will import all of these files itself.

## Breakdown

As far as I can tell, the digipick in Starfield works something like this.
  1. The difficulty is selected. Novice and Advanced have only two rings (segments in my code), while expert has three and master has four.
  2. Two "picks" are generated for each ring/segment. This can vary from 1-4 possible pick points in each pick, depending on difficulty. These picks then "poke holes"/remove segments from their respective ring/segment. On harder difficulties, additional dud/fakeout picks are generated, which may accidentally be correct.
  3. The player changes picks/rotates picks, filling in empty holes in rings/segments until the puzzle is complete.

## Future Development

I didn't get around to putting audio in, I figure the audio is an important part of the Starfield version so I want to take the time necessary to add in the closest version to the original that I can. Also, this version is missing the feature from the main game where you can auto-slot in picks. This wouldn't necessarily be too hard to implement but I would want to play the main game and take notes on how the feature works there first.

Future-future development, this would make a great mobile game. Hit me up Todd.


## Visuals
![Alt text](Sprites/digipickTitle.png "Digipick Screenshot")