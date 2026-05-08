# FAM Economy — Space Engineers Mod

A Space Engineers mod simulating a space agency economy loop inspired by *For All Mankind*. Players operate as a space agency, purchasing components from independent contractors to fund and build missions, achieving milestones to unlock grant funding, and eventually establishing off-world production infrastructure.

---

## Design Philosophy

The standard Space Engineers survival loop — mine ore, refine ingots, build components — is replaced with a **milestone-based economy**. Credits are not earned through grinding; they are unlocked by achieving mission objectives. Supply is managed by simulated vendors with dynamic pricing driven by resource availability and demand pressure.

The mod is designed to stay out of the way. It handles only what the game cannot do natively, and degrades gracefully when expected infrastructure is missing.

---

## Core Systems

### Vendor Economy

A network of independent contractor vendors supplies components, gases, and consumables. Each vendor:

- Receives raw resource deposits on a configurable restock tick
- Processes resources through an assembler into finished components
- Adjusts prices dynamically based on stock levels and resource availability
- Operates independently with its own supply chain personality

Vendors are identified by faction ownership and block naming conventions. No entity IDs are stored — the mod binds to whatever exists and skips what doesn't.

### Dynamic Pricing

Prices are determined by three additive signals:

- **Finished product supply** — stock below buffer size pushes price upward, converging to base price at full stock
- **Resource availability** — raw material scarcity pushes prices at a higher weighting than finished stock, modeling anticipated future shortage
- **Demand pressure** — order frequency and velocity over a rolling window, with stockout events permanently ratcheting the safety stock threshold upward

### Milestone Grants

Credits are unlocked exclusively through mission milestones — not through any farmable loop. Milestones are detected automatically where measurable (orbital parameters, surface landing, grid composition) and claimed manually at mission control for narrative milestones.

Automatic detection examples:
- Satellite achieving and sustaining orbital altitude and velocity
- Grid with agency tag touching down on a planetary body
- Minimum hab module count present on a surface grid

### World State Persistence

Vendor safety stock levels, demand history, and milestone completion state persist across world saves via world storage. On first run, vendors are seeded with full buffer stock. Subsequent loads resume from persisted state.

---

## Block Naming Convention

Vendor station blocks are identified by custom name. All blocks must follow this convention:

```
[BlockType].[TAG]
```

Examples for Tectron Metal (TEC):
```
Storage.TEC       — cargo container, receives raw resource deposits
Store.TEC         — store block, manages sell offers
Assembler.TEC     — assembler, processes resources into components
Reactor.TEC       — reactor, receives uranium fuel deposits
H2O2.TEC          — gas generator, receives ice deposits
Refinery.TEC      — refinery (optional)
Safezone.TEC      — safe zone block (optional)
```

Missing blocks are logged on init and skipped gracefully. The mod processes whatever is present.

---

### Resource Acquisition

- **Earth** — vendor purchases only. No mining or harvesting.
- **Luna** — ISRU unlocks after establishing 3 hab modules on the surface.
- **Mars** — same threshold as Luna, longer supply line from Earth.

### Infrastructure

- Large blocks must be stationary. Anything that moves must be small grid.
- Vendor stations are pre-built in creative and owned by a dedicated NPC faction.

### Ships

- H2 thrusters are forward-facing only (script enforced).
- RCS thrust is reduced via mod to model realistic attitude control.
- Reactor count is limited via Block Limiter mod to enforce power budgets.
- Weapon loadout: homing missiles, swarmers, laser point defense.

### Credits

- Credits are milestone-gated. No farming loop exists.
- Government budget grants deposit on a fixed timer.
- Commercial satellite launch contracts provide secondary income.

---

## Architecture

```
EconomySession              — session component, entry point, server-side only
  VendorManager             — owns vendor states, orchestrates tick loop
    GridManager             — block binding, grid tracking, lookup indices
    VendorStore           — per-vendor offer lifecycle, sale detection
  WorldState                — persistence, first-run detection
  ConfigLoader              — deserializes economy_config.xml from mod directory
```

### Key Design Decisions

- **No entity ID persistence** — blocks are found by name pattern on each init scan
- **Graceful degradation** — missing blocks are logged and skipped, never crash
- **Server-side only** — all economy logic runs on the host, clients see results via game sync
- **Config-driven behavior** — pricing parameters, restock rates, and block naming conventions live in XML config, not code

---

## Configuration

Key fields per vendor:
- `restock_tick` — how often resources are deposited (in game ticks, 60/sec)
- `restock_multiplier` — scales deposit quantities
- `procurement` — derived resource requirements per restock tick, keyed by resource category

Key fields per product:
- `base_price` — price floor, used when stock is at full buffer
- `buffer_size` — target stock level, safety stock ratchets upward on stockout
- `type` — SE item type ID used for builder construction

---

## Projected Milestone Progression

| Milestone | Detection | Grant |
|-----------|-----------|-------|
| First satellite orbit | Automatic — altitude + velocity sustained over 3 ping intervals | Large |
| Lunar flyby | Automatic — grid enters lunar gravity well | Medium |
| Lunar landing | Automatic — grid surface contact on Luna | Large |
| Lunar hab established | Automatic — 3 hab modules present on lunar surface grid | Large, unlocks ISRU |
| Lunar ore sample return | Automatic — ore present in Earth return cargo after lunar landing milestone | Medium |
| Mars transit | Automatic — grid exits inner system boundary | Medium |
| Mars landing | Automatic — grid surface contact on Mars | Large |
| Mars hab established | Automatic — 3 hab modules present on Mars surface grid | Large, unlocks ISRU |

Commercial contracts (satellite launches, comms relay establishment) are defined manually in mission control contract blocks and claimed there.

---

## Development Status

- [x] Config loading and validation pipeline
- [x] Grid manager — block binding and tracking
- [x] Vendor state construction
- [x] Deposit loop — resource spawning and spillover
- [ ] Store management — offer lifecycle and sale detection  
- [ ] Pricing model — demand signals and price curve
- [ ] World state persistence
- [ ] Milestone detection
- [ ] CSV analytics log
- [ ] Faction and NPC identity initialization