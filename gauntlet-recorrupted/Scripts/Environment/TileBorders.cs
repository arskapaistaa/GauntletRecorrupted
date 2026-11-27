using Godot;
using System;

public partial class TileBorders : TileMapLayer
{
	[Export] private DiggableTiles _tileset;
	[Export] private TileMaterial _tileMaterial;
	public enum TileMaterial
	{
		Stone,
		Flesh
	}
	private const string MaterialDataName = "Material";
	private int TileID;
	public static readonly Vector2I BlockRight = new Vector2I(0, 0);
    public static readonly Vector2I BlockLeft = new Vector2I(1, 0);
    public static readonly Vector2I BlockDown = new Vector2I(2, 0);
    public static readonly Vector2I BlockUp = new Vector2I(3, 0);

    public static readonly Vector2I BlockUpLeft = new Vector2I(0, 1);
    public static readonly Vector2I BlockUpRight = new Vector2I(1, 1);
    public static readonly Vector2I BlockUpDownLeft = new Vector2I(2, 1);
    public static readonly Vector2I BlockUpRightDown = new Vector2I(3, 1);

    public static readonly Vector2I BlockDownLeft = new Vector2I(0, 2);
    public static readonly Vector2I BlockRightDown = new Vector2I(1, 2);
    public static readonly Vector2I BlockUpRightLeft = new Vector2I(2, 2);
    public static readonly Vector2I BlockAll = new Vector2I(2, 3);

    public static readonly Vector2I BlockUpDown = new Vector2I(0, 3);
    public static readonly Vector2I BlockRightLeft = new Vector2I(1, 3);
    public static readonly Vector2I BlockRightDownLeft = new Vector2I(3, 2);
	public override void _Ready()
	{
		if (_tileMaterial == TileMaterial.Stone)
			TileID = 0;
		else if (_tileMaterial == TileMaterial.Flesh)
			TileID = 1;
		_tileset.TileDestroyed += UpdateTileBorders;
		SetupTileBorders();
	}

	public override void _ExitTree()
	{
		_tileset.TileDestroyed -= UpdateTileBorders;
	}

	private void SetupTileBorders()
	{
		Godot.Collections.Array<Vector2I> usedCells = _tileset.GetUsedCells();

		foreach (Vector2I cell in usedCells)
		{
			if (!IsWantedMaterial(cell))
				continue;
			UpdateTileBorders(cell);
		}
	}

	public void UpdateTileBorders(Vector2I coords)
	{
		int startCol = coords.X - 1;
		int startRow = coords.Y - 1;
		int endCol = coords.X + 1;
		int endRow = coords.Y + 1;
		for (int row = startRow; row <= endRow; row++)
		{
			for (int col = startCol; col <= endCol; col++)
			{
				Vector2I cellCoords = new Vector2I(col, row);
				if (!CanHaveBorder(cellCoords))
					continue;
				
				bool hasBlockRight = IsWantedMaterial(new Vector2I(col + 1, row));
				bool hasBlockLeft = IsWantedMaterial(new Vector2I(col - 1, row));
				bool hasBlockDown = IsWantedMaterial(new Vector2I(col, row + 1));
				bool hasBlockUp = IsWantedMaterial(new Vector2I(col, row - 1));

				if (hasBlockRight && !hasBlockLeft && !hasBlockDown && !hasBlockUp)
					PlaceTile(cellCoords, BlockRight);
				else if (!hasBlockRight && hasBlockLeft && !hasBlockDown && !hasBlockUp)
					PlaceTile(cellCoords, BlockLeft);
				else if (!hasBlockRight && !hasBlockLeft && hasBlockDown && !hasBlockUp)
					PlaceTile(cellCoords, BlockDown);
				else if (!hasBlockRight && !hasBlockLeft && !hasBlockDown && hasBlockUp)
					PlaceTile(cellCoords, BlockUp);
				else if (hasBlockRight && hasBlockLeft && !hasBlockDown && !hasBlockUp)
					PlaceTile(cellCoords, BlockRightLeft);
				else if (hasBlockRight && !hasBlockLeft && hasBlockDown && !hasBlockUp)
					PlaceTile(cellCoords, BlockRightDown);
				else if (hasBlockRight && !hasBlockLeft && !hasBlockDown && hasBlockUp)
					PlaceTile(cellCoords, BlockUpRight);
				else if (!hasBlockRight && hasBlockLeft && hasBlockDown && !hasBlockUp)
					PlaceTile(cellCoords, BlockDownLeft);
				else if (!hasBlockRight && hasBlockLeft && !hasBlockDown && hasBlockUp)
					PlaceTile(cellCoords, BlockUpLeft);
				else if (hasBlockRight && hasBlockLeft && hasBlockDown && !hasBlockUp)
					PlaceTile(cellCoords, BlockRightDownLeft);
				else if (hasBlockRight && hasBlockLeft && !hasBlockDown && hasBlockUp)
					PlaceTile(cellCoords, BlockUpRightLeft);
				else if (hasBlockRight && !hasBlockLeft && hasBlockDown && hasBlockUp)
					PlaceTile(cellCoords, BlockUpRightDown);
				else if (hasBlockRight && hasBlockLeft && hasBlockDown && hasBlockUp)
					PlaceTile(cellCoords, BlockAll);
				else if (!hasBlockRight && !hasBlockLeft && hasBlockDown && hasBlockUp)
					PlaceTile(cellCoords, BlockUpDown);
				else if (!hasBlockRight && hasBlockLeft && hasBlockDown && hasBlockUp)
					PlaceTile(cellCoords, BlockUpDownLeft);
				else
					EraseCell(cellCoords);
			}
		}
	}

	private bool IsWantedMaterial(Vector2I coords)
	{
		bool isEmpty = _tileset.GetCellSourceId(coords) == -1;
		if (isEmpty)
			return false;
		string wantedMaterial = "";
		if (_tileMaterial == TileMaterial.Stone)
			wantedMaterial = "Stone";
		else if (_tileMaterial == TileMaterial.Flesh)
			wantedMaterial = "Flesh";
		
		TileData data = _tileset.GetCellTileData(coords);

		string material = data.GetCustomData(MaterialDataName).AsString();
		return material == wantedMaterial;
	}

	private void PlaceTile(Vector2I coords, Vector2I tile)
	{
		SetCell(coords, TileID, tile);
	}

	private bool CanHaveBorder(Vector2I coords)
	{
		if (_tileset.GetCellSourceId(coords) == -1)
			return true;
		if (_tileMaterial == TileMaterial.Flesh && _tileset.GetCellTileData(coords).GetCustomData(MaterialDataName).AsString() == "Stone")
			return true;
		return false;
	}
}
