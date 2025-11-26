using UnityEngine;
using System.IO;
using System;

public class PaintSimulator : MonoBehaviour
{
    [Header("Configuración del Canvas")]
    public int canvasWidth = 1200;
    public int canvasHeight = 1000;
    public Color backgroundColor = Color.white;
    
    [Header("Herramientas")]
    public int brushSize = 5;
    public Color currentColor = Color.black;
    
    [Header("Límites de Dibujo (Opcional)")]
    public bool useDrawingBounds = true;
    public float drawingBoundsMargin = 100f;
    
    private Texture2D canvas;
    private Texture2D backupCanvas;
    private bool isDrawing = false;
    private Vector2 lastDrawPos;
    
    private enum Tool { Pencil, Brush, Eraser, Fill, Line, Rectangle, Circle, Eyedropper }
    private Tool currentTool = Tool.Pencil;
    
    private Vector2 lineStartPos;
    private bool isDrawingShape = false;
    
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;
    
    // Optimización: Buffer para acumular cambios
    private bool needsApply = false;
    private float lastApplyTime = 0f;
    private const float applyInterval = 0.05f; // Aplicar cada 50ms en lugar de cada frame
    
    private Color[] colorPalette = new Color[]
    {
        Color.black, new Color(0.5f, 0.5f, 0.5f), new Color(0.5f, 0, 0), new Color(0.5f, 0.25f, 0),
        new Color(0.5f, 0.5f, 0), new Color(0, 0.5f, 0), new Color(0, 0.5f, 0.5f), new Color(0, 0, 0.5f),
        new Color(0.25f, 0, 0.5f), new Color(0.5f, 0, 0.5f),
        Color.white, new Color(0.75f, 0.75f, 0.75f), Color.red, new Color(1, 0.5f, 0),
        Color.yellow, Color.green, Color.cyan, Color.blue,
        new Color(0.5f, 0, 1), Color.magenta,
        new Color(1, 0.75f, 0.8f), new Color(0.6f, 0.4f, 0.2f), new Color(1, 0.9f, 0.6f), new Color(0.8f, 1, 0.8f),
        new Color(0.6f, 1, 1), new Color(0.8f, 0.8f, 1), new Color(1, 0.8f, 1), new Color(1, 0.6f, 0.6f)
    };
    
    void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (mainCamera == null)
        {
            Debug.LogError("¡No se encontró la Main Camera!");
            return;
        }
        
