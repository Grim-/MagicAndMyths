# Magic and Myths - RimWorld Mod Overview

**A comprehensive magic system overhaul bringing new RPG mechanics to RimWorld**

Aside from adding items, equipment and more from various sources of fantasy media most of us know and love, I am adding some systems on top that can be used by other modders, almost every system available has components that can be used, or defs, classes and other things that can be expanded upon to suit needs.

## Core Magic Systems

Enchantment Framework

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

*Magic and Myths is an expansion of RimWorld's systems, adding layers of magical depth while maintaining the game's core survival elements. Looking for collaborators interested in content creation, balancing, and expanding the magical world.*
