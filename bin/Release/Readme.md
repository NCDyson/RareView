# RareView
Model Viewer/Dumper for Rare games for the Xbox 360.
Currently only Kameo: Elements of Power is supported, and I use the term "supported" very loosely.

## Viewport Camera Controls:
Default Movement type is a Walk Around type.
|Key| Function |
|--|--|
|W  |Move Backward |
|S  |Move Backward |
|A  |Move Left |
|D  |Move Right |
|Z  |Move Down |
|X  |Move Up |
|R  |Reset Camera Position |
|Shift  |Move/Zoom Slower |
|-(Minus)| Zoom Out|
|=(Equals)|Zoom In|

Hold the Ctrl Key to use Axis Aligned Movement:
|Keys| Axis |
|--|--|
|W,S  |Z Axis (Toward/Away) |
|A,D | X Axis (Left/Right) |
|Z,X  |Y Axis (Up/Down) |

Right click and Drag in the Viewport to rotate the view. Use the Scrollwheel to Zoom in and Out.

## Developer Notes
Internally, Triangle Strips are converted to Triangle Lists. This is done to make it easier to render the UV view.

### Materials
 I have yet to find any actual Material definitions, so "Materials" in the viewer is a placeholder construct that just has a slot for a diffuse texture, which you have to manually set.

### Vertex Attributes
I have also yet to find any vertex attribute definitions for Vertex Batches. I'm not sure if they're hard-coded into the shaders, or stored elsewhere inside the Rendergraph. 

### Rendergraphs
Rendergraphs, like the name implies are a rather complex data structure.
There are several tables inside them to parse, but I haven't figured out what most of the data they hold is, so there's hacks in place to read the Rendergraphs just enough to get some model information out.
Without a good debugger to let me step through the game code, most of their complexities remain a mystery.

### Textures
Some, maybe all, 64x64 textures that use Block-Compressed formats (DXT1, DXT3, DXT5, DXN) are apparently bugged up. Right click on the Texture Node in the Tree View and Select Recrop to attempt to fix this.
The Recrop option re-reads the as if it were twice as big (128x128), and then crops it down to 64x64. Keep in mind that this may crash if the texture is near the end of the file. 

Also, there's a bug in the implementation of the de-tiling code, some textures that are less than 128px high may cause the program to attempt to access violate and crash. I've got a hack in place to stop that, but it means the texture won't read properly. I haven't had a chance to go through and reverse the de-tiling code from the Unbundler or study the de-tiling code used([See License Info](#license-credits)), so I'm not sure what the issue is, and if it's related to the 64x64 texture issue above.

You can also dump the texture to a .xpr file for use with the Unbundler from the official SDK. for legal reasons I cannot distribute those with this application or source. Handy with all the texture bugs.



## Menus:
**File**
 - **Load:** Loads a .mdl or .lvl file.
 - **Exit:** Exits the program.
**Options**
- **Log Window:** Shows a log window at the bottom of the program. This is mostly for errors or other things. Probably not useful. 

## Mini-Tutorial:
Right now, there's some work you have to do to get models viewing properly.

Load up a .lvl or .mdl file.
The tree view will have Rendergraph and Texture Nodes.
Expand a Rendergraph node, and you'll see Vertex Batch, Triangle Strips and Material sub nodes.

 - Clicking on a RenderGraph Node will render every Triangle Strip
   inside it.
 --  Clicking on a Vertex Batch will render every Triangle Strip that uses
   that Vertex Batch.
  -- Clicking on a Triangle Strip will render that Triangle Strip.
  -- Clicking on a Material will render every Triangle Strip that uses
   that Material.
   - Clicking on a Tetxure will display the Texture. ([See Developer Notes about Textures](#textures))

Vertex Batch attributes are set to defaults, assuming 16bit texture coordinates, followed by 16bit vertex position coordinates. If a model isn't rendering properly, you'll have to go in and manually edit the attributes ([See Developer Notes about Vertex Attributes](#vertex-attributes))

|Attribute  | Explanation |
|--|--|
|**Position Attributes**:  |  |
|32Bit| Some Vertex Batches use 32bit floating point numbers for the position values. RareView will read positions from 32bit float when set to True, otherwise it will read them from 16bit half-float values.|
|Position Offset|Position inside the vertex buffer to read the Position attribute from|
|**Texture Coordinate Options**| |
|32Bit| Like with the position attribute, the vertex batch may have the Texture Coordinates use 32bit floating point numbers. Set to True to read them as such.|
|Flip| Flip Texture V Coordinate. You probably won't need to use this.|
|Has TexCoords|VertexBatches of Type: vPos usually don't have texture coordinates. Setting this to False will set the Texture Coordinates of all Verteices to 0.0, 0.0|
|Scale|Scaling value for the Texture Coordinates. Usually around 4096. Try powers of 2 like 1024, 2048, 4096, 8192, 16384, etc.|
|Texture Coordinate Offset| Same with the Position Offset|

There is currently no run-time checking to make sure the values set for attribute offsets fall within the correct bounds. Start low, work your up. try not to go higher than 24 for the Position Offset,
If the program lags after setting a value, then it is probably reading invalid data, NaN(Not a Number), etc, so try another value. 

You can use the UV view to view the UVs for current Triangle Strips. Keep in mind that the values may not always fall within the texture bounds. 

Once you've got everything looking not like a jumbled mess, you can start assigning Textures to Materials.

Currently, Materials have only 1 parameter, TextureID. ([See the Developers notes on Materials](#materials))

You can set the TextureID to -1 to set it to the UV Grid debug texture that's been included, otherwise, the TextureIDs correspond to the Texture node names, ie "Texturefile_0" is TextureID 0, Texturefile_32 is TextureID 32, etc.

Once you get everything looking pretty, right click on the main file node and select Export. This will open a Save File Dialog where you can select the location to save to. I would recommend that you create a new folder to avoid naming conflicts, since a file along the lines of "DEADBEEF_texturefile_23.png" is just ridiculous. I might create an option for that later. or you can.

## License-Credits:
Texture De-Tiling and Decoding is pretty much copy-pasta'd from [GTA IV Xbox 360 Texture Editor by "Pimpin Tyler and Anthony"](http://forum.xentax.com/blog/?p=302) ([See Developers notes on Textures](#textures)).

As the source for that program was not released with a license, and I have not yet had a chance to attempt to contact the author(s) of the original software about it, this is currently unlicensed. Any requests by said authors to remove their code from this app will be complied with. I really don't think it'll come to that, but  I really don't want to get sued over a hobby.



