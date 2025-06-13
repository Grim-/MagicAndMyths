# Magic and Myths - RimWorld Mod Overview

**A comprehensive magic system overhaul bringing new RPG mechanics to RimWorld**



**Magic and myths is a work in progress, there will be bugs, problems that did not even occour to me to fix, and other fun things, it's a large mod with some complex systems, so for the moment while I dont believe it will ruin your save, you're probably best making back up before hand.**

Aside from adding items, equipment and more from various sources of fantasy media most of us know and love, I am adding some systems on top that can be used by other modders, almost every system available has components that can be used, or defs, classes, workers and other things that can be expanded upon to suit needs.



A little about me, I'm Emo. 
I have been programming for over 10 years, some large portion of that spent in solo game development, I am a full time carer and do this in my spare time for fun, I enjoy fantasy stories, media of almost any description.


I am aiming to make this a comprehensive mod for magical systems, mythology creatures, items and more, but I could do with some help! 

Help with any of the following would be a massive boon!
- Art for various pawns, items and visual effects. 
- Creating new abilities, items, artifacts and other things using the various mod systems, this requires understanding what the mod provides, any interesting systems that do not exist, I am willing to add within reason.
- Designing system interactions, dungeon obstacles or themes
- Programmers are welcome also!


I am ofcourse willing to get people upto speed on what is/isnt possible and how you might go about certain things.

People of any experience level can apply, but be warned this is likely still months from release with much work left to be done, if you still feel you'd like to contribute then please get in touch at `.emo_` on discord, or join the LifeIsGame.Inc discord server `https://discord.gg/vqeXhTCXrx` where I am being graciously hosted by the author of the Saiyans and Naruto mods.


## Core Magic Systems


## World & Environment Features

### Procedural Dungeon Generation
- Portal system using paintings/artifacts to access pocket dimensions
- Binary Space Partitioning (BSP) algorithm for realistic layouts
- Configurable dungeon parameters:
  - Room count, size constraints, corridor generation
  - Wall/floor materials and structural elements
  - Difficulty scaling and progression systems

