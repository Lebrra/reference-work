# VR Drawing Matcher
This is part of a project for my VR class in Spring 2025.

This project allows the user to draw with their right hand (or mouse in editor) and test if the drawing matches one of 3 premade drawings. 
Drawings can also be saved for future matching:
- in Unity editor during play 
- select a TubeRenderer_# GameObject
- right click on TubeRenderer (Script)
- select 'Save Drawing Data'
- find new drawing in Assets/Resources/Drawings/'Drawing_newDrawing'
- to add to matchable drawings, edit the list of Drawings To Match on the DrawingManager GameObject (in or out of play)

I would like to turn this into a cleaned up package at some point with some additional features such as a customizable shader, tweens, particle effects, and more accurate shape comparisons.