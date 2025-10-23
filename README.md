# Dungeon Decks (Working Title)

**Course:** ECS657U / ECS7003P - Prototype Submission
**Group Members:** Acil, Yasir, Khaled, Alzain

---

## About The Game

This is a prototype for a 3D single-player game that blends third-person exploration with turn-based, card-driven combat.

The core concept is that the player explores a dungeon to find clues. The objective is to escape the dungeon by finding clues through exploration, the player may encounter an NPC and engage in a turn based card combat.

## Core Prototype Features

This prototype is a **"minimum viable product"** designed to test the core gameplay loop: the transition from exploration to combat.

In line with the module's marking criteria for "Focus", this prototype implements *only* core features. All non-core elements (like deckbuilding, card rewards, and checkpoints) have been cut to be developed for the final game.

* **Two Game Modes:** A core `GameManager` script that switches the player between "Out-of-Battle" exploration and "In-Battle" combat.
* **3D Exploration:** The player can navigate a small dungeon environment using keyboard and mouse controls.
* **Card-Action Combat:** A turn-based battle system where the player uses a fixed set of 2-3 card-actions (e.g., Attack, Dodge) to fight an enemy.
* **Scripted Enemy:** A single enemy NPC that follows a simple, "scripted sequence of actions" to provide a predictable challenge for testing.
* **Win/Loss Condition:** The battle is decided by a simple Health system for both the player and the enemy.

## How To Play

1.  Use the movement controls (see below) to explore the dungeon.
2.  Walk into the designated "battle trigger" area to start combat.
3.  The camera will shift, and the combat UI will appear.
4.  You and the enemy will take turns. On your turn, select a card-action (e.g., "Attack" or "Dodge") by clicking on it.
5.  Click the "End Turn" button to finish your turn.
6.  The enemy will then perform its scripted action.
7.  Reduce the enemy's health to 0 to win the battle and return to exploration.
8.  If your health reaches 0, you lose the game.

## Controls

[cite_start]Controls are implemented using Unity's new Input System[cite: 92].

* **Out-of-Battle:**
    * **Forward/Backward:** `W` / `S`
    * **Strafe Left/Right:** `A` / `D`
    * **Rotate Camera:** `Mouse`
    * **Pick up:** `E`
    * **Throw item:**: `Left Mouse Click`

* **In-Battle:**
   * **Draw Card**: `Mouse`
   * **Attack**: With Card Drawn: `Click on Enemy`

## Technical Note: Git LFS

**This repository uses Git LFS (Large File Storage)** to manage large game assets.

Please ensure you have [Git LFS installed](https://git-lfs.github.com/) to clone or pull this project correctly. This is required by the submission guidelines.

## External Assets Used

As per the coursework requirements, this prototype uses **only** the provided starter assets. All assets are appropriately acknowledged below. No other external or self-created assets were used for this prototype.

* **Card Shirts Lite:** `https://assetstore.unity.com/packages/2d/gui/card-shirts-lite-165698`
* **TornioD'uva Card Pack:** `https://tornioduva.itch.io/tornioduva-card-pack`
* **Ultimate Low Poly Dungeon:** `https://assetstore.unity.com/packages/3d/environments/dungeons/ultimate-low-poly-dungeon-143535`
* **Dungeon Skeletons DEMO:** `https://assetstore.unity.com/packages/3d/characters/creatures/dungeon-skeletons-demo-71087`
* **GUI Parts:** `https://assetstore.unity.com/packages/2d/gui/icons/gui-parts-159068`
* **RPG Hero PBR HP Polyart:** `https://assetstore.unity.com/packages/3d/characters/humanoids/fantasy/rpg-hero-pbr-hp-polyart-121480`
* **RPG Monster Partners PBR Polyart:** `https://assetstore.unity.com/packages/3d/characters/creatures/rpgmonster-partners-pbr-polyart168251`
* **Dragon for Boss Monster PBR:** `https://assetstore.unity.com/packages/3d/characters/creatures/dragon-for-boss-monster-pbr-78923`
