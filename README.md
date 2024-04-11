# SkiaSharpVisualizer
A Visual Studio debugger extension for viewing [SkiaSharp](https://github.com/mono/SkiaSharp) bitmaps and images.

## Building
Designed for Visual Studio 2022, simply update to at least 17.9 and install the "Visual Studio extension development" workload. There are no external requirements beyond the SkiaSharp nuget package and Visual Studio SDK. After building the solution, run the generated .vsix file to install.

## How to Use
The extension adds a new UI item to view SkiaSharp SKBitmap, SKImage, and SKSurface objects.
![image](https://github.com/MapLarge/SkiaSharpVisualizer/assets/38544371/932c0544-dea0-445a-a052-e971878af182)

When viewing, you will see a tool window containing a graphical preview of the image.
![image](https://github.com/MapLarge/SkiaSharpVisualizer/assets/38544371/e7c97551-7767-4e4a-8f87-c66feec7dcdd)

The stretch option will make the image fill the entire dialog space.
![image](https://github.com/MapLarge/SkiaSharpVisualizer/assets/38544371/42625fa9-4e31-4751-b442-c9ed632e4ee8)

The bordered option will add an indicator border around the image so you can figure out the boundary of an image with transparency.
![image](https://github.com/MapLarge/SkiaSharpVisualizer/assets/38544371/e4ce3b8a-a81b-4f38-9423-3313cc6e60d4)
