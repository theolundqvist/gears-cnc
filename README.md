# Gears to CNC

A Unity-based tool for generating G-Code to cut precisely spaced holes in CNC-machined plates for gear assemblies. Originally built for use in educational gear puzzles, the tool features automatic route optimization to minimize total tool travel distance.

## ✦ Key Features
- **G-Code Generation**: Automatically generates CNC-compatible G-Code for gear hole layouts.
- **Route Optimization**: Integrates a modified Ant Colony Optimization algorithm to reduce toolpath length.
- **Educational Use**: Designed for hands-on learning in preschool and primary schools, with [Komtek Halmstad](https://www.halmstad.se/barnochutbildning/teknikochentreprenorsskolankomtek.n2636.html).
- **Built in Unity**: Includes an intuitive interface for visualizing gear placement and toolpaths.

## ✦ Example Use
Gears were laser-cut from plexiglass and used in physical puzzle activities in schools. The route optimization significantly reduced the cutting distance — from 158 cm down to 64 cm.

| Before Optimization | After Optimization |
|---------------------|--------------------|
| ![gear5](https://raw.githubusercontent.com/theolundqvist/images_for_readme/main/gear5.png) | ![gear4](https://raw.githubusercontent.com/theolundqvist/images_for_readme/main/gear4.png) |

## ✦ Screenshots

Main view and UI:

![gear2](https://raw.githubusercontent.com/theolundqvist/images_for_readme/main/gear2.png)

Additional views:

<p float="left">
  <img src="https://raw.githubusercontent.com/theolundqvist/images_for_readme/main/gear1.png" height="240" />
  <img src="https://raw.githubusercontent.com/theolundqvist/images_for_readme/main/gear3.png" height="240" />
</p>

## ✦ Algorithm Details
The route optimization is based on [Dr. James McCaffrey’s Ant Colony Optimization algorithm](https://docs.microsoft.com/en-us/archive/msdn-magazine/2012/february/test-run-ant-colony-optimization), adapted and extended for CNC use cases.

## ✦ Download
- Download the runnable Windows executable here:  
  [📁 Dropbox – gears_cnc](https://www.dropbox.com/sh/ssqr4q3f0iic6mi/AACE0E65xaUV96X30Gs2yC1Ka?dl=0)
