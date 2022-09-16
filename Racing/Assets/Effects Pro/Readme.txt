Effects Pro : Simple Image Effects
Created by Arnab Raha

Version 1.0.0 (9 March 2021)

License: MIT License - https://en.wikipedia.org/wiki/MIT_License
This basically means you can do whatever you want with the source code, provided you keep the comments at the top and don't claim ownership of any codes included in this asset. I'm not responsible if anything goes wrong with your software.

This asset can really be useful when you want to add some minimal graphic 
customizations to your game. This package will provide the following
facalities :

1. Contrast and Brightness adjustments.
2. 3 different but useful image effects : Greyscale, Sepia, Negative and
   their style strength control.

This is the first release of the asset. Version 1.0. Here I have added some
minimal image effects. I will add more options and effects on future updates
If you find any bugs or have any compliance with this asset, it would be a
help if you contact me via e-mail (arnabraha501@gmail.com).

How to use:
===========
Simply add the Effects.cs to the Camera to apply the image effects to the
renderer.

To set any image effect from another script, create a reference and call
the SetFx(Fx effects) method.

For example, to set Sepia effect, write SetFx(Fx.sepia); for Greyscale
effect, write SetFx(Fx.greyscale); etc.

To stop/disable all the image effects associated with Effects.cs, you can
disable the script or call DisableFx(); and to start/enable those enable 
the script or call EnableFx();