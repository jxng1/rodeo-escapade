# rodeo-escapade
Unity 2D roguelike game(not to be confused with roguelite!) with a western theme.

## Room Generation

Room generation is done procedurally. Rooms are separated using Delaunay triangulation and flocking behaviour(separation). Minimal distance between rooms are found by using graph techniques and a minimal spanning tree implementation. Straight line paths are created between rooms by seeing which side of the room is closest and then picking said direction(as a straight line horizontally or vertically or an L-shape). Finally, any rooms within said path is added to the generated room set, and tiling is applied to the rooms.

![Generation Demo](https://user-images.githubusercontent.com/58788429/158494147-e9ce2a7e-4255-4fac-bf48-d995757313cb.gif)
