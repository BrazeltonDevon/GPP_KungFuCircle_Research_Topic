# Kung Fu Circle - AI Research Topic Devon Brazelton 2022 - 2023

A Kung-Fu Circle or less commonly known as a Fighting Circle or Tactical Circle is an AI technique implemented in action games using real-time combat. 
Often in action games a large amount of enemies can be too stressful for players and if too many attack at once they can easily overwhelmed. This is where the
Kung-Fu Circle comes into play. This technique limits the amount of enemies that can attack simultaneously. Depending on your implementation, this can be one at a time or a few according to what your game's balance/difficulty needs are. 

Further expanding on this technique distinguishes between different enemy types and attacks.

An implementation of the Kung-Fu technique is the "Belgian AI" system (named after the waffle-like appearance that comes to mind when explaning the algorithm).
The Belgian AI algorithm involves every creature having a grid around them. Each grid has a maximum capacity for attacking enemies/creatures and an attack capacity.
The total grid weight of attacking enemies must be lower or equal to the maximum grid capacity.

We will use the player as the example for explaning the algorithm, since in my implementation the player is the only one implementing a grid.

![image](https://user-images.githubusercontent.com/96618671/211682943-5ad173d9-9465-46d4-abd1-c08850e348df.png)

Every enemy/creature has a grid "weight" (how much grid capacity it takes for them to enter the fighting circle) and every attack has a "weight" as well. In order for an enemy to be granted a spot to the player's grid, the creature's weight must be less than or equal to the remaining capacity of the grid.
Different creatures can have different weights in order to make them more "dangerous" and therefore costly to the grid.

Now, suppose a creature on the grid has been granted permission to attack the player once they are in the grid. They must now select an attack that has a weight/cost that is less than or equal to the remaining attack capacity. Depending on your implementation you may loop through all of the enemies currently in the grid and give them a chance to attack until there is no more remaining attack capacity in the player's grid.

# My implementation

Now that we have established what a Kung-Fu circle and the Belgian AI system are, I will delve into my implementation of the Kung-Fu circle.

In my implementation, the only character who uses a grid is the player.
This grid can store a limited amount of enemies at a time and implements coroutines to allow only one enemy to attack at a time.
Only enemies registered to the Kung-Fu circle (ApproachCircle.cs) will be included in who is available to attack.

I am approaching my research topic using the Inner-Outer circle technique ontop of the Belgian AI system. 

![image](https://user-images.githubusercontent.com/96618671/211686098-5be4f783-9e90-48c8-bb92-e2aa2ff1523b.png)

This means that there is a "melee range" and an "approach circle". Creatures assigned to be given permission to attack must stand at a slight distance away from the player. If they are given permission, they may approach within melee range of the player and attack them. Afterwards, they will be returned to the approach circle position.

![image](https://user-images.githubusercontent.com/96618671/211684212-5abdc7a0-a440-4993-a49a-7720e4032028.png)

Every slot (represented by the red circles) is a place that a creature can be assigned to wait permission to attack.
With debug drawing on, you can also tell if that spot is occupied by an enemy (blue == occupied , green == not occupied).
The maximum grid capacity is detached from the amount of slots, however both an available slot and available capacity is needed
for an enemy to be registered to the Kung-Fu Circle and allowed permission to attack. To avoid issues, by default I set the maximum amount of slots to be equal
to the maximum grid capacity. Otherwise, if the amount of slots were to be less than the maximum capacity there could be issues where there IS enough capacity but no available slots.

The player's Kung-Fu circle can be customized in the inspector to change the amount of slots, the distance the slots will be from the player's position...
The degrees in which the slots will then spread out from the player (360 degrees making them spread into a full circle, etc...). 
And the drawing of the slots can be toggled on/off in the inspector as well (Make sure to turn on gizmos so they are visible).

The player can also attack enemies. Pressing the left-mouse button will send a short raycast infront of the player which upon making contact with an enemy will deal 1 damage. Each enemy has 3 health and dies upon losing all health. This will unregister them from the enemy manager who before that unregisters them from the list of enemies registered to the approach circle. This frees up a slot as well as grid capacity and allows other enemies to enter combat with the player.
On the topic of the enemy manager, the enemy manager is an overbranching entity I use to control all of the alive enemies. All enemies are children under the manager and will obey its orders. The manager controls who is registered to the player's Kung-Fu circle, when they should be registered, what their movement target is, when to use certain coroutines, and combat. The manager allows one enemy at a time to have permission to attack in the main AI_Loop.

The coroutines that the AI uses for its behavior can be better understood by splitting the behaviors into different states.
-PURSUING
-WAITING
-ATTACKING
-RETREATING

Pursuing: The behavior the AI has when it is not registered in the Kung-Fu circle and must follow the player while staying outside of the radius of the circle.

Waiting: The AI is registered in the circle and when close to their assigned slot's position will pace to the left and right until given permission to attack. When the player moves and the AI is no longer in range (too far from) the slot's position they will no longer pace and will attempt to get back in range.

Attacking: The AI is now preparing to attack the player and has started the process of moving within melee range. To indicate that the enemy wants to attack, a sword appears above their head. Upon being within melee range a sound cue will be played when damage would usually happen in a game.

Retreating: The AI has successfully damaged the player and will move backwards to get back into their assigned slot's position. This leads back into the WAIT behavior until the AI is given permission to attack again.

https://user-images.githubusercontent.com/96618671/211688304-228b2dfd-8602-408f-8a94-7e22e14c7bb2.mp4

Resources:
GameAIPro - Chapter28: Beyond the Kung-Fu Circle
https://www.gameaipro.com/GameAIPro/GameAIPro_Chapter28_Beyond_the_Kung-Fu_Circle_A_Flexible_System_for_Managing_NPC_Attacks.pdf