![image](https://github.com/user-attachments/assets/e9686dce-b143-4d5b-87cd-262754708995)

### Encounter Room System
- Multiple room types: combat, treasure, obstacles, recovery
- Progression-based enemy scaling
- Specific encounter configurations:
  - Early game: basic animals
  - Mid game: mixed encounters with explosives
  - Late game: golems and apex predators
- Weight-based random selection with progression ranges

### Transformation System
- Characters and objects transform into different forms
- Shapeshifting magic and cursed item possibilities

### Growable Buildings
- Magical structures that develop and expand over time
- Prefab system for creating custom growth patterns

https://streamable.com/9scyza



### Enchantment Framework

Every item and piece of equipment can be enchanted with magical properties
Multiple effect types: on-hit damage, stat modifications, damage nullification, time-based effects
Categorized enchantments (melee, armor, etc.) with configurable rarity
Visual effects and complex conditional triggers like daylight-dependent bonuses
Auto-slotting enchantment system for specific items
Examples: 
  - Fire Tongue (adds flame damage on hit), 
  - Sunlight (boosts armor and strength during day, weakens at night)
  - Lifedrinking (heals on damage dealt)





### Leveled Abilities & Mastery
- Spells and abilities grow stronger through use
- Mastery levels unlock enhanced effects and new capabilities
- Progression system for colonist magical skill development

### AbilityResource System
- Flexible mana/energy framework supporting multiple resource types
- Sub-resources and complex spell casting costs
- Easy integration for modders to add custom resource types

### Elemental Combinations
- Dynamic hediff system where magical effects combine and interact
- Status effects react to specific damage types (wet + lightning = stun + chain damage)
- Custom damage, effects, targeting parameters, and visual feedback
- Emergent gameplay through elemental synergies

### Artifact System
- Magical items with charges, cooldowns, and dual-purpose functionality
- Items behave differently when used directly vs thrown
- Configurable targeting parameters, usage durations, charge restoration
- Complex effects: pawn storage, healing with custom parameters, multi-target AOE

### Magical Tomes
- Wearable spellbooks that grant abilities
- Built-in enchantment provider slots
- Auto-slotted enchantments for specific magical themes



## Magical Items & Structures

### Buildings
- **Phylactery**: Soul storage device preventing death while intact - essential for Lich survival
- **Philosophers Alchemical Array**: Large magical circle that harvests energy from deaths within radius, produces philosopher's stones that reduce ability costs
- **Lockable doors**: Mostly used for the dungeon obstacles, but can be used anywhere really.

### Magical Equipment  
- **Invisibility Cloak**: Elegant cape granting invisibility abilities
- **The One Ring**: Cursed ring providing invisibility but cannot be removed once equipped
- **Simple Bomb**: Basic throwable explosive - detonates on impact, damage, or heat

### Equipment Systems
- **Cursed Items**: Cannot be unequipped, provide powerful benefits with dangerous drawbacks
- **Soul-bound Weapons**: Bind to specific owners, return through space and time, become worthless to others until owner dies

---

## Classes & Abilities
Magic and myths is also attempting to implement a range of classes and archetypes, will hopefully more tactically interesting choices to make, or just fun.


### Classes
- **Death Knight**: Kill your foes raise them as servants and soldiers, using another mod I created SquadBehaviours, this allows you to organise pawns into squads, you can assign specific objectives to squads, such as patrolling, defending or attacking a target, upon finding a dark heart and recieving the quest to kill a fellow colonist you being your descent into un-death, you slowly lose all mental breaks and inspiriations, death no longer concerns you, creative 

Squad behaviours also allows them to use abilities automatically (unless toggled) while called to arms and in an active squad.
https://streamable.com/mgm13v

- **SpellBlade**: A class with four elemental stances, each providing access to their respective damage type and a few abilities, spell blades are warriors who pick up magic to enhance their fighting skills.



## Advanced Combat & Movement

### Modular Projectile System
- Completely customizable projectiles with interchangeable components
- Available components:
  - AOE damage on impact
  - Fire starting/stopping effects
  - Camera shake effects
  - Sub-projectile spawning for chain reactions
- Independent configuration with custom targeting and friendly fire settings

https://streamable.com/8qzhz4

### Throwing Mechanics
- Full throwing system with effect-on-impact capabilities
- Dual-purpose items: different effects when thrown vs used directly
- Examples: potions heal user when consumed, create AOE healing when thrown
- Prison seals capture targets on impact

https://streamable.com/zc3rh8

### Innate Jumping
- Natural movement enhancement for leaping over obstacles
- New terrain traversal possibilities

### Staged Visual Effects
- Time-based visual systems for expanding spell radiuses
- Evolving magical phenomena over time

### ActiveZone System
- Performance-optimized persistent area-of-effect zones
- Zone capabilities:
  - Apply ongoing buffs/debuffs to pawns in area
  - Trigger periodic effects (lightning strikes, healing pulses)
  - Dynamically modify terrain (floors, walls, filth)
  - Place temporary structures with restoration on removal
- Spawnable by projectiles, spells, or other triggers
- Configurable lifetimes and multiple simultaneous effects

https://streamable.com/t50e4e


### Custom Damage Types
- Necrotic damage that applies stacking debuffs
- Custom damage workers with unique effects
- Specialized hediffs for each damage type


## Quality of Life Improvements

### Radial Menu System
- Streamlined UI organizing abilities into intuitive radial menus
- Reduces gizmo bar clutter significantly

![image](https://github.com/user-attachments/assets/940d13c2-90e2-486b-be2e-fae129c6fad8)
![image](https://github.com/user-attachments/assets/b8ba0f20-d586-4f56-98c4-99c85b10f5eb)

### Enhanced Hediff Types
- Stacking hediffs with custom behavior:
  - Maximum stack limits with severity scaling
  - Stack loss over time intervals
  - Stack refresh on new applications
- Mergeable injuries with custom capacity modifications
- Complex interaction systems and timed durations

### ThingFlyers
- Improved object trajectory system
- thrown items, pawns and weapons.


Here are a collection of gifs ive recorded 

https://streamable.com/fb98nr

https://streamable.com/6ln3x8

https://streamable.com/kph5kb

https://streamable.com/jg7et0

https://streamable.com/0chs0d

https://streamable.com/7lirie

https://streamable.com/syw1jw


---



If you made it this far, you're a trooper! -- Emo

*Magic and Myths is an expansion of RimWorld's systems, adding layers of magical depth while maintaining the game's core survival elements. Looking for collaborators interested in content creation, balancing, and expanding the magical world.*
