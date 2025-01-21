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
