____________________________________________________________________________________________________  




EN — Cartoon Shader - URP  



____________________________________________________________________________________________________  




Compatible version: Unity 6000.0.33f1  



_____ Installation _____  

1. Create a material and assign the Cartoon Shader.  
2. Apply the material to a 3D model.  
3. The shader uses the main light in the scene to generate shadows and highlights.  



_____ Available Parameters _____  

Main parameters:  
- Base color  
- Shadows color  
- Shadows contrast  
- Shadows size  

Outline:  
- Outline size  
- Outline color  

Highlight:  
- Highlight size  
- Glow size  
- Highlight color  
- Highlight intensity  



_____ Editing the Gradients _____  

The gradients defining the shadows and highlights cannot be modified in the material editor. They are stored as images in the Shader Graph (default format: Photoshop).  

To modify them:  
1. Open the corresponding Photoshop file.  
2. Edit the gradient as needed.  
3. Save and replace the image used in the Shader Graph.  



_____ Support _____  

This shader is provided as is. If you encounter any bugs or want a variant, feel free to contact me.  