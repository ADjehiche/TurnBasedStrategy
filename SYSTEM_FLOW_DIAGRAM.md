# Card Collection System - Visual Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         GAME START (TitleScene)                      │
│                                                                       │
│  GameInitializer.Start()                                             │
│         ↓                                                             │
│  Creates CardCollection singleton                                    │
│         ↓                                                             │
│  CardCollection.InitializeStartingCollection()                       │
│         ↓                                                             │
│  Randomly selects 15 cards:                                          │
│     • 8 Attack (can duplicate)                                       │
│     • 4 Defense (can duplicate)                                      │
│     • 3 Utility (can duplicate)                                      │
│         ↓                                                             │
│  Example: [Quick Slash, Quick Slash, Quick Slash, Power Strike,     │
│            Power Strike, Slash, Slash, Cleave,                       │
│            Block, Block, Brace, Shield Bash,                         │
│            Battle Focus, Disarm, Battle Focus]                       │
│         ↓                                                             │
│  Collection = 15 cards ✅                                            │
│                                                                       │
└─────────────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────────────┐
│                      ENTER BATTLE (Battle Scene)                     │
│                                                                       │
│  DeckManager.BuildStartingDeck()                                     │
│         ↓                                                             │
│  Checks if CardCollection exists                                     │
│         ↓                                                             │
│  Loads all 15 cards into drawPile                                    │
│         ↓                                                             │
│  Shuffles drawPile                                                   │
│         ↓                                                             │
│  drawPile = [shuffled 15 cards] ✅                                   │
│                                                                       │
└─────────────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────────────┐
│                          PLAYER TURN START                           │
│                                                                       │
│  TurnManager.StartPlayerTurn()                                       │
│         ↓                                                             │
│  Tick status effects                                                 │
│         ↓                                                             │
│  Refill stamina                                                      │
│         ↓                                                             │
│  TurnManager.DrawCardsForPlayerTurn()                                │
│         ↓                                                             │
│  CardCollection.DrawHandWithRules(drawPile, 4)                       │
│         ↓                                                             │
│  ┌──────────────────────────────────────────┐                       │
│  │ HAND COMPOSITION RULES:                  │                       │
│  │                                           │                       │
│  │ Step 1: Guarantee 1 Attack card          │                       │
│  │    → Finds all Attack cards in drawPile  │                       │
│  │    → Picks random Attack                 │                       │
│  │    → Adds to hand                        │                       │
│  │                                           │                       │
│  │ Step 2: Guarantee 1 Defense card         │                       │
│  │    → Finds all Defense cards in drawPile │                       │
│  │    → Picks random Defense                │                       │
│  │    → Adds to hand                        │                       │
│  │                                           │                       │
│  │ Step 3: Maybe add 1 Utility (70% chance) │                       │
│  │    → Random 0-100                         │                       │
│  │    → If < 70: Add 1 Utility              │                       │
│  │    → Adds to hand                        │                       │
│  │                                           │                       │
│  │ Step 4: Fill 4th slot (prefer Attack)    │                       │
│  │    → Check if Attack cards left          │                       │
│  │    → If yes: Add random Attack           │                       │
│  │    → If no: Add any random card          │                       │
│  │    → Adds to hand                        │                       │
│  └──────────────────────────────────────────┘                       │
│         ↓                                                             │
│  Hand = [Attack, Defense, Utility, Attack] ✅                        │
│  Example: [Quick Slash, Block, Battle Focus, Power Strike]          │
│         ↓                                                             │
│  Cards displayed in UI                                               │
│         ↓                                                             │
│  Player plays cards → End turn                                       │
│         ↓                                                             │
│  Cards go to discardPile                                             │
│         ↓                                                             │
│  When drawPile empty → Shuffle discardPile back into drawPile       │
│                                                                       │
└─────────────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────────────┐
│                          BATTLE END (VICTORY)                        │
│                                                                       │
│  Last enemy dies                                                     │
│         ↓                                                             │
│  EnemyHealth notifies EnemyManager                                   │
│         ↓                                                             │
│  EnemyManager.CheckBattleEndAfterDelay()                             │
│         ↓                                                             │
│  Wait 0.6 seconds (death animation)                                  │
│         ↓                                                             │
│  Check if ALL enemies dead                                           │
│         ↓                                                             │
│  BattleState.SetOver(true) ✅                                        │
│         ↓                                                             │
│  BattleRewardManager detects battle over                             │
│         ↓                                                             │
│  Wait 2 seconds (victory celebration)                                │
│         ↓                                                             │
│  BattleRewardManager.ShowCardReward()                                │
│         ↓                                                             │
│  CardRewardUI.ShowRewardSelection()                                  │
│         ↓                                                             │
│  CardCollection.GetRandomRewardOptions(2)                            │
│         ↓                                                             │
│  ┌──────────────────────────────────────────┐                       │
│  │ REWARD CARD SELECTION:                   │                       │
│  │                                           │                       │
│  │ For each of 2 cards:                     │                       │
│  │   → Roll random 0-100                    │                       │
│  │   → If < 10 (10% chance):                │                       │
│  │      • Offer rare merge-only card        │                       │
│  │      • Example: Whirlwind, Iron Fortress │                       │
│  │   → Else (90% chance):                   │                       │
│  │      • Offer starter pool card           │                       │
│  │      • Example: Quick Slash, Block       │                       │
│  └──────────────────────────────────────────┘                       │
│         ↓                                                             │
│  UI shows 2 cards: [Option A] [Option B]                            │
│         ↓                                                             │
│  Player clicks one                                                   │
│         ↓                                                             │
│  CardRewardUI.OnCardSelected(selectedCard)                           │
│         ↓                                                             │
│  CardCollection.AddCard(selectedCard)                                │
│         ↓                                                             │
│  Collection = 16 cards now ✅                                        │
│         ↓                                                             │
│  Reward panel closes                                                 │
│         ↓                                                             │
│  Return to exploration / next battle                                 │
│                                                                       │
└─────────────────────────────────────────────────────────────────────┘
                               ↓
