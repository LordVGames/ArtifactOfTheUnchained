## 2.3.0
Procs from items now have separate config values for damage/proc coefficient nerfs
Added a config option to blacklist survivors who do not have proper `DamageSource`s setup yet and/or rely on SeekersPatcher
- Any survivors that are like this have the artifact detect their attacks as item damage, which heavily nerfs the survivor's damage.
- Adding a survivor to this blacklist will make them stronger than others since they aren't receiving nerfs for procs from items, but it's better than having barely any damage.

## 2.2.0
Mod now depends on `DamageSourceForEnemies`
- This is so I can gurantee if damage is truely from an item instead of jankily figuring out if damage was a monster or an item

Refactored the hook & method used
- Now uses the hook used in an older version of the mod
- Method code is simplified

Mod now detects modded procs in the proc chain

Replaced individual item configs with a single option for all item procs
- Manging all the config options was kind of a hassle, I may re-add it if there's demand for it

## 2.1.0
Added the console command `unchained_toggle_logging_nerfs` that toggles logging damage/coefficients being nerfed & proc chains being blocked as a sanity check for if the mod is even nerfing things

## 2.0.2
Forgot to change the internal name of the artifact to the proper name

## 2.0.1
Try not to leave in large amount of debug logging challenge impossible

## 2.0.0
Big internal rework! Mod also got renamed too since it doesn't just remove proc chains anymore!

All items now apply a custom proc to the chain, it's only purpose is to track when a damage instance is from an item proc.
This means that procs from items such as fireworks and similar items can now be easily and consistently be detected, even modded items too!
- Due to this I'm pretty sure it won't be vanilla compatible anymore
- Monsters still have to somewhat use the older system since they don't have DamageSource support added

You can configure how much you want proc chains & procs from other items to be nerfed!
- The damage dealt and the proc coefficient can both be nerfed separately
- The limit for proc chains is also configurable, meaning you can let procs chain but nerf their damage/proc coefficients

Added a serverside config option, letting you play with the artifact's effects without needing to actually activate it in the lobby

## 1.2.1
Aspect attacks config supports the new gilded spike attack now (i forgot)
More checks for monsters' items proccing their other items
Equipment procs config is enabled by default now

## 1.2.0
Mod reworked internally, it also now uses the new DamageSource system meaning EVERY item attack's proc coefficient can be removed when it couldn't before!
- The way the mod detects when damage is taken has also changed, multiplayer seems fine but if there are problems let me know

Added a config options for missiles in general
- This does not affect ATG but will affect Disposable Missile Launcher and Starstorm 2's Armed Backpack

## 1.1.4
Made Sawmerang and Electric Boomerang separate config options that are on by default
(they both have built-in on-hit effects that don't work when their proc coefficient is 0, so AFAIK i have to let them proc items)

## 1.1.3
Added support for SivsContentPack's yellow items

## 1.1.2
SS2's haunted lamp item is accounted for

Fixed sulfur pods not inflicting their debuff

## 1.1.1
The artifact now accounts for boss items added by [EnemiesReturns](https://thunderstore.io/package/Risky_Sleeps/EnemiesReturns/)

## 1.1.0
A bunch of survivors had one or more abilities that would never proc items due to the artifact:
- Huntress
- Bandit
- MUL-T
- Acrid
- SS2's Executioner

All of which are now fixed
Fireworks config default was also set to disabled because i forgot icbm + plimp + fireworks combo exists lmao

## 1.0.5
- Made fireworks proccing items configurable, on by default
- Fixed some of Miner's abilities not proccing anything

## 1.0.4
- Fixed electric boomerang not stunning anything
- Equipment procs config now also prevents preon accumulator procs (i forgot it existed)

## 1.0.3
- Fixed void fog (and possibly other damage sources not from an enemy/item) erroring out and not damaging anything

## 1.0.2
- Improved the artifact's enabled & disabled icons

## 1.0.1
Fixed the following:
- Blood shrines not damaging the player
- Glacial elite death explosions not freezing the player
- Nemesis Enforcer's hammer slam in the minigun stance not proccing anything

## 1.0.0

- First release
