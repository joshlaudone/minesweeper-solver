# minesweeper-solver

This project aims to create a minesweeper solving algorithm for the no guessing variation of minesweeper that is as efficient as possible.

The boards I used for test cases came from minesweeper.online. Their boards for this variant are the hardest I have found.

I chose to focus on the no-guessing variation as it is the version of the game I usually play and it would be easier to test knowing that all of the boards are solvable.

Here is a high-level overview of the algorithm:
TODO

Overview of folder structure:
    The Minesweeper folder contains the Unity project, which has a recreation of Minesweeper in Unity as well as the actual algorithm.
        All code is located in Minesweeper/Assets/Scripts
        The algorithm is in Sweepotron_AI.cs
    minesweeper-image-parser contains a quick python script that I used to translate screenshots of boards from minesweeper.online to a format that could be loaded into the Unity project for testing purposes.
        This was quick and dirty, so there are some hardcoded values for parsing the image.
        Depends on two libraries: matplotlib and skimage