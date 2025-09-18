using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

public class LSystemController : MonoBehaviour {

    Hashtable ruleHash = new Hashtable(100);

	public float initial_length = 2;
	public float initial_radius = 1.0f;
	StringBuilder start = new StringBuilder("");
	StringBuilder lang = new StringBuilder("");
	GameObject contents;
	GameObject parent;
	List<GameObject> list = new List<GameObject>();
	public float angleToUse = 45f;
	public float baseAngle = 90f;
	public int iterations = 4;
	
	// for drawing lines
	public float lineWidth = 1.0f;
    List<List<Vector3>> allPositions = new List<List<Vector3>>(100);
    List<Color> colors = new List<Color>(100);
    static Material lineMaterial;
    
    // for drawing meshes
	List<Mesh> allMeshes = new List<Mesh>(100);
	public Material meshMaterial;
    

    void Start () {

        // create the line material for 2d openGL drawing
        CreateLineMaterial();

        //variables : 0, 1
        //constants: [, ]
        //axiom  : 0
        //rules  : (1 → 11), (0 → 1[0]0)
        // Second example LSystem from 
        // http://en.wikipedia.org/wiki/L-system
        // start = new StringBuilder("0");
        // ruleHash.Add("1", "11");
        // ruleHash.Add("0", "1[0]0");
        // angleToUse = 45f;
        // run(iterations);
        // print(lang);
        // display2();

        // Weed type plant example: 
        // example use of cylinders with 2d drawing
        // start = new StringBuilder("X");
        // ruleHash.Add("X", "F-[[X]+X]+F[+FX]-X");
        // ruleHash.Add("F", "FF");
        // angleToUse = 35f;
        // run(iterations);
        // print(lang);
        // // 2d display
        // display3();
        // // 3d display - it'll be offset by a bit
        // display4();

        // weird tree with 4 branches
        start = new StringBuilder("F");
        ruleHash.Add("F", "FF![&F][/F][+F][-F]F");
        angleToUse = 45f;
        run(iterations);
        print(lang);
        display5();
    }

    // Get a rule from a given letter that's in our array
    string getRule( string input) {		
		if (ruleHash.ContainsKey(input))
        {
            return (string)ruleHash[input];

        }
		return input;
	}
	
	// Run the lsystem iterations number of times on the start axiom.
	void run(int iterations) {
    	StringBuilder curr = start;
		
    	for (int i = 0; i < iterations; i++) {
        	for (int j = 0; j < curr.Length; j++) {
            	string buff = getRule(curr[j].ToString() );
                curr = curr.Replace(curr[j].ToString(), buff, j, 1);
                j += buff.Length - 1;
        	}
    	}

    	lang = curr;
	}

    // based on example at: https://docs.unity3d.com/ScriptReference/GL.html
    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    // based on example at: https://docs.unity3d.com/ScriptReference/GL.html
    public void OnRenderObject()
    {
        // Apply the line material
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        // Set transformation matrix for drawing to
        // match our transform
        GL.MultMatrix(transform.localToWorldMatrix);

        // Draw lines
        GL.Begin(GL.LINES);
        RenderLineList(allPositions, colors);
        GL.End();
        GL.PopMatrix();
    }

    private void RenderLineList(List<List<Vector3>> lineList, List<Color> colors)
    {
        
        int lineListCount = lineList.Count;

        for (int j = 0; j < lineListCount; j++)
        {
            List<Vector3> line = lineList[j];
            int lineCount = line.Count;
            GL.Color(colors[j]);
            for (int i = 0; i < lineCount-1; i++)
            {
                GL.Vertex(line[i]);
                GL.Vertex(line[i + 1]);
            }
        }
    }