        InitializeCanvas();
    }
    
    void InitializeCanvas()
    {
        canvas = new Texture2D(canvasWidth, canvasHeight);
        backupCanvas = new Texture2D(canvasWidth, canvasHeight);
        
        Color[] pixels = new Color[canvasWidth * canvasHeight];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = backgroundColor;
        }
        canvas.SetPixels(pixels);
        canvas.Apply();
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = Sprite.Create(
                canvas,
                new Rect(0, 0, canvasWidth, canvasHeight),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }
    }
    
    void Update()
    {
        if (mainCamera == null) return;
        HandleInput();
        
        // Optimización: Aplicar cambios solo periódicamente
        if (needsApply && Time.time - lastApplyTime > applyInterval)
        {
            canvas.Apply();
            needsApply = false;
            lastApplyTime = Time.time;
        }
    }
    
    void HandleInput()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        
        if (mouseScreenPos.x < 0 || mouseScreenPos.x > Screen.width || 
            mouseScreenPos.y < 0 || mouseScreenPos.y > Screen.height)
        {
            isDrawing = false;
            return;
        }
        
        if (useDrawingBounds)
        {
            if (mouseScreenPos.x < drawingBoundsMargin || 
                mouseScreenPos.x > Screen.width - drawingBoundsMargin ||
                mouseScreenPos.y < drawingBoundsMargin || 
                mouseScreenPos.y > Screen.height - drawingBoundsMargin)
            {
                isDrawing = false;
                return;
            }
        }
        
        Vector2? mousePosNullable = GetCanvasMousePosition();
        if (!mousePosNullable.HasValue)
        {
            isDrawing = false;
            return;
        }
        
        Vector2 mousePos = mousePosNullable.Value;
        
        if (mousePos.x < 0 || mousePos.x >= canvasWidth || mousePos.y < 0 || mousePos.y >= canvasHeight)
        {
            isDrawing = false;
            return;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            BackupCanvas();
            
            if (currentTool == Tool.Line || currentTool == Tool.Rectangle || currentTool == Tool.Circle)
            {
                lineStartPos = mousePos;
                isDrawingShape = true;
            }
            else if (currentTool == Tool.Fill)
            {
                FloodFill((int)mousePos.x, (int)mousePos.y, currentColor);
                canvas.Apply(); // Aplicar inmediatamente para fill
            }
            else if (currentTool == Tool.Eyedropper)
            {
                currentColor = canvas.GetPixel((int)mousePos.x, (int)mousePos.y);
            }
            else
            {
                isDrawing = true;
                Draw(mousePos, false); // No aplicar inmediatamente
            }
            
            lastDrawPos = mousePos;
        }
        
        if (Input.GetMouseButton(0) && isDrawing)
        {
            DrawLine(lastDrawPos, mousePos, false); // No aplicar inmediatamente
            lastDrawPos = mousePos;
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            if (isDrawingShape)
            {
                if (currentTool == Tool.Line)
                    DrawShapeLine(lineStartPos, mousePos);
                else if (currentTool == Tool.Rectangle)
                    DrawRectangle(lineStartPos, mousePos);
                else if (currentTool == Tool.Circle)
                    DrawCircle(lineStartPos, mousePos);
                
                isDrawingShape = false;
                canvas.Apply(); // Aplicar al terminar la forma
            }
            
            if (isDrawing)
            {
                canvas.Apply(); // Aplicar al soltar el botón
                needsApply = false;
            }
            
            isDrawing = false;
        }
        
        // Preview de formas mientras se dibuja
        if (isDrawingShape && Input.GetMouseButton(0))
        {
            RestoreCanvas();
            if (currentTool == Tool.Line)
                DrawShapeLine(lineStartPos, mousePos);
            else if (currentTool == Tool.Rectangle)
                DrawRectangle(lineStartPos, mousePos);
            else if (currentTool == Tool.Circle)
                DrawCircle(lineStartPos, mousePos);
            canvas.Apply();
        }
        
        // Teclas de atajo
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentTool = Tool.Pencil;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentTool = Tool.Brush;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentTool = Tool.Eraser;
        if (Input.GetKeyDown(KeyCode.Alpha4)) currentTool = Tool.Fill;
        if (Input.GetKeyDown(KeyCode.Alpha5)) currentTool = Tool.Line;
        if (Input.GetKeyDown(KeyCode.Alpha6)) currentTool = Tool.Rectangle;
        if (Input.GetKeyDown(KeyCode.Alpha7)) currentTool = Tool.Circle;
        if (Input.GetKeyDown(KeyCode.Alpha8)) currentTool = Tool.Eyedropper;
        
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Equals))
            brushSize = Mathf.Min(brushSize + 2, 50);
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            brushSize = Mathf.Max(brushSize - 2, 1);
        
        if (Input.GetKeyDown(KeyCode.Z) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            RestoreCanvas();
        
        if (Input.GetKeyDown(KeyCode.C) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            ClearCanvas();
        
        if (Input.GetKeyDown(KeyCode.S) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            SaveImage();
    }
    
    Vector2? GetCanvasMousePosition()
    {
        try
        {
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
            
            if (spriteRenderer == null || spriteRenderer.sprite == null)
                return null;
            
            Bounds bounds = spriteRenderer.bounds;
            float normalizedX = (worldPos.x - bounds.min.x) / bounds.size.x;
            float normalizedY = (worldPos.y - bounds.min.y) / bounds.size.y;
            
            float x = normalizedX * canvasWidth;
            float y = normalizedY * canvasHeight;
            
            return new Vector2(x, y);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error al obtener posición del mouse: " + e.Message);
            return null;
        }
    }
    
    void Draw(Vector2 pos, bool apply = true)
    {
        int size = currentTool == Tool.Eraser ? brushSize * 2 : brushSize;
        Color color = currentTool == Tool.Eraser ? backgroundColor : currentColor;
        
        // Debug para verificar el tamaño
        // Debug.Log("Drawing with size: " + size + " (brushSize: " + brushSize + ")");
        
        for (int x = -size; x <= size; x++)
        {
            for (int y = -size; y <= size; y++)
            {
                if (currentTool == Tool.Brush || currentTool == Tool.Eraser)
                {
                    // Pincel circular suave
                    if (x * x + y * y <= size * size)
                    {
                        int px = (int)pos.x + x;
                        int py = (int)pos.y + y;
                        if (px >= 0 && px < canvasWidth && py >= 0 && py < canvasHeight)
                            canvas.SetPixel(px, py, color);
                    }
                }
                else if (currentTool == Tool.Pencil)
                {
                    // Lápiz usa el brushSize también
                    if (x * x + y * y <= (brushSize * brushSize) / 4)
                    {
                        int px = (int)pos.x + x;
                        int py = (int)pos.y + y;
                        if (px >= 0 && px < canvasWidth && py >= 0 && py < canvasHeight)
                            canvas.SetPixel(px, py, color);
                    }
                }
            }
        }
        
        if (apply)
            canvas.Apply();
        else
            needsApply = true;
    }
    
    void DrawLine(Vector2 start, Vector2 end, bool apply = true)
    {
        int x0 = (int)start.x;
        int y0 = (int)start.y;
        int x1 = (int)end.x;
        int y1 = (int)end.y;
        
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        
        while (true)
        {
            Draw(new Vector2(x0, y0), false);
            
            if (x0 == x1 && y0 == y1) break;
            
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
        
        if (apply)
            canvas.Apply();
        else
            needsApply = true;
    }
    
    void DrawShapeLine(Vector2 start, Vector2 end)
    {
        int x0 = (int)start.x;
        int y0 = (int)start.y;
        int x1 = (int)end.x;
        int y1 = (int)end.y;
        
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        
        while (true)
        {
            // Dibujar punto con grosor del pincel
            for (int i = -brushSize; i <= brushSize; i++)
            {
                for (int j = -brushSize; j <= brushSize; j++)
                {
                    if (i * i + j * j <= brushSize * brushSize)
                    {
                        int px = x0 + i;
                        int py = y0 + j;
                        if (px >= 0 && px < canvasWidth && py >= 0 && py < canvasHeight)
                            canvas.SetPixel(px, py, currentColor);
                    }
                }
            }
            
            if (x0 == x1 && y0 == y1) break;
            
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
    
    void DrawRectangle(Vector2 start, Vector2 end)
    {
        int x1 = (int)Mathf.Min(start.x, end.x);
        int x2 = (int)Mathf.Max(start.x, end.x);
        int y1 = (int)Mathf.Min(start.y, end.y);
        int y2 = (int)Mathf.Max(start.y, end.y);
        
        // Líneas horizontales
        for (int x = x1; x <= x2; x++)
        {
            // Línea superior
            for (int i = -brushSize; i <= brushSize; i++)
            {
                for (int j = -brushSize; j <= brushSize; j++)
                {
                    if (i * i + j * j <= brushSize * brushSize)
                    {
                        int px = x + i;
                        int py = y1 + j;
                        if (px >= 0 && px < canvasWidth && py >= 0 && py < canvasHeight)
                            canvas.SetPixel(px, py, currentColor);
                    }
                }
            }
            
            // Línea inferior
            for (int i = -brushSize; i <= brushSize; i++)
            {
                for (int j = -brushSize; j <= brushSize; j++)
                {
                    if (i * i + j * j <= brushSize * brushSize)
                    {
                        int px = x + i;
                        int py = y2 + j;
                        if (px >= 0 && px < canvasWidth && py >= 0 && py < canvasHeight)
                            canvas.SetPixel(px, py, currentColor);
                    }
                }
            }
        }
        
        // Líneas verticales
        for (int y = y1; y <= y2; y++)
        {
            // Línea izquierda
            for (int i = -brushSize; i <= brushSize; i++)
            {
                for (int j = -brushSize; j <= brushSize; j++)
                {
                    if (i * i + j * j <= brushSize * brushSize)
                    {
                        int px = x1 + i;
                        int py = y + j;
                        if (px >= 0 && px < canvasWidth && py >= 0 && py < canvasHeight)
                            canvas.SetPixel(px, py, currentColor);
                    }
                }
            }
            
            // Línea derecha
            for (int i = -brushSize; i <= brushSize; i++)
            {
                for (int j = -brushSize; j <= brushSize; j++)
                {
                    if (i * i + j * j <= brushSize * brushSize)
                    {
                        int px = x2 + i;
                        int py = y + j;
                        if (px >= 0 && px < canvasWidth && py >= 0 && py < canvasHeight)
                            canvas.SetPixel(px, py, currentColor);
                    }
                }
            }
        }
    }
    
    void DrawCircle(Vector2 center, Vector2 edge)
    {
        int radius = (int)Vector2.Distance(center, edge);
        int cx = (int)center.x;
        int cy = (int)center.y;
        
        // Dibujar círculo con más puntos para mejor calidad
        int numPoints = radius * 4; // Más puntos = círculo más suave
        if (numPoints < 360) numPoints = 360;
        
        for (int i = 0; i < numPoints; i++)
        {
            float angle = (i * 360f / numPoints) * Mathf.Deg2Rad;
            int x = cx + (int)(radius * Mathf.Cos(angle));
            int y = cy + (int)(radius * Mathf.Sin(angle));
            
            // Dibujar punto con grosor del pincel
            for (int dx = -brushSize; dx <= brushSize; dx++)
            {
                for (int dy = -brushSize; dy <= brushSize; dy++)
                {
                    if (dx * dx + dy * dy <= brushSize * brushSize)
                    {
                        int px = x + dx;
                        int py = y + dy;
                        if (px >= 0 && px < canvasWidth && py >= 0 && py < canvasHeight)
                            canvas.SetPixel(px, py, currentColor);
                    }
                }
            }
        }
    }
    
    void FloodFill(int x, int y, Color fillColor)
    {
        Color targetColor = canvas.GetPixel(x, y);
        
        if (ColorEquals(targetColor, fillColor)) return;
        
        System.Collections.Generic.Queue<Vector2Int> queue = new System.Collections.Generic.Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(x, y));
        
        int fillCount = 0;
        int maxFill = 500000;
        
        while (queue.Count > 0 && fillCount < maxFill)
        {
            Vector2Int pos = queue.Dequeue();
            
            if (pos.x < 0 || pos.x >= canvasWidth || pos.y < 0 || pos.y >= canvasHeight)
                continue;
            
            Color pixelColor = canvas.GetPixel(pos.x, pos.y);
            
            if (ColorEquals(pixelColor, targetColor))
            {
                canvas.SetPixel(pos.x, pos.y, fillColor);
                fillCount++;
                
                queue.Enqueue(new Vector2Int(pos.x + 1, pos.y));
                queue.Enqueue(new Vector2Int(pos.x - 1, pos.y));
                queue.Enqueue(new Vector2Int(pos.x, pos.y + 1));
                queue.Enqueue(new Vector2Int(pos.x, pos.y - 1));
            }
        }
    }
    
    bool ColorEquals(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.01f && 
               Mathf.Abs(a.g - b.g) < 0.01f && 
               Mathf.Abs(a.b - b.b) < 0.01f && 
               Mathf.Abs(a.a - b.a) < 0.01f;
    }
    
    void BackupCanvas()
    {
        backupCanvas.SetPixels(canvas.GetPixels());
        backupCanvas.Apply();
    }
    
    void RestoreCanvas()
    {
        canvas.SetPixels(backupCanvas.GetPixels());
        canvas.Apply();
    }
    
    public void ClearCanvas()
    {
        BackupCanvas();
        Color[] pixels = new Color[canvasWidth * canvasHeight];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = backgroundColor;
        }
        canvas.SetPixels(pixels);
        canvas.Apply();
    }
    
    public void SaveImage()
    {
        byte[] bytes = canvas.EncodeToPNG();
        string filename = "PaintDrawing_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string path = Application.persistentDataPath + "/" + filename;
        File.WriteAllBytes(path, bytes);
        Debug.Log("Imagen guardada en: " + path);
    }
    
    public void SetColor(int index)
    {
        if (index >= 0 && index < colorPalette.Length)
            currentColor = colorPalette[index];
    }
    
    public void SetTool(int toolIndex)
    {
        currentTool = (Tool)toolIndex;
    }
    
    public void SetBrushSize(int size)
    {
        brushSize = Mathf.Clamp(size, 1, 50);
        Debug.Log("SetBrushSize llamado: " + brushSize);
    }
    
    public string GetCurrentToolName() => currentTool.ToString();
    public int GetBrushSize() => brushSize;
    public Color GetCurrentColor() => currentColor;
    public Color[] GetColorPalette() => colorPalette;
}