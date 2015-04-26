using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TileTex
{
	//This class enables us to define various textures for tiles.
	public string str;
	public Texture2D tex;
}

public class LayoutTiles : MonoBehaviour {

	static public LayoutTiles S;

	public TextAsset roomsText;  //The rooms.xml file
	public string roomNumber = "0";  //Current room # as a string
	//^ roomNumber as string allows encoding in the XML & rooms 0-F
	public GameObject tilePrefab;  //Prefab for all tiles
	public TileTex[] tileTextures;  //A list of named textures for Tiles
	public GameObject portalPrefab;  //Prefab for the portals between rooms

	public bool ___________________;

	private bool firstRoom = true;  //Is this the first room built
	public PT_XMLReader roomsXMLR;
	public PT_XMLHashList roomsXML;
	public Tile[,] tiles;
	public Transform tileAnchor;

	void Awake()
	{
		S = this;  //Set the singleton for LayouTiles

		//Make a new GameObject to be the TileAnchor (the parent transform of all tiles).
		//This keeps Tiles tidy in the heiarchy pane.
		GameObject tAnc = new GameObject("TileAnchor");
		tileAnchor = tAnc.transform;

		//Read the XML
		roomsXMLR = new PT_XMLReader ();  //Create a PT_XMLRReader
		roomsXMLR.Parse (roomsText.text);  //Parse the Rooms.xml file

		//This next line in the book says it's just roomsXML, but that throws errors so made it roomsXMLR.

		roomsXML = roomsXMLR.xml["xml"][0]["room"];  //Pull all the <room>s

		//Build the 0th room
		BuildRoom (roomNumber);
	}

	/*
	 * Possiably duplicaed, will delete when sure.
	// Build a room based on room number. This is an alternative version of
	//  BuildRoom that grabs roomXML based on  num.
	public void BuildRoom(string rNumStr) {
		PT_XMLHashtable roomHT = null;
		for (int i=0; i<roomsXML.Count; i++) {
			PT_XMLHashtable ht = roomsXML[i];
			if (ht.att("num") == rNumStr) {
				roomHT = ht;
				break;
			}
		}
		if (roomHT == null) {
			Utils.tr("ERROR","LayoutTiles.BuildRoom()",
			         "Room not found: "+rNumStr);
			return;
		}
		BuildRoom(roomHT);
	}
	*/

	//This is the GetTileTex() method that Tile uses
	public Texture2D GetTileTex(string tStr)
	{
		//Search through all the tileTextures for the proper string
		foreach (TileTex tTex in tileTextures)
		{
			if (tTex.str == tStr)
			{
				return(tTex.tex);
			}
		}
		//Return null if nothing was found
		return(null);
	}

	//Build a room based on room number.  This is an alternative version of 
	// BuildRoom that grabs roomXML based on <room> num.
	public void BuildRoom(string rNumStr)
	{
		PT_XMLHashtable roomHT = null;
		for (int i=0; i<roomsXML.Count; i++)
		{
			PT_XMLHashtable ht = roomsXML[i];
			if (ht.att("num") == rNumStr)
			{
				roomHT = ht;
				break;
			}
		}
		if (roomHT == null)
		{
			Utils.tr("ERROR","LayoutTiles.BuildRoom()", "Room not found: "+rNumStr);
			return;
		}

		BuildRoom (roomHT);

	}

