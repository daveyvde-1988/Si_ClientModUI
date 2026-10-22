# 0.4.0 release

Only HUD client and companion DLLs are updated. Full catalogs are transferred in bounded chunks and displayed as continuous lists with side scrollbars, without Next/Previous controls. The full window can be dragged by its title and saves normalized position per Steam user. The collapsed header is lowered to 96 px.

Buy uses production-facility selection followed by all its available units. Ranks and Help are non-clickable tables, with faction-specific rank titles and traitor rules, and categorized commands excluding numeric/back navigation. Root tabs are distinct, SOL is neutral slate, fonts have more contrast and Centauri has subtle procedural scan lines.

An authenticated Generic-admin tab lists loaded server mods. Formation, Logistics, KoH, Crab Cannon and Heavy Teleporter expose reviewed live switches using their own APIs/handlers. Other mods explicitly show no live switch; no generic unloading is attempted. Opening lists makes no changes. Handler save/cleanup behavior applies only when an authorized player clicks a switch.

Both Release builds pass with zero warnings/errors. No tests, runtime launches or toggles were performed. See ../../TESTING.md for user-owned live checks and ../../INSTALLATION_STATUS.md for verified backups and rollback.