    // The display routine for the weed type plant above
    void display3() {
		
		// to push and pop location and angles
		Stack<float> positions = new Stack<float>(10);
		Stack<float> angles = new Stack<float>(10);

		// current location and angle
		float angle = 0f;
		Vector3 position = new Vector3(0,0,0);
		float posy = 0.0f;
		float posx = 0.0f;

        // location and rotation to draw towards
		Vector3 newPosition;
		Vector2 rotated;
        allPositions.Clear();
        List<Vector3> currentLine = new List<Vector3>(10);

		// start at 0,0,0

		// Apply all the drawing rules to the lsystem string
		for(int i=0; i<lang.Length; i++) {
			string buff = lang[i].ToString();
			switch (buff) {
			case "-" : 
				// Turn left 25
				angle -= angleToUse;
				break;
			case "+" : 
				// Turn right 25
				angle += angleToUse;
				break;
			case "F" : 
				// draw a line 
				posy += initial_length;
				newPosition = new Vector3(position.x, posy, 0);
				rotated = rotateZ (position, new Vector3(position.x,posy,0), angle);
				newPosition = new Vector3(rotated.x,rotated.y,0);
                currentLine.Add(position);
                currentLine.Add(newPosition);
				// set up for the next draw
				position = newPosition;
				posx = newPosition.x;
				posy = newPosition.y;
				
				break;
			case "[" :
				//[: push position and angle
				positions.Push (posy);
				positions.Push (posx);
				float currentAngle = angle;
				angles.Push(currentAngle);
				break;
			case "]" : 
				//]: pop position and angle
				posx = positions.Pop();
				posy = positions.Pop();
				position = new Vector3(posx, posy, 0);
				angle = angles.Pop();
                // every time we pop we need to create
                // a new line segment to draw
                allPositions.Add(currentLine);
                colors.Add(Color.blue);
                currentLine = new List<Vector3>(10);                   
                break;
			default : break;
			}
            if (currentLine.Count > 0)
            {
                allPositions.Add(currentLine);
                colors.Add(Color.green);
            }

        }
        
    }

    // Display routine for 2d examples in the main program
    void display2()
    {

        // to push and pop location and angle
        Stack<float> positions = new Stack<float>();
        Stack<float> angles = new Stack<float>();

        // current angle and position
        float angle = 0f;
        Vector3 position = new Vector3(0, 0, 0);
        float posy = 0.0f;
        float posx = 0.0f;

        // positions to draw towards
        Vector3 newPosition;
        Vector2 rotated;

        // start at 0,0,0
        // create a new object for every line segment
        List<Vector3> currentLine = new List<Vector3>(10);
        List<Vector3> leaf = new List<Vector3>(10);

        // Apply the drawing rules to the string given to us
        for (int i = 0; i < lang.Length; i++)
        {
            string buff = lang[i].ToString();
            switch (buff)
            {
                case "0":
                    // draw a line ending in a leaf
                    posy += initial_length;
                    newPosition = new Vector3(position.x, posy, 0);
                    rotated = rotateZ(position, new Vector3(position.x, posy, 0), angle);
                    newPosition = new Vector3(rotated.x, rotated.y, 0);
                    currentLine.Add(position);
                    currentLine.Add(new Vector3(rotated.x, rotated.y, 0));
                    // set up for the next draw
                    position = newPosition;
                    posx = newPosition.x;
                    posy = newPosition.y;
                    drawCircle(0.45f, 0.45f, position, leaf);
                    allPositions.Add(leaf);
                    leaf = new List<Vector3>(10);
                    colors.Add(Color.magenta);
                    break;
                case "1":
                    // draw a line 
                    posy += initial_length;
                    newPosition = new Vector3(position.x, posy, 0);
                    rotated = rotateZ(position, new Vector3(position.x, posy, 0), angle);
                    newPosition = new Vector3(rotated.x, rotated.y, 0);
                    currentLine.Add(position);
                    currentLine.Add(newPosition);
                    //drawLSystemLine(position, newPosition, line, Color.green);
                    // set up for the next draw
                    position = newPosition;
                    posx = newPosition.x;
                    posy = newPosition.y;
                    //tmp = new GameObject();
                    //line = tmp.AddComponent<LineRenderer>();
                    break;
                case "[":
                    //[: push position and angle, turn left 45 degrees
                    positions.Push(posy);
                    positions.Push(posx);
                    float currentAngle = angle;
                    angles.Push(currentAngle);
                    angle -= angleToUse;
                    break;
                case "]":
                    //]: pop position and angle, turn right 45 degrees
                    posx = positions.Pop();
                    posy = positions.Pop();
                    position = new Vector3(posx, posy, 0);
                    angle = angles.Pop();
                    angle += angleToUse;
                    allPositions.Add(currentLine);
                    colors.Add(Color.green);
                    currentLine = new List<Vector3>(10);
                    break;
                default: break;
            }
            if (currentLine.Count > 0)
            {
                allPositions.Add(currentLine);
                colors.Add(Color.green);
            }

        }
    }