┌─────────────────────────────────────────────────────────────────────┐
│                      NEXT BATTLE (Uses Updated Deck)                 │
│                                                                       │
│  DeckManager.BuildStartingDeck()                                     │
│         ↓                                                             │
│  Loads 16 cards from CardCollection ✅                               │
│         ↓                                                             │
│  Shuffles drawPile                                                   │
│         ↓                                                             │
│  Battle continues with larger deck...                                │
│                                                                       │
└─────────────────────────────────────────────────────────────────────┘


═══════════════════════════════════════════════════════════════════════
                        KEY SYSTEM FEATURES
═══════════════════════════════════════════════════════════════════════

🎴 STARTING DECK
   • 15 cards (8 Attack, 4 Defense, 3 Utility)
   • Duplicates allowed
   • Randomized each playthrough

🃏 HAND COMPOSITION
   • Always 1+ Attack (guaranteed)
   • Always 1+ Defense (guaranteed)
   • 0-1 Utility (70% chance)
   • 4th slot prefers Attack cards

🏆 BATTLE REWARDS
   • Choose 1 of 2 cards
   • 90% starter cards
   • 10% rare merge-only cards
   • Added to permanent collection

🔄 DECK GROWTH
   • 15 → 16 → 17 → 18... cards
   • Collection grows throughout game
   • Resets each game session

♻️ DISCARD & SHUFFLE
   • Played cards → discard pile
   • Empty draw pile → shuffle discard back
   • Ensures all cards eventually drawn


═══════════════════════════════════════════════════════════════════════
                          BUG FIXES INCLUDED
═══════════════════════════════════════════════════════════════════════

✅ Invisibility popup at PLAYER position (not enemy)
✅ Invisibility HIDES player model (SetActive false)
✅ Whirlwind hits ALL enemies (loops through EnemyManager)
✅ Battle ends only when ALL enemies dead (not just one)
✅ Cards per turn set to 4


═══════════════════════════════════════════════════════════════════════
                      PERSISTENCE & SCOPE
═══════════════════════════════════════════════════════════════════════

🎮 GAME SESSION SCOPE
   • Collection persists across scenes (DontDestroyOnLoad)
   • Resets when game closes
   • Perfect for roguelike gameplay

💾 NO SAVE FILES
   • Current implementation: Session only
   • Future: Can add save/load to PlayerPrefs or JSON
   • Would require additional Save/Load methods
```
