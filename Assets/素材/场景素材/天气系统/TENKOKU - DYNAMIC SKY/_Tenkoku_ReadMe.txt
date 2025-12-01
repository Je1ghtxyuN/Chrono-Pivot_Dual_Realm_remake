---------------------------------------------------
TENKOKU - DYNAMIC SKY

Copyright ©2019 Tanuki Digital
Version 1.2.2
---------------------------------------------------


----------------------------
THANK YOU FOR YOUR PURCHASE!
----------------------------
Thank you for buying TENKOKU and supporting Tanuki Digital!
It's people like you that allow us to build and improve our software! 
if you have any questions, comments, or requests for new features
please visit the Tanuki Digital Forums and post your feedback:

http://tanukidigital.com/forum/

or email us directly at: konnichiwa@tanukidigital.com



----------------------
REGISTER YOUR PURCHASE
----------------------
Did you purchase Tenkoku - Dynamic Sky on the Unity Asset Store?
Registering at Tanuki Digital.com gives you immediate access to new downloads, updates, and exclusive content as well as Tenkoku and Tanuki Digital news and info.  Fill out the registration forum using your Asset Store "OR" Order Number here:

http://www.tanukidigital.com/tenkoku/index.php?register=1



----------------------
SUPPORT
----------------------
If you have questions about Tenkoku, need help with a feature, or think you've identified a bug please let us know either in the Unity forum or on the Tanuki Digital forum below.

Unity Forum Thread: http://forum.unity3d.com/threads/tenkoku-dynamic-sky.318166/
Tanuki Digital Forum: http://tanukidigital.com/forum/

You can also email us directly at: konnichiwa@tanukidigital.com



----------------------
DOCUMENTATION
----------------------
Please read the Tenkoku documentation files for more in-depth customization information.
http://tanukidigital.com/tenkoku/documentation



-------------
INSTALLATION
-------------
I. IMPORT TENKOKU FILES INTO YOUR PROJCT
Go to: “Assets -> Import Package -> Custom Package...” in the Unity Menu and select the “tenkoku_dynamicsky_ver1.x.unitypackage” file. This will open an import dialog box. Click the import button and all the Tenkoku files will be imported into your project list.

II. ADD THE TENKOKU MODULE TO YOUR SCENE
1) Drag the Tenkoku DynamicSky prefab located in the “/PREFABS” folder into your scene list.
2) If it isn’t set already, make sure to set the Tenkoku DynamicSky’s position in the transform settings to 0,0,0

III. ADD TENKOKU EFFECTS TO YOUR CAMERA
1) Click on your main camera object and add the Tenkoku Fog effect by going to Component-->Image Effects-->Tenkoku-->Tenkoku Fog.
Note: For best results this effect should be placed to render BEFORE your Tonemapping effect(if applicable).

(optional)
2) Click on your main camera object and add the Tenkoku Sun Shaft effect by going to Component-->Image Effects-->Tenkoku-->Tenkoku Sun Shafts.
Note: For best results this effect should be placed to render AFTER your Tonemapping effect(if applicable).


A Note About Scene Cameras:
Tenkoku relies on tracking your main scene camera in order to properly update in the scene.  By default Tenkoku attempts to auto locate your camera by selecting the camera in your scene with the ‘MainCamera’ tag.  Alternatively you can set it to manual mode and drag the appropriate camera into the ‘Scene Camera’ slot.




-------------
NOTES
-------------
A Note On Accuracy:
Moon and planet position calculations are currently accurate for years ranging between 1900ce - 2100ce.  The further away from the year 2000ca that you get (in either direction) the more noticeable calculation errors will become.  Additional calculation methods are currently being looked at to increase the accuracy range for these objects.

Integration with CTS - Complete Terrain Shader:
CTS is an advanced terrain shader for Unity with built-in settings to control wetness, rain, snow, and seasonal tinting directly on the terrain itself.  Tenkoku provides an integration script which will automatically drive the effects in CTS according to the weather settings you use in Tenkoku.  To enable this integration, install CTS and Tenkoku in your project, add the CTS weather Manager, then finally drag the /SCRIPTS/Tenkoku_CTS_Weather component onto the 'CTS Weather Manager' object in your scene.
Learn more about CTS here:  https://www.assetstore.unity3d.com/en/#!/content/91938





-------------------------------------------------------------
A note about SRP (Scriptable Render Pipeline) in Unity 2018+ 
-------------------------------------------------------------
(November 2019)
Many people have asked about the new SRP system in Unity 2018+.  Because Unity's new HDRP and URP(LWRP) Render Pipeline is still under active development, and because of the massive differences between these and the current default rendering system, I've decided to take a "wait and see approach" to when/if Tenkoku will support these modes.  Unfortunately the amount of work involved in upgrading to SRP is not trivial.  Once SRP is more mature I will take another look and reconsider if I will rewrite Tenkoku to support them.





-------------------------------
RELEASE NOTES - Version 1.2.2
-------------------------------

BUG FIXES:
- Fixed strange ambient coloring at night (moved reflection probe out of skysphere object).
- Edited legacy atmosphere shader to reduce sun bleed rendering issues.
- Removed TenkokuGaia.cs because of outdated dependency.
- Removed Tenkoku_CTS_Weather.cs because of outdated dependency.





----------------------------
CREDITS
----------------------------
- Cloud System adapted from prototype work by Keijiro Takahashi.  Used with permission.
https://github.com/keijiro

- Temporal Reprojection is modified from Playdead Games' public implementation, available here:
https://github.com/playdeadgames/temporal

- Oskar Elek Atmospheric model originally developed in 2009 by Oskar Elek.  Model has been adapted for Unity by Michal Skalsky (2016) as part of his Public Domain work regarding volumetric atmospheric rendering:
http://www.oskee.wz.cz/stranka/oskee.php
https://github.com/SlightlyMad

- Lunar image adapted from texture work by James Hastings-Trew.  Used with permission.
http://planetpixelemporium.com

- Galaxy image adapted from an open source image made available by the European Southern Observatory(ESO/S. Brunier):
https://www.eso.org/public/usa/images/eso0932a/

- Star position and magnitude data is taken from the Yale Bright Star Catalog, 5th Edition:
http://tdc-www.harvard.edu/catalogs/bsc5.html (archive)
http://heasarc.gsfc.nasa.gov/W3Browse/star-catalog/bsc5p.html (data overview)

- Calculation algorithms for Sun, Moon, and Planet positions have been adapted from work published by Paul Schlyter, in his paper 'Computing Planetary Positions'.  I've taken liberties with his procedure where I thought appropriate or where I found it best suits the greater Tenkoku system.  You can read his original paper here:
http://www.stjarnhimlen.se/comp/ppcomp.html  
