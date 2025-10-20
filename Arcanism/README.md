
# Arcanism - a large Arcanist-focused overhaul mod for Erenshor

## Compatibility

Created for Erenshor v0.2 (early access). At the time of writing (Oct 2025) I'm not intending to regularly update and maintain this as the game's very early access nature makes it likely the mod will break frequently, and core gameplay systems and balance will probably fundamentally change over the next year.

Arcanism should generally be compatible with most other mods as it aims to be as unintrusive as possible, however it does touch several core systems so there's always that risk.

There is a **known minor compatibility issue** with the [Extended Hotbars](https://discord.com/channels/1099145747364057118/1366557459212402850/1425781245283139645) mod, which is just that the two revised skills in Arcanism (Control Chant and Twin Spell) won't have their cooldown triggered in certain circumstances if they're in one of the custom hotkey bars. You can work around this by ensuring they're in the game's regular hotkey bar, while still using the extended bars for spells.

## Installation

This mod requires [BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html) which is a simple and light weight platform to support mods for Unity games, so install that in Erenshor's directory first if you haven't already! For a Steam installation, the path should look like "\Steam\steamapps\common\Erenshor\BepInEx"

Grab the latest [release of Arcanism]() and place it in the BepInEx/plugins folder (if it doesn't exist yet, feel free to create it). You should place the entire folder in there, as the mod comes with image assets in its own subfolder. 

So, your final install path (for a Steam release) should look like "\Steam\steamapps\common\Erenshor\BepInEx\plugins\Arcanism\"!

## Overview

### Design Direction

Using an Arcanist as your main? This mod is for you! Arcanism rebalances the game in various ways whilst also adding a small amount of Arcanist-focused new content, all with the aim of providing a satisfying progression curve and gameplay with a slightly higher skill ceiling than vanilla.

The overall feel is that every spell unlock is meaningful and useful at the time you get it (assuming you get them in order), often without directly replacing your others to become your new "main spell" -- in fact you'll usually have 3 or 4 main damage spells on rotation. Spells are more distinct from each other due to overall differences in damage, cooldown, cast time and mana cost. 

Skill changes make combat a little more dynamic and a little more strategic in how you utilise your mana and spells. Make sure you read the changes to Control Chant and the new skill, Twin Spell!

The mana economy is tightened up with regeneration changed, spells costing a bit more in general, and your abilities giving you powerful ways to expend your mana. You'll often find yourself running out during a fight and using the Meditative Trance skill to fill it back up. This makes the mana reduction from resonating spells more important, and also makes the Wisdom stat more important.

Whilst this mod isn't intended to make the game easier, it DOES have a net effect of making the Arcanist slightly stronger. To balance this, I'm personally playing with a 3 man party (Druid+Paladin), which feels like a nice amount of difficulty and lets my Arcanist be the main DPSer, while also making combat a bit less chaotic and easier to follow what's going on compared with a 5 person party.

Note that this mod has been balanced with Arcanist mains in mind and so the Simplayer Arcanist AI hasn't been adjusted to account for the skill or spell changes (ain't nobody got time for that!)

### Recommendations

I recommend using the following mods alongside Arcanism for the best experience.

- My own [BigDamage](https://github.com/cammaron/erenshor-mods/tree/master/BigDamage) mod to ensure you can read how much damage you're doing!
- The [Loot Rarities](https://discord.com/channels/1099145747364057118/1419995182710919279) mod without which you will NOT be able to get over 100 resonance, which is required for Arcanist endgame builds

### Changes

#### New Content

- Backfire damage from failed Control Chant attempts (see Control Chant section) and 2 new passive skill books that improve it!
	- Expert Control I reduces the backfire chance and makes it so even when your early release spells backfire they still do half damage to the target
	- Expert Control II further reduces the backfire chance and makes it so backfired spells still do full target damage
	- This means the risk:reward ratio of early release improves throughout the game, and once it does full damage, even a backfire is still useful -- becoming a little bit like blood magic!
- Brand new Twin Spell active skill, allowing you to target another enemy while casting and duplicate your single target damage spell to them once cast finishes
	- Twinning the spell requires more mana than simply casting the original spell twice
	- A late-game passive skill book called Twin Spell Mastery allows you to twin your spell to a THIRD target!
- 3 new items for the Arcanist's leg slot
	- Trickster's Pants (level 8)
	- Tattered Wrap (level 14) - featuring a handmade graphic by yours truly!
	- Lunar Weave (level 39) - likewise! This is arguably a new BiS for the Arcanist, mainly because I just didn't want to wear a pair of trousers as my current endgame gear
- New chest armour
	- Novice's Robe (level 6) - replaces the Spidersilk Shirt as a weaker early-game item for sale from Edwin Ansegg's shop in Port Azure. Spidersilk Shirt is now a drop instead
- 2 new Arcanist ascension skills to level up
	- Mind Split decreases the mana cost exponent for twinning spells to additional targets
	- Refraction decreases the (initially very long) cooldown for Twin Spell ability by 15%/level
	- Together, these will allow for late-game Arcanist empowerment shenanigans!
- New spell replaces Funeral Pyre: Eldritch Warp
	- A DoT spell which, compared with the existing Burning Chains spell line, has a shorter duration, higher damage per tick, and no debuffs to movement/attack speed
	- Does void damage instead of elemental and stacks with the Burning Chains line, meaning more strategic DoT droppin' during combat!
- Details for where and how all new items (incl skill books) can be acquired may be found below

#### Spell Balance

Every Arcanist spell has been rebalanced by hand and with great care. Target damage spells now fall (roughly) into light, medium and heavy categories, although all others have been changed as well -- read on!

- Heavy spells have the highest damage, cast (7 - 8s) and cooldown (18 - 24s) times. Their mana:damage efficiency is the greatest of the spells, so you generally want to have them on cooldown! Their higher damage and cast time also makes them synergise well with the skill changes.
- Medium spells are closer to the vanilla game's spells, with around a 3s cast time and 6s cooldown (varies). 
- Light spells have a very fast casting time (0.5 - 1s), short cooldown (1 - 2s) and lower damage. They're the highest raw DPS option but their mana:damage efficiency is very bad compared to heavy spells, so you'll run out really quickly if you spam them!
- DoT spells like Burning Chains and Immolation are a tiny bit stronger, much faster to cast, and much lower cooldown. This makes them much more economical and desirable to keep stacked on enemies, especially in the new downtime while your heavy and medium spells are on cooldown.
- Jolt/Concussion fall firmly into the light spell category, but doing less damage, higher mana cost, and much shorter stun duration (10s down to 1.5s) compared to vanilla. However, their rapid cast time means you can spam them to partially disable an enemy (at high mana cost!), which feels a little like gradually electrocuting them. Obviously, they're BiS for interrupting enemies.
- Sleep has a reduced cooldown to make it more viable to drop it on multiple enemies during a battle, but it also has a higher mana cost, so has to be used considerately.
- Coma is no longer a direct replacement for Sleep, as it fills a new niche. It has a much shorter duration than vanilla, but puts enemies into a more powerful state of suspension -- from which damage won't necessarily interrupt them, and the chance for them to be woken up by damage is even lower than the stun state from Jolt/Concuss. This makes it a more powerful control spell to use on a target you're actively attacking rather than trying to keep out, but it has a longer cooldown to make up for it.
- Funeral Pyre has been replaced by the Eldritch Warp spell, described above under New Content. 
- Mind control spells (Invasive Thoughts, Twisting Mind etc.) now work on enemies 3 levels higher than vanilla, ensuring the spell is useful against targets stronger than you at the time you get it (because if you're fighting weaker targets... you don't need it!)
- Finally, the armour spells like Magical Skin have been re-imagined. They now have a short duration (~10s) but MUCH larger damage shield and 0.5s cast time, whilst still having a very long cooldown. This means they're your "oh shit" panic button when being attacked or targeted, effectively mitigating a whole lot of damage for a very short duration, and feasible to be used mid-battle in a pinch.

#### Stat Balance

- Regen has been changed to be more stat focused
	- The "meditative state" (sitting still regen buff) has been massively nerfed from 50x to 2.5x
	- The "Nourished" buff from food has had its regen bonus reduced from 25 HP and Mana per tick to 9
	- Natural regen from Endurance, Wisdom and their respective proficiencies has been boosted, so that these stats play a much more important part in keeping you alive and fighting
	- You will run out of mana during fights if you don't use it wisely, and simply sitting still during a fight will no longer fill your mana almost immediately, making the Arcanist's Meditative Trance skill useful
- Spell damage formula has been completely changed so that it has no "additive" damage component based on int any more, meaning weaker spells will scale but will still always be proportionately weaker than later spells
	- This was necessary both to ensure unlocking more powerful spells felt rewarding, and because early spells hitting so hard meant they were SIGNIFICANTLY more damage:mana efficient. No longer!
	- The new formula is entirely multiplicative and has stronger scaling with your character level, Intelligence and Intelligence Proficiency
	- The formula, and each spell accordingly, has been tweaked so that damage feels about the same as vanilla throughout the game, though the end result of course is that early spells will be much much weaker than your later unlocks
- Roaring Echoes' damage formula has been changed
	- In vanilla, spells that resonate only do 30% their normal damage, but if Roaring Echoes triggers, they do full base damage plus 1% per point of resonance over 100
	- This meant the most significant part of Roaring Echoes was simply getting to 101 res, with additional res points not having a hugely substantial benefit considering that they're much more difficult to squeeze out at those high levels
	- In Arcanism Roaring Echoes is initially weaker (45% base spell damage instead of 100%), but resonance points over 100 have a scaling **exponential bonus** to the damage
		- Each point of res from 100 to 104 increase damage 21% (exponentially per point, so with 104 res your spell damage will be Base x 0.45 x 2.14 = (Base x 0.96), whereas vanilla would be (Base x 1.04)
		- Points from 105 to 108 have a reduced exponent of 7.5%, so at 108: Base x 0.45 x 2.14 x 1.335 = (Base x 1.285), compared with vanilla (Base x 1.08)
		- Points from 109 to 112, the exponent is 5%, so at 112: Base x 0.45 x 2.14 x 1.335 x 1.215 = (Base x 1.56) vs vanilla (Base x 1.12)
		- Points from 113 and beyond give an exponential boost of 2.5% per point
	- At time of writing in _vanilla_ it's actually impossible to get resonance over the mid/high 80s or something. Arcanism does contain some new items and buffs to help get it higher, but keep in mind my Loot Rarities mod recommendation to stack that res nice and high.
- The benefit Charisma applies to reducing enemy resistances in vanilla is entirely negligible, so this mod buffs it quite a bit to make it a useful and desirable stat, and you'll also want to put some proficiency points in it!
	- Enemy resists (for your spell purposes) are now reduced by Charisma * ProficiencyAsAPercentage * 0.64. For example, with 50 Charisma and 13 Cha Proficiency, enemy resists would be lowered by 4.16%. With 130 Charisma and max (40) proficiency, this would be a 30% resist reduction -- potentially extremely significant against an enemy with high resists, and on enemies with low resists it will push them into the negative allowing you to do X% more damage.

#### Equipment Changes

All leg, chest and feet slot equipment for Arcanists have been hand tweaked and rebalanced to ensure a steady stream of upgrades throughout the game. In vanilla, you don't tend to find many Arcanist focused pieces until you get a lot at once near the end!

In general, items have been buffed -- just to allow room for there there to be meaningful differences between them all the way along. New gear will be exciting!

(At the time of writing I'd very much like to do this for other slots too, but am out of time/energy!)

#### Control Chant

The Control Chant skill has been revamped with the aim of making it useful, more flexible, and also not just something you use every time you cast a spell.

- 10 second cooldown (increased from around 2 seconds)
- Releasing a spell early now does the full normal spell damage, but with mana cost and cooldown both reduced proportionately to how far along the spell casting progress is. 
	- If you use Control Chant to release a spell at 30% progress, it will cost 30% of its normal mana and have 30% of the normal cooldown duration and so on for 80% etc.
	- Balancing this is the risk of a spell backfire. The backfire chance is inversely proportional to the above, and when a spell backfires, by default it does NO damage to the enemy, instead doing damage to the caster equivalent to double the spell's base mana cost. 
	- Releasing a spell at 30% cast progress will have a 70% chance of backfiring, and at 90% progress the backfire chance will be 10%.
	- New skills exist to improve this (see the New Content section)
- When releasing a spell _late_, Control Chant now progressively drains additional mana (in real time) the longer the cast is held. The time it can be held has been increased a little compared with vanilla, and likewise the associated rewards. It still also increases the spell's cooldown after use.
	- Erenshor doesn't deduct a spell's mana cost until the spell is actually released. This means that while over-chanting (and your mana being drained), you will need to watch that your remaining mana doesn't drop BELOW the spell's base cost which you will still need when you release it. 
	- If your mana drops below this level, you likewise experience a spell backfire, cancelling your spell and damaging you greatly (full overchant+base spell mana cost times two).
- Can NO LONGER be used on skills/spells from items that have 0 mana cost
	
The result of this, combined with the spell balance changes, is that it's more difficult to decide when to use your this ability and which spell to use it with--especially once taking into account the other new spell augment ability, Twin Spell.

- If you're using a heavy and slow spell, over-chanting is fantastic to pre-cast that on an enemy before they're targeting you. 
- Releasing a heavy spell early is also a great way to cut its cast time and cooldown during a fight, but with proportionate risk
- Since its cooldown is shorter than most heavy spells, you'll be facing similar decisions about how you use your medium spell, or whether to save the Control Chant for your heavy coming off cooldown.

#### Twin Spell

Pretty much described in the New Content section, but this is a new active meta-spell skill that can be used while casting to make your single target damage spells hit additional enemies! See above for that info.

### Where do I get the new stuff? Loot/vendor changes

- Skill Book: Twin Spell - from Braxon Manfred in the Braxonian Desert, who is now an "Exotic Books" trader. Requires Arcanist level 10+ and 28,000 gold
- Skill Book: Expert Control I - from Braxon Manfred, requires level 13+ and 33,000 gold.
- Skill Book: Twin Spell Mastery - requires level 25+, rare drop from Fenton the Blighted in The Blight
- Skill Book: Expert Control II - requires level 29+, rare drop from Elwio the Traitor in Vitheo's Rest

- Trickster's Pants: rare drop from Molorai Militia Arcanists in Old Krakengard and Vitheo's Watch
- Tattered Wrap: rare drop from Priel Deceiver in the Malaroth's Nesting Grounds or Windwashed Pass
![Tattered Wrap](Assets/Items/90000005.png)
- Lunar Weave: rare drop from Vessel Siraethe in the Jaws of Sivakaya
![Lunar Weave](Assets/Items/90000006.png)

- Novie's Robe: purchasable in place of Spidersilk Shirt from Edwin Ansegg in Port Azure (I got lazy so this one is just a recolour of a vanilla item)
![Novice's Robe](Assets/Items/90000007.png)
- Spidersilk Shirt: now a rare drop from Risen Druids in Fernella's Revival Plains

## That's all!

Credit of course to Brian for his amazing game! Have fun!