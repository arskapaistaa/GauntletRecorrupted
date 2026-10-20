# Intensity Level
*Intensity level indicates how intensive the level is currently.*

0.0f - 2.4f LOW
2.5f - 4.9f MID
5.0f - 7.4f HIGH
7.5f - 10.0f INSANE

## What increases the Intensity

### Basic enemies (Roach, tick etc.)
**0.0f to 4.0f LOW**

When enemies starts to chase player, one enemies should increse it by 0.5f or something.
4.0f is max Intensity. So 8 enemies will make it full.

### Worm
**4.0f to 8f LOW to MID**

Worm increses by 4.0f when nearby the player.

### Players Health
**8.0f to 10.0f HIGH**

When player haves under:

50% health += 1.0f;

25% health += another 1.0f;
So combined 2.0f;

### Exit is open
Increase Intensity with 2.0f;


## What does this all bullshit mean?

### How i think about the numbers
If there is only few enemies, it is not intense yet. BUT worm itself makes it intense enough to go mid.
Worm close or Enough enemies close + low health goes High already.
Insane level is when enoung enemies + worm is close + exit is open or Player is low health

### How does this affect
Intensity Level will affect enemy spawn rates.
Also it's easiest way to control music.

It can affect:
- Weapon DMG
- Movement speed