	//Build a room from an XML <room> entry
	public void BuildRoom(PT_XMLHashtable room)
	{
		//Destroy old tiles
		foreach (Transform t in tileAnchor)  //Clear out the old tiles
		{
			//^ You can iterate over a transform to get its children
			Destroy(t.gameObject);
		}

		//Move the Mage out od the way
		Mage.S.pos = Vector3.left * 1000;
		//^ This keeps the mafe grom accidentially triggering OnTriggerExit() on
		// a portal.  In my testing, I found that OnTriggerExit was being called 
		// at strange times.
		Mage.S.ClearInput ();  //Cancle any active mouse input and drage

		string rNumStr = room.att("num");

		//Get the texture names for the floors and walls from <room> attributes
		string floorTexStr = room.att("floor");
		string wallTexStr = room.att("wall");
		//Split the room into rows of tiles based on carriage returns in the Rooms.XML file
		string[] roomRows = room.text.Split('\n');
		//Trim tabs from the beginings of lines. However, we're leaving spaces
		//and underscores to allow for non-rectangular rooms.
		for (int i=0; i<roomRows.Length; i++)
		{
			roomRows[i] = roomRows[i].Trim('\t');
		}
		//Clear the tiles Array
		tiles = new Tile[100, 100];  //Arbitrary max room size is 100x100

		//Declare a number of local fields that we'll use later
		Tile ti;
		string type, rawType, tileTexStr; 
		GameObject go;
		int height;
		float maxY = roomRows.Length - 1;
		List<Portal> portals = new List<Portal> ();

		//These loops scan through each tile of each row of the room
		for (int y=0; y<roomRows.Length; y++)
		{
			for (int x=0; x<roomRows[y].Length; x++)
			{
				//Set Defaults
				height = 0;
				tileTexStr = floorTexStr;

				//Get the character representing the tile
				type = rawType = roomRows[y][x].ToString();
				switch (rawType)
				{
				case " ":  //empty space
				case "_":  //empty space
					//just skip over empty space
					continue;
				case ".":  //default wall
					//Keep type="."
					break;
				case "|":  //default wall
					height = 1;
					break;
				default:
					//anything else will be interperteted as floor
					type = ".";
					break;
				}
				//Set the texture for floor or wall based on <room> attributes
				if (type == ".")
				{
					tileTexStr = floorTexStr;
				} else if (type == "|") {
					tileTexStr = wallTexStr;
				}

				//Instiantiate a new TilePrefab
				go = Instantiate(tilePrefab) as GameObject;
				ti = go.GetComponent<Tile>();
				//Set the parent Transform to tileAnchor
				ti.transform.parent = tileAnchor;
				//Set the position of the tile
				ti.pos = new Vector3(x, maxY-y, 0);
				tiles[x,y] = ti;  //Add ti to the tiles 2D Array

				//Set the type, height, and texture of the Tile
				ti.type = type;
				ti.height = height;
				ti.tex = tileTexStr;

				//If the type is still rawType, continue to the next iteration
				if (rawType == type) continue;

				//check for specific entities in the room
				switch (rawType)
				{
				case "X":  //Starting position for the mage
					//Commented out according to book - Mage.S.pos = ti.pos;  //Uses the mage Singelton
					if (firstRoom)
					{
						Mage.S.pos = ti.pos;  //Uses the Mage Singleton
						roomNumber = rNumStr;
						//^ Setting roomNumber now keeps any portals from 
						// moving the mage to them in this first room.
						firstRoom = false;
					}
					break;
				case "0":
				case "1":
				case "2":
				case "3":
				case "4":
				case "5":
				case "6":
				case "7":
				case "8":
				case "9":
				case "A":
				case "B":
				case "C":
				case "D":
				case "E":
				case "F":
					//Instiantiate a portal
					GameObject pGO = Instantiate(portalPrefab) as GameObject;
					Portal p = pGO.GetComponent<Portal>();
					p.pos = ti.pos;
					p.transform.parent = tileAnchor;
					//^ Attaching this to the tileAnchor means that the portal
					// will be destroyed when a new room is built
					p.toRoom = rawType;
					portals.Add(p);
					break;
				}

				//More to come here...
			}
		}

		//Position the Mage
		foreach (Portal p in portals)
		{
			//If p.toRoom is the same as the room nimber the mage just exited, 
			// then the mage should enter this room through this portal
			//Alternativly, if firstRoom == true and there was no X in the 
			// room (as a default Mage starting point), move the Mage to this
			// Portal as a backup measure (if, for instance, you want to just
			// load room number "5")
			if (p.toRoom == roomNumber || firstRoom)
			{
				//^ If there's an X in the room, firstRoom will be set to false
				// by the time the code gets here
				Mage.S.StopWalking();  //Stop any Mage movement
				Mage.S.pos = p.pos;  //Move _Mage from the previous room, so there
				// is no need to rotate her in order for her to enter this room
				//  facing the right direction.
				p.justArrived = true;
				//^ Tell the portal that Mage has just arrived
				firstRoom = false;
				//^ Stops a 2nd Portal in this room from moving the Mage to it
			}
		}

		//Finally assign the roomNumber
		roomNumber = rNumStr;

	}

}
