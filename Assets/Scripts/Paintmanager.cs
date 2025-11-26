using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PaintUIManager : MonoBehaviour
{
    [Header("Referencias")]
    public PaintSimulator paintSimulator;
    
    [Header("UI - Herramientas")]
    public Button pencilButton;
    public Button brushButton;
    public Button eraserButton;
    public Button fillButton;
    public Button lineButton;
    public Button rectangleButton;
    public Button circleButton;
    public Button eyedropperButton;
    
    [Header("UI - Controles")]
    public Button saveButton;
    public Button clearButton;
    public Slider brushSizeSlider;
    public TextMeshProUGUI brushSizeText;
    public TextMeshProUGUI currentToolText;
    public Image colorPreview;
    
    [Header("UI - Paleta de Colores")]
    public GameObject colorButtonPrefab;
    public Transform colorPaletteContainer;
    
    private Button[] toolButtons;
    private int currentToolIndex = 0;
    private bool updatingSlider = false;
    
    void Start()
    {
        if (paintSimulator == null)
        {
            Debug.LogError("PaintSimulator no está asignado en PaintUIManager!");
            return;
        }
        
        SetupToolButtons();
        SetupColorPalette();
        SetupControls();
        UpdateUIDisplay();
    }
    
    void Update()
    {
        UpdateUIDisplay();
    }
    
    void SetupToolButtons()
    {
        toolButtons = new Button[]
        {
            pencilButton, brushButton, eraserButton, fillButton,
            lineButton, rectangleButton, circleButton, eyedropperButton
        };
        
        if (pencilButton) pencilButton.onClick.AddListener(() => SelectTool(0));
        if (brushButton) brushButton.onClick.AddListener(() => SelectTool(1));
        if (eraserButton) eraserButton.onClick.AddListener(() => SelectTool(2));
        if (fillButton) fillButton.onClick.AddListener(() => SelectTool(3));
        if (lineButton) lineButton.onClick.AddListener(() => SelectTool(4));
        if (rectangleButton) rectangleButton.onClick.AddListener(() => SelectTool(5));
        if (circleButton) circleButton.onClick.AddListener(() => SelectTool(6));
        if (eyedropperButton) eyedropperButton.onClick.AddListener(() => SelectTool(7));
    }
    
    void SetupColorPalette()
    {
        if (colorButtonPrefab == null || colorPaletteContainer == null || paintSimulator == null)
            return;
        
        Color[] colors = paintSimulator.GetColorPalette();
        
        for (int i = 0; i < colors.Length; i++)
        {
            int index = i;
            GameObject colorBtn = Instantiate(colorButtonPrefab, colorPaletteContainer);
            Button btn = colorBtn.GetComponent<Button>();
            Image img = colorBtn.GetComponent<Image>();
            
            if (img) img.color = colors[i];
            if (btn) btn.onClick.AddListener(() => paintSimulator.SetColor(index));
        }
    }
    
    void SetupControls()
    {
        // Botón de guardar
        if (saveButton)
            saveButton.onClick.AddListener(() => paintSimulator.SaveImage());
        
        // Botón de limpiar
        if (clearButton)
            clearButton.onClick.AddListener(() => paintSimulator.ClearCanvas());
        
        // Slider de tamaño de pincel
        if (brushSizeSlider)
        {
            brushSizeSlider.minValue = 1;
            brushSizeSlider.maxValue = 50;
            brushSizeSlider.wholeNumbers = true;
            brushSizeSlider.value = paintSimulator.GetBrushSize();
            brushSizeSlider.onValueChanged.AddListener(OnBrushSizeChanged);
        }
        
        // Sincronizar el texto inicial
        if (brushSizeText)
            brushSizeText.text = paintSimulator.GetBrushSize().ToString();
    }
    
    void OnBrushSizeChanged(float value)
    {
        if (paintSimulator != null && !updatingSlider)
        {
            int intValue = (int)value;
            paintSimulator.SetBrushSize(intValue);
            
            if (brushSizeText)
                brushSizeText.text = intValue.ToString();
                
            Debug.Log("Brush size cambiado a: " + intValue);
        }
    }
    
    void SelectTool(int toolIndex)
    {
        currentToolIndex = toolIndex;
        paintSimulator.SetTool(toolIndex);
        HighlightSelectedTool(toolIndex);
    }
    
    void HighlightSelectedTool(int selectedIndex)
    {
        for (int i = 0; i < toolButtons.Length; i++)
        {
            if (toolButtons[i] != null)
            {
                ColorBlock colors = toolButtons[i].colors;
                colors.normalColor = (i == selectedIndex) ? new Color(0.7f, 0.9f, 1f) : Color.white;
                toolButtons[i].colors = colors;
            }
        }
    }
    
    void UpdateUIDisplay()
    {
        if (paintSimulator == null) return;
        
        // Actualizar texto de herramienta actual
        if (currentToolText)
            currentToolText.text = "Herramienta: " + paintSimulator.GetCurrentToolName();
        
        // Actualizar preview de color
        if (colorPreview)
            colorPreview.color = paintSimulator.GetCurrentColor();
        
        // Actualizar slider solo si cambió desde el teclado
        if (brushSizeSlider)
        {
            int currentSize = paintSimulator.GetBrushSize();
            int sliderSize = (int)brushSizeSlider.value;
            
            // Solo actualizar si es diferente y no estamos arrastrando
            if (currentSize != sliderSize && !Input.GetMouseButton(0))
            {
                updatingSlider = true;
                brushSizeSlider.value = currentSize;
                if (brushSizeText)
                    brushSizeText.text = currentSize.ToString();
                updatingSlider = false;
            }
        }
    }
    
    bool IsSliderBeingDragged()
    {
        if (brushSizeSlider == null) return false;
        
        return Input.GetMouseButton(0) && 
               UnityEngine.EventSystems.EventSystem.current != null &&
               UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == brushSizeSlider.gameObject;
    }
}