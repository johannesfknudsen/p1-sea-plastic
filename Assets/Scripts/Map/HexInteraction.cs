using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static HexGrid;

public class HexInteraction : MonoBehaviour
{
    public static HexInteraction instance;

    public int touchedRegion;

    public delegate void CellTypePlacedHandler();
    public event CellTypePlacedHandler OnCellTypePlaced;


    public terrainType[] Structures = new terrainType[]
    {
        terrainType.recycler,
        terrainType.incinerator,
        terrainType.landfill,
        terrainType.boatCleaner,
        terrainType.riverBarricade
    };

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(gameObject);
        }
    }
    [HideInInspector]
    public bool uiActive = false;

    [SerializeField]
    private bool canPlace = false;

    [SerializeField]
    private terrainType nextStructure;

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && !uiActive)
        {
            Debug.Log("Left click");
            HandleInput();
        }
    }

    void HandleInput()
    {
        int layerMask = 1 << 8;
        layerMask = ~layerMask;

        Ray inputRay = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        Debug.Log("Mouse position " + Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()));

        if (Physics.Raycast(inputRay, out hit, Mathf.Infinity, layerMask))
        {
            Debug.Log("Hit at " + hit.point);
            Debug.DrawRay(inputRay.origin, inputRay.direction * hit.distance, Color.yellow);
            TouchCell(hit.point);
        }
        else
        {
            Debug.DrawRay(inputRay.origin, inputRay.direction * 1000, Color.white);
            Debug.Log("No hit");
        }
    }

    void TouchCell(Vector3 position)
    {
        Debug.Log("touched position " + position + " -> " + transform.InverseTransformPoint(position));
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int coordinateZ = coordinates.Z * -1;
        int index = coordinates.X + (coordinateZ * HexGrid.instance.width) + coordinates.Z / 2; /*  + (coordinates.Z * -1 / 2) */
        // Debug.Log("touched index " + "(" + "X: " + (coordinateZ) + "*" + "W: " + width + " -> " + "X: " + coordinates.X + "+" + "Z*W:" + (coordinateZ * width) + "+ Z/2: " + (coordinateZ / 2) + " -> " + "i: " + index);
        Debug.Log("touched at " + coordinates.ToString() + " -> " + index);
        if (index >= 0 && index < HexGrid.instance.cells.Length)
        {
            HexCell cell = HexGrid.instance.cells[index];
            touchedRegion = cell.region;
            Debug.Log("Touched region " + touchedRegion);

            /// Will return true if the touched cell is not already a structure
            /// and the next structure is a structure.
            /// Also checks if the cell is not a mountain, snow or artic.
            if (canPlace
                && !Structures.Contains(cell.terrainType)
                && Structures.Contains(nextStructure)
                && !(cell.terrainType == terrainType.mountain
                        || cell.terrainType == terrainType.snow
                        || cell.terrainType == terrainType.artic))
            {
                // TODO: Implement river barricade
                /// Will return true if the touched cell is a body of water.
                if (cell.terrainType == terrainType.contaminatedWater || cell.terrainType == terrainType.water)
                {
                    /// Will return true if the next structure is a boat cleaner.
                    if (nextStructure == terrainType.boatCleaner)
                    {
                        /// Remove the cell from the contaminated cells list.
                        if (cell.terrainType == terrainType.contaminatedWater)
                        {
                            WaterContamination.Instance.contaminatedCells.Remove(cell);
                        }
                        cell.SetCellType(nextStructure);
                        OnCellTypePlaced?.Invoke();
                    }

                }
                /// Else, if the touched cell is not a body of water,
                /// and the next structure is not a boat cleaner,
                /// place the structure.
                else if (nextStructure != terrainType.boatCleaner)
                {
                    cell.SetCellType(nextStructure);
                    OnCellTypePlaced?.Invoke();
                }
            }

            Debug.Log("Touched cell position " + cell.transform.position);

            for (int i = 0; i < cell.neighbors.Length; i++)
            {
                HexCell neighbor = cell.neighbors[i];
                if (neighbor != null)
                {
                    // Debug.Log("Neighbor " + ((HexDirection)i).ToString() + neighbor.coordinates.ToString());

                    // Debug directions
                    /* TMP_Text neighborlabel = Instantiate<TMP_Text>(cellLabelPrefab);
                    neighborlabel.rectTransform.SetParent(gridCanvas.transform, false);
                    neighborlabel.rectTransform.anchoredPosition =
                        new Vector2(neighbor.position.x, neighbor.position.z);
                    neighborlabel.fontSize = 5;
                    neighborlabel.color = Color.yellow;
                    neighborlabel.text = ((HexDirection)i).ToString() + "\n" + index.ToString(); */
                }
            }
        }
    }

    public void PlaceCellType(terrainType type)
    {
        nextStructure = type;
        Debug.Log("Placing " + type);
        // hexSpawner.SpawnPrefab(type);
    }
}