     // Display routine for 3d example in the main program
    void display4()
    {
        // to push and pop location and angle
        Stack<float> positions = new Stack<float>();
        Stack<float> angles = new Stack<float>();
		
        // current angle and position
        float angle = 0f;
        Vector3 position = new Vector3(0, 0, 0);
        float posy = 0;
        float posx = 0.0f;
		
        // positions to draw towards
        Vector3 newPosition;
        Vector2 rotated;
        
        // Apply the drawing rules to the string given to us
        for (int i = 0; i < lang.Length; i++)
        {
            string buff = lang[i].ToString();
            switch (buff)
            {
                case "F":
	                // create the mesh and set it to the terrain variable
	                GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cube);
	                cylinder.transform.position = new Vector3(0, 0, 0);
	                MeshRenderer meshRenderer =cylinder.GetComponent<MeshRenderer>();
	                MeshFilter meshFilter = cylinder.GetComponent<MeshFilter>();
	                meshRenderer.material = meshMaterial;
	                // draw a cylinder
	                meshFilter.mesh = MakeCylinder(8, 2, 0.1f, initial_length);
                    // transform it to where it should be
                    posy += initial_length;
                    newPosition = new Vector3(position.x, posy, 0);
	                if (angle != 0)
	                {
		                cylinder.transform.Rotate(new Vector3(0, 0, 1), -1 * angle);
		                rotated = rotateZ(position, new Vector3(position.x, posy, 0), angle);
		                cylinder.transform.position = newPosition;
		                newPosition = new Vector3(rotated.x, rotated.y, 0);
	                }
	                else
	                {
		                cylinder.transform.position = newPosition;
	                }

                    // set up for the next draw
                    position = newPosition;
                    posx = newPosition.x;
                    posy = newPosition.y;
	                
                    break;
                case "[":
                    //[: push position and angle
                    positions.Push(posy);
                    positions.Push(posx);
                    angles.Push(angle);
                    break;
                case "+":
	                angle += angleToUse;
	                break;
                case "]":
                    //]: pop position and angle
                    posx = positions.Pop();
                    posy = positions.Pop();
                    position = new Vector3(posx, posy, 0);
                    angle = angles.Pop();
                    break;
                case "-":
	                angle -= angleToUse;
	                break;
                default: break;
            }
        }
    }

      // Display routine for 3d example in the main program
    void display5()
    {

        // to push and pop location and angle
        Stack<float> positions = new Stack<float>();
        Stack<float> angles = new Stack<float>();
		Stack<int> anglesToUse = new Stack<int>();
		
        // current angle and position
        float angle = 0f;
        Vector3 position = new Vector3(0, 0, 0);
        float posy = 0;
        float posx = 0.0f;
		int whichDirection = 0;
		
        // positions to draw towards
        Vector3 newPosition;
        Vector3 rotated;
        
        // Apply the drawing rules to the string given to us
        for (int i = 0; i < lang.Length; i++)
        {
            string buff = lang[i].ToString();
            switch (buff)
            {
                case "F":
	                // create the mesh and set it to the terrain variable
	                GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cube);
	                cylinder.transform.position = new Vector3(0, 0, 0);
	                
	                MeshRenderer meshRenderer =cylinder.GetComponent<MeshRenderer>();
	                MeshFilter meshFilter = cylinder.GetComponent<MeshFilter>();
	                meshRenderer.material = meshMaterial;
	                // draw a cylinder
	                meshFilter.mesh = MakeCylinder(8, 2, 0.1f, initial_length);
                    // transform it to where it should be
                    posy += initial_length;
                    newPosition = new Vector3(position.x, posy, 0);
	                if (angle != 0)
	                {
		                if (whichDirection == 0) {
			                cylinder.transform.Rotate(new Vector3(1, 0, 0), -1 * angle);
			                rotated = rotateX(position, new Vector3(position.x, posy, 0), angle);
			                cylinder.transform.position = newPosition;
			                newPosition = new Vector3(rotated.x, rotated.y, rotated.z);
		                }
		                else  if (whichDirection == 1) {
			                cylinder.transform.Rotate(new Vector3(0, 0, 1), -1 * angle);
			                rotated = rotateZ(position, new Vector3(position.x, posy, 0), angle);
			                cylinder.transform.position = newPosition;
			                newPosition = new Vector3(rotated.x, rotated.y, 0);
		                }
	                }
	                else
	                {
		                cylinder.transform.position = newPosition;
	                }
	                
                    // set up for the next draw
                    position = newPosition;
                    posx = newPosition.x;
                    posy = newPosition.y;
	                
                    break;
                case "[":
                    //[: push position and angle
                    positions.Push(posy);
                    positions.Push(posx);
                    angles.Push(angle);
                    anglesToUse.Push(whichDirection);
                    break;
                case "&":
	                angle += baseAngle;
	                whichDirection = 0;
	                break;
                case "/":
	                angle -= baseAngle;
	                whichDirection = 0;
	                break;
                case "]":
                    //]: pop position and angle
                    posx = positions.Pop();
                    posy = positions.Pop();
                    position = new Vector3(posx, posy, 0);
                    angle = angles.Pop();
                    whichDirection = anglesToUse.Pop();
                    break;
                case "+":
	                angle += angleToUse;
	                whichDirection = 1;
	                break;
                case "-":
	                angle -= angleToUse;
	                whichDirection = 1;
	                break;
                default: break;
            }
            
        }
    }

    // make and return a cylinder with appropriate measurements
    Mesh MakeCylinder(float radialDivision, float heightDivision, float radius, float height)
    {
	    Mesh mesh = new Mesh();
	    List<Vector3> vertices = new List<Vector3>(100);
	    List<int> triangles = new List<int>(100);
	    
	    float thetaDiv = 2.0f * 3.14159f / radialDivision;
		float theta = 0;
		float cosTheta =(float)( radius * Math.Cos(theta));
		float sinTheta =(float)( radius * Math.Sin(theta));
		float nextTheta = theta + thetaDiv;
		float cosNextTheta = (float)(radius * Math.Cos(nextTheta));
		float sinNextTheta = (float)(radius * Math.Sin(nextTheta));
		float size = height;
		float startPosition = 0;

		for (int i = 0; i < radialDivision; i++)
		{
			// draw top of cylinder
			addTriangle(vertices, triangles, cosNextTheta, size, sinNextTheta, cosTheta, size, sinTheta, 0, size, 0);
			// draw bottom of cylinder
			addTriangle(vertices, triangles, 0, startPosition, 0, cosTheta, startPosition, sinTheta, cosNextTheta, startPosition,
				sinNextTheta);
			// draw rectangles for the height of the cylinder
			float heightSubDiv = size / heightDivision;
			float minH = startPosition;
			float maxH = startPosition + heightSubDiv;
			for (int j = 0; j < heightDivision; j++)
			{
				addTriangle(vertices, triangles, cosTheta, maxH, sinTheta, cosNextTheta, maxH, sinNextTheta, cosTheta, minH, sinTheta);
				addTriangle(vertices, triangles, cosNextTheta, maxH, sinNextTheta, cosNextTheta, minH, sinNextTheta, cosTheta, minH,
					sinTheta);
				minH = maxH;
				maxH = maxH + heightSubDiv;
			}

			// now go onto the next subdivision
			sinTheta = sinNextTheta;
			cosTheta = cosNextTheta;
			theta = nextTheta;
			nextTheta = nextTheta + thetaDiv;
			sinNextTheta = (float) (radius * Math.Sin(nextTheta));
			cosNextTheta = (float) (radius * Math.Cos(nextTheta));
		}

		mesh.vertices = vertices.ToArray(); 
		mesh.triangles = triangles.ToArray();
		
		// reset the mesh
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	    
    }
    
    public void addTriangle (List<Vector3> vertices, List<int> indices, float x0,float y0,float z0,float x1, float y1,float z1,float x2,float y2,float z2)
    {
	    var nverts = vertices.Count;
    
	    // push first vertex
	    //console.log(nverts + "---" + x0 + ":" + y0 + ":" + z0);
	    vertices.Add(new Vector3(x0,y0,z0));
	    indices.Add(nverts);
	    nverts++;
	    //console.log(nverts + "---" + x1 + ":" + y1 + ":" + z1);
	    // push second vertex
	    vertices.Add(new Vector3(x1,y1,z1));
	    indices.Add(nverts);
	    nverts++;
	    //console.log(nverts + "---" + x2 + ":" + y2 + ":" + z2);
	    // push third vertex
	    vertices.Add(new Vector3(x2,y2,z2));
	    indices.Add(nverts);
	    nverts++;
    }
    
    // rotate a line and return the position after rotation
    // Assumes rotation around the Z axis
    Vector3 rotateZ(Vector3 pivotPoint, Vector3 pointToRotate, float angle) {
   		Vector3 result;
   		float Nx = (pointToRotate.x - pivotPoint.x);
   		float Ny = (pointToRotate.y - pivotPoint.y);
   		angle = -angle * Mathf.PI/180f;
   		result = new Vector3(Mathf.Cos(angle) * Nx - Mathf.Sin(angle) * Ny + pivotPoint.x, 
		    Mathf.Sin(angle) * Nx + Mathf.Cos(angle) * Ny + pivotPoint.y, 0.0f);
   		return result;
	}
   
    // rotate a line and return the position after rotation
	// Assumes rotation around the x axis
	Vector3 rotateX(Vector3 pivotPoint, Vector3 pointToRotate, float angle) {
		Vector3 result;
		float Nz = (pointToRotate.z - pivotPoint.z);
		float Ny = (pointToRotate.y - pivotPoint.y);
		angle = -angle * Mathf.PI/180f;
		result = new Vector3(0, Mathf.Cos(angle) * Ny - Mathf.Sin(angle) * Nz + pivotPoint.y, Mathf.Sin(angle) * Ny + Mathf.Cos(angle) * Nz + pivotPoint.z);
		return result;
	}
	
	// Draw a circle with the given parameters
	// Should probably use different stuff than the default
    void drawCircle(float radiusX, float radiusY, Vector3 center, List<Vector3> line) {

        float x;
        float y;
        float z = 0f;
		int segments = 15;
        float angle = (360f / segments);

        for (int i = 0; i < (segments + 1); i++) {

            x = Mathf.Sin (Mathf.Deg2Rad * angle) * radiusX + center.x;
            y = Mathf.Cos (Mathf.Deg2Rad * angle) * radiusY + center.y;

            line.Add(new Vector3(x, y, 0));
            angle += (360f / segments);

        }

    }
		
	
	// Update is called once per frame
	void Update () {
	
	}
	
}
