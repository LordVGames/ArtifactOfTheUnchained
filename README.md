# Artifact of the Unchained
This artifact prevents proc chains from happening with multiple on-hit items (ATG, charged perforator, polylute, etc.) along with removing the ability to proc on-hit items for pretty much all damaging equipment and for certain items that hit enemies for you (razorwire, unstable tesla coil, etc.)

![artifactoftheunchained](https://github.com/user-attachments/assets/d0f921b5-46bd-427a-99b7-91042c115976)

## Configuration
There are also some items whose ability to proc your items I left configurable:
- All damaging equipment items (off by default)
- Shuriken (off by default)
- Egocentrism (off by default)
- Genesis Loop (on by default)
- Malachite & Perfected elite passive attacks (on by default)

I also made it configurable whether or not procs are able to crit or not, allowed by default. IIRC procs couldn't crit in ror1 & rorr, so it's an option for those who want it.

Along with all that, if everyone in a multiplayer game changes a config option for the artifact using Risk of Options, config options can be changed mid-run! Best to do it in the bazaar or just somewhere where there's no combat going on to prevent any errors.

## Items unaffected
Sadly I couldn't figure out how to do make *every* item unable to proc, though. These are the only ones that AFAIK I'm unable to prevent proccing on-hit items without modifying the items directly:
- Headstompers
- Cryptic Source from Starstorm 2

## Bugs/Suggestions
Due to the way this artifact works I have a feeling that either the procs from some mod somewhere gets turned off when it shouldn't, or I missed an item that needs to be explicitly checked for to prevent item proccing.

So, if you have a suggestion, found a bug, or found an item that isn't supported by the artifact, feel free to either ping me (lordvgames) in the ror2 modding discord or make a github issue here.
