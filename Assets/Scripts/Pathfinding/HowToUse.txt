This package is a tilemap implementation of A*, making use of Unity's Job System and Burst compiler for improved performance.
As a result, it is dependant on the following packages: 
Unity Mathematics, Unity Collections, Unity Burst and Unity 2D Tilemap Extras.

The system is designed to be easy to use, performant and extendable. 
It works with normal tiles, isometric and hexagonal (both flat and pointed top).

This package is compatible with WebGL, 
however, as WebGL is a silly little goofer, it does not support Unity's Jobs or Burst, 
meaning it does not benefit from the performance boosts those provide.
Nevertheless, it should still be quite performant for web-based games.


Pathfinder
The base pathfinder does most of the lifting for you. It can find a path between two positions, or simply find every reachable tile from a starting position.
You can also find multiple paths at once, which is multithreaded so it calculates multiple paths at the same time.
The type of grid, whether diagonal movement is allowed and the maximum distance can be configured here.
It can convert grid positions to world positions and vice versa.

You can implement your own rules for what tiles are walkable and what the cost of each individual tile should be, 
or you can simply use the premade solution that makes any occupied tile unwalkable.

Extras
- Utility functions like getting the distance between two tiles and getting all neighbours of a tile.
- A path class with a few useful features, such combining two paths together or removing all non-corner points.
- A PathDrawer to easily visualise a path.
- Premade rule-tiles for normal, flat-top hex and pointed-top hex tiles to help you get started with your game